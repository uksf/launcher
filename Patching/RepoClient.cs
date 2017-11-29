using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FastRsync.Delta;

namespace Patching {
    public class RepoClient : LogReporter {
        private const string USERNAME = "launcher";
        private const string PASSWORD = "sneakysnek";
        private static Action<int, object> _progressAction;
        private readonly DownloadManager _downloadManager;

        private readonly string _repoFilePath;
        private readonly string _repoLocalPath;
        private readonly string _repoName;
        private ConcurrentBag<RepoAction> _actions;
        private ConcurrentDictionary<string, string> _deltas;

        private CancellationTokenSource _downloadCancellationTokenSource;
        private ConcurrentDictionary<string, bool> _processedSignatures;
        private int _progressIndex;
        private Dictionary<string, string[]> _repoFileDictionary;

        private string _repoPath;

        private ManualResetEvent _waitHandle, _downloadWaitHandle;

        public RepoClient(string path, string localPath, string name, Action<string> logAction, Action<int, object> progressAction) : base(logAction) {
            _repoPath = path;
            _repoName = name;
            _progressAction = progressAction;

            _repoLocalPath = Path.Combine(localPath, _repoName);
            _repoFilePath = Path.Combine(_repoLocalPath, ".repo.urf");
            _repoFileDictionary = new Dictionary<string, string[]>();
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));

            _downloadManager = new DownloadManager();
            DownloadManager.LogEvent += (sender, message) => LogAction(message);
            DownloadManager.ProgressEvent += (sender, progress) => { };
        }

        public event EventHandler<Exception> ErrorEvent, ErrorNoShutdownEvent;
        public event EventHandler<Tuple<string, string, string, string>> UploadEvent;
        public event EventHandler<string> DeleteEvent;

        private void LogAction(string message) {
            LogReporterAction.Invoke(message);
        }

        private void ProgressAction(float progress, string message) {
            _progressAction.Invoke((int) (progress * 100), message);
        }

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string[]> addonPair in from pair in _repoFileDictionary orderby pair.Key select pair) {
                    streamWriter.WriteLine($"{addonPair.Key};{addonPair.Value[0]}:{addonPair.Value[1]}:{addonPair.Value[2]}");
                }
                LogAction("Repo file written");
            }
        }

        public bool CheckLocalRepo(string remoteRepoData, CancellationTokenSource tokenSource) {
            _downloadCancellationTokenSource = tokenSource;
            _waitHandle = new ManualResetEvent(false);
            try {
                if (!Directory.Exists(_repoLocalPath)) {
                    LogAction($"Creating repo location for {_repoName}");
                    Directory.CreateDirectory(_repoLocalPath);
                }
                if (!File.Exists(_repoFilePath)) {
                    LogAction($"Creating repo file for {_repoName}");
                    File.Create(_repoFilePath).Close();
                }
                CleanRepo();

                // Check local addon caches against mods files, and both against remote addons
                _progressIndex = 0;
                _repoFileDictionary = File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
                Dictionary<string, string[]> remoteRepoDictionary = remoteRepoData.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries)
                                                                                  .Select(repoLine => repoLine.Split(';'))
                                                                                  .ToDictionary(values => values[0], values => values[1].Split(':'));
                List<Addon> changedAddons = GetChangedAddons(remoteRepoDictionary);
                LogAction($"{changedAddons.Count} changes");
                if (changedAddons.Count == 0) {
                    WriteRepoFile();
                    ProgressAction(1, "stop");
                    return false;
                }

                // Get actions from changed addons
                _progressIndex = 0;
                _actions = GetAddonActions(changedAddons);
                LogAction($"{_actions.Count} actions");
                if (_actions.Count == 0) {
                    WriteRepoFile();
                    ProgressAction(1, "stop");
                    return false;
                }

                // Process deleted actions
                _progressIndex = 0;
                ProcessDeletedActions();

                // Process and upload and signatures for modified addons
                _progressIndex = 0;
                _processedSignatures = new ConcurrentDictionary<string, bool>();
                _deltas = new ConcurrentDictionary<string, string>();
                ProcessModifiedActions();

                // Get dictionary of added files to download with file sizes
                _downloadWaitHandle = new ManualResetEvent(false);
                foreach (RepoAction action in _actions.Where(action => action is RepoAction.AddedAction)) {
                    _downloadManager.AddDownloadTask(Path.Combine(action.Addon.FolderPath, action.AddonFile), $"{_repoName}/{action.Addon.Name}/{action.AddonFile}",
                                                     _downloadCancellationTokenSource.Token, () => {
                                                         if (_actions.Last(lastAction => lastAction is RepoAction.AddedAction).Equals(action)) _downloadWaitHandle.Set();
                                                     });
                }
                _downloadManager.ProcessDownloadQueue();

                // Wait for deltas to finish processing
                _waitHandle.WaitOne();
                _waitHandle = new ManualResetEvent(false);
                // Get dictionary of modified files to download with file sizes
                foreach (KeyValuePair<string, string> delta in _deltas) {
                    _downloadManager.AddDownloadTask(delta.Key, $"{_repoName}/{delta.Value}", _downloadCancellationTokenSource.Token, () => {
                        ProcessDelta(delta.Key, $"{_repoName}/{delta.Value}");
                        DeleteEvent?.Invoke(this, Path.Combine(_repoName, ".repo", delta.Value));
                        if (_deltas.Last().Equals(delta)) _waitHandle.Set();
                    });
                }
                if (!_downloadManager.IsDownloadQueueEmpty()) _downloadManager.ProcessDownloadQueue();
                _downloadWaitHandle.WaitOne();
                _waitHandle.WaitOne();

                // Validate invalidated addons and write repo file
                HashSet<Addon> invalidatedAddons = new HashSet<Addon>();
                foreach (RepoAction action in _actions.Where(action => File.Exists(Path.Combine(action.Addon.FolderPath, action.AddonFile)))) {
                    if (action is RepoAction.DeletedAction) {
                        action.Addon.RemoveHash(Path.Combine(action.Addon.FolderPath, action.AddonFile));
                    } else {
                        action.Addon.GenerateHash(Path.Combine(action.Addon.FolderPath, action.AddonFile));
                    }
                    invalidatedAddons.Add(action.Addon);
                }
                Parallel.ForEach(invalidatedAddons, addon => addon.GenerateFullHash());
                WriteRepoFile();
                return false;
            } catch (OperationCanceledException) {
                LogAction("Repo check cancelled");
                return false;
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                LogAction($"An error occured during local repo check\n{exception}");
                ErrorEvent?.Invoke(this, exception);
                return true;
            } finally {
                ProgressAction(1, "stop");
                CleanRepo();
            }
        }

        private List<Addon> GetChangedAddons(Dictionary<string, string[]> remoteRepoDictionary) {
            List<Addon> changedAddons = new List<Addon>();
            foreach (KeyValuePair<string, string[]> remoteAddonPair in remoteRepoDictionary) {
                string addonName = Path.GetFileName(remoteAddonPair.Key);
                ProgressAction((float) _progressIndex / remoteRepoDictionary.Count, $"Checking '{addonName}'");
                string localAddon = _repoFileDictionary.Keys.FirstOrDefault(key => key != null && Path.GetFileName(key).Equals(addonName));
                localAddon = string.IsNullOrEmpty(localAddon) ? Path.Combine(_repoPath, addonName) : localAddon;
                if (!Path.GetDirectoryName(localAddon).Equals(_repoPath)) {
                    localAddon = Path.Combine(_repoPath, addonName);
                }
                Addon addon = new Addon(localAddon, new DirectoryInfo(_repoLocalPath));
                if (!Directory.Exists(addon.FolderPath)) {
                    LogAction($"Could not find mod folder for '{addon.Name}'");
                    changedAddons.Add(addon);
                } else {
                    CheckAddonCache(addon, remoteRepoDictionary.Count);
                    if (_repoFileDictionary[addon.FolderPath][0] != remoteAddonPair.Value[0] ||
                        Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) != Convert.ToInt32(remoteAddonPair.Value[1])) {
                        changedAddons.Add(addon);
                    }
                }
                _progressIndex++;
            }
            _progressIndex = 0;
            foreach (KeyValuePair<string, string[]> localDataPair in _repoFileDictionary) {
                string addonName = Path.GetFileName(localDataPair.Key);
                if (changedAddons.Any(addon => addon.Name.Equals(addonName)) || !remoteRepoDictionary.Keys.All(key => key != null && !Path.GetFileName(key).Equals(addonName))) {
                    continue;
                }
                ProgressAction((float) _progressIndex++ / remoteRepoDictionary.Count, $"Deleting '{addonName}'");
                Directory.Delete(localDataPair.Key, true);
                Directory.Delete(Path.Combine(_repoLocalPath, addonName), true);
                File.Delete(Path.Combine(_repoLocalPath, $"{addonName}.urf"));
            }
            return changedAddons;
        }

        private void CheckAddonCache(Addon addon, int progressCount) {
            string[] addonFiles = Directory.GetFiles(addon.FolderPath, "*", SearchOption.AllDirectories);
            long ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            if (!_repoFileDictionary.Keys.Any(key => key != null && Path.GetFileName(key).Equals(addon.Name)) || !File.Exists(Path.Combine(_repoLocalPath, $"{addon.Name}.urf"))) {
                if (!File.Exists(Path.Combine(_repoLocalPath, $"{addon.Name}.urf"))) {
                    LogAction($"Could not find addon.urf '{addon.Name}'");
                    ProgressAction((float) _progressIndex / progressCount, $"Generating cache '{addon.Name}'");
                    addon.GenerateAllHashes(_downloadCancellationTokenSource.Token);
                    addon.GenerateFullHash();
                } else {
                    ProgressAction((float) _progressIndex / progressCount, $"Updating cache '{addon.Name}'");
                    LogAction($"Could not find addon in repo.urf for '{addon.Name}'");
                    addon.GenerateFullHash();
                }
                UpdateAddonCache(addon, addonFiles.Length, ticks);
            }
            Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(_repoLocalPath, $"{addon.Name}.urf")).Select(line => line.Split(';'))
                                                                    .ToDictionary(values => values[0], values => values[1].Split(':'));
            int currentFile = 1;
            foreach (KeyValuePair<string, string[]> addonFilePair in localAddonDictionary) {
                ProgressAction((float) _progressIndex / progressCount, $"Checking cache '{addon.Name}'\nFile {currentFile++} of {localAddonDictionary.Count}");
                FileInfo addonFile = new FileInfo(Path.Combine(addon.FolderPath, addonFilePair.Key));
                if (addonFile.Exists) {
                    if (Convert.ToInt32(addonFilePair.Value[1]) != addonFile.Length || Convert.ToInt64(addonFilePair.Value[2]) != addonFile.LastWriteTime.Ticks) {
                        addon.GenerateHash(addonFile.FullName);
                    }
                } else {
                    addon.RemoveHash(addonFile.FullName);
                }
            }
            currentFile = 1;
            foreach (string addonFile in addonFiles.Where(addonFile => !localAddonDictionary.ContainsKey(addonFile.Replace($"{addon.FolderPath}{Path.DirectorySeparatorChar}", "")))
            ) {
                ProgressAction((float) _progressIndex / progressCount, $"Checking files '{addon.Name}'\nFile {currentFile++} of {addonFiles.Length}");
                addon.GenerateHash(addonFile);
            }
            addon.GenerateFullHash();
            ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            if (_repoFileDictionary.ContainsKey(addon.FolderPath) && _repoFileDictionary[addon.FolderPath][0] == addon.FullHash &&
                Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == addonFiles.Length && localAddonDictionary.Count == addonFiles.Length &&
                Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == localAddonDictionary.Count && Convert.ToInt64(_repoFileDictionary[addon.FolderPath][2]) == ticks) {
                return;
            }
            LogAction($"Addon hash does not match repo.urf for '{addon.Name}'");
            UpdateAddonCache(addon, addonFiles.Length, ticks);
        }

        private void UpdateAddonCache(Addon addon, int files, long ticks) {
            _repoFileDictionary[addon.FolderPath] = new[] {addon.FullHash, Convert.ToString(files), Convert.ToString(ticks)};
            WriteRepoFile();
            _repoFileDictionary = File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
        }

        private ConcurrentBag<RepoAction> GetAddonActions(List<Addon> changedAddons) {
            ConcurrentBag<RepoAction> actions = new ConcurrentBag<RepoAction>();
            Parallel.ForEach(changedAddons, new ParallelOptions {CancellationToken = _downloadCancellationTokenSource.Token}, changedAddon => {
                ProgressAction((float) _progressIndex / changedAddons.Count, $"Finding changes '{changedAddon.Name}'");
                WebClient webClient = new WebClient {Credentials = new NetworkCredential(USERNAME, PASSWORD)};
                Dictionary<string, string[]> remoteAddonDictionary = new Dictionary<string, string[]>();
                try {
                    using (Stream stream = webClient.OpenRead($"ftp://uk-sf.com/{_repoName}/.repo/{changedAddon.Name}.urf")) {
                        using (StreamReader reader = new StreamReader(stream)) {
                            string line;
                            while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
                                remoteAddonDictionary.Add(line.Split(';')[0], line.Split(';')[1].Split(':'));
                            }
                        }
                    }
                } catch (Exception exception) {
                    LogAction($"Could not get remote addon data for '{changedAddon.Name}'");
                    ErrorNoShutdownEvent?.Invoke(this, exception);
                }
                if (!File.Exists(Path.Combine(_repoLocalPath, $"{changedAddon.Name}.urf"))) {
                    foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                        actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                    }
                } else {
                    Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(_repoLocalPath, $"{changedAddon.Name}.urf")).Select(line => line.Split(';'))
                                                                            .ToDictionary(values => values[0], values => values[1].Split(':'));
                    int currentFile = 1;
                    foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                        ProgressAction((float) currentFile / remoteAddonDictionary.Count,
                                       $"Finding changes '{changedAddon.Name}'\n File {currentFile++} of {remoteAddonDictionary.Count}");
                        if (localAddonDictionary.Keys.Any(key => key.Equals(remoteAddonFile.Key))) {
                            if (localAddonDictionary[remoteAddonFile.Key][0] == remoteAddonFile.Value[0] &&
                                localAddonDictionary[remoteAddonFile.Key][1] == remoteAddonFile.Value[1]) {
                                continue;
                            }
                            if (Convert.ToInt64(localAddonDictionary[remoteAddonFile.Key][1]) > 1048576) {
                                actions.Add(new RepoAction.ModifiedAction(changedAddon, remoteAddonFile.Key));
                            } else {
                                actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                            }
                        } else {
                            actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                        }
                    }
                    foreach (KeyValuePair<string, string[]> localAddonFile in localAddonDictionary.Where(localFile => !remoteAddonDictionary.Keys.Contains(localFile.Key))) {
                        actions.Add(new RepoAction.DeletedAction(changedAddon, localAddonFile.Key));
                    }
                }
                _progressIndex++;
            });
            return actions;
        }

        private void ProcessModifiedActions() {
            ConcurrentDictionary<string, string> signatures = new ConcurrentDictionary<string, string>();
            int modifiedActions = _actions.Count(action => action is RepoAction.ModifiedAction);
            Parallel.ForEach(_actions.Where(action => action is RepoAction.ModifiedAction),
                             new ParallelOptions {MaxDegreeOfParallelism = 20, CancellationToken = _downloadCancellationTokenSource.Token}, action => {
                                 ProgressAction((float) _progressIndex++ / modifiedActions, "Calculating signatures");
                                 signatures.TryAdd(Path.Combine(action.Addon.FolderPath, action.AddonFile),
                                                   action.Addon.GenerateSignature(Path.Combine(action.Addon.FolderPath, action.AddonFile)));
                             });
            _progressIndex = 1;
            Parallel.ForEach(signatures, new ParallelOptions {MaxDegreeOfParallelism = 5, CancellationToken = _downloadCancellationTokenSource.Token}, signature => {
                string remotePath = $"ftp://uk-sf.com/{_repoName}/.repo/{Path.GetRandomFileName()}";
                _downloadManager.UploadFile(signature.Value, remotePath, _downloadCancellationTokenSource.Token, () => {
                    _processedSignatures.TryAdd(signature.Key, false);
                    UploadEvent?.Invoke(this,
                                        new Tuple<string, string, string, string>(_repoName, signature.Key, signature.Key.Replace($"{_repoPath}{Path.DirectorySeparatorChar}", ""),
                                                                                  remotePath));
                });
                ProgressAction((float) _progressIndex++ / signatures.Count, "Uploading signatures");
            });
        }

        private void ProcessDeletedActions() {
            int filesToDelete = _actions.Count(action => action is RepoAction.DeletedAction);
            foreach (RepoAction action in _actions.Where(action => action is RepoAction.DeletedAction)) {
                ProgressAction((float) _progressIndex++ / filesToDelete, "Deleting files");
                string filePath = Path.Combine(action.Addon.FolderPath, action.AddonFile);
                File.Delete(filePath);
            }
        }

        public void QueueDelta(string response) {
            try {
                string[] parts = response.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                string addonPath = parts[0];
                string remoteDeltaPath = parts[1];
                _processedSignatures.TryUpdate(addonPath, true, false);
                _deltas.TryAdd(addonPath, $".repo/{remoteDeltaPath}");
                if (_processedSignatures.Values.All(state => state)) {
                    _waitHandle.Set();
                }
            } catch (Exception exception) {
                LogAction($"An error occured processing delta '{response}'\n{exception}");
                ErrorNoShutdownEvent?.Invoke(this, exception);
            }
        }

        private void ProcessDelta(string addonPath, string deltaPath) {
            if (_downloadCancellationTokenSource.Token.IsCancellationRequested) return;
            try {
                DeltaApplier deltaApplier = new DeltaApplier {SkipHashCheck = true};
                using (FileStream basisStream = new FileStream(Path.Combine(_repoPath, addonPath), FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        using (FileStream newFileStream = new FileStream(Path.Combine(_repoPath, $"{addonPath}.urf"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                            deltaApplier.Apply(basisStream, new BinaryDeltaReader(deltaStream), newFileStream);
                        }
                    }
                }
                File.Delete(Path.Combine(_repoPath, addonPath));
                File.Delete(deltaPath);
                File.Copy(Path.Combine(_repoPath, $"{addonPath}.urf"), Path.Combine(_repoPath, $"{addonPath}"));
                File.Delete(Path.Combine(_repoPath, $"{addonPath}.urf"));
            } catch (Exception exception) {
                LogAction($"An error occured processing delta '{addonPath}'\n{exception}");
                ErrorNoShutdownEvent?.Invoke(this, exception);
            }
        }

        public IEnumerable<string> GetRepoMods() => File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')[0]).ToList();

        public void MoveRepo(string newLocation, CancellationToken cancellationToken) {
            try {
                _repoFileDictionary.Clear();
                Dictionary<string, string[]> addonFolders =
                    File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
                int currentIndex = 0;
                foreach (KeyValuePair<string, string[]> addonPair in addonFolders) {
                    if (!Directory.Exists(addonPair.Key)) return;
                    ProgressAction((float) currentIndex / addonFolders.Count, $"Moving '{Path.GetFileName(addonPair.Key)}'");
                    IEnumerable<IGrouping<string, string>> files = Directory.EnumerateFiles(addonPair.Key, "*", SearchOption.AllDirectories).GroupBy(Path.GetDirectoryName);
                    foreach (IGrouping<string, string> folder in files) {
                        string targetFolder = folder.Key.Replace(_repoPath, newLocation);
                        Directory.CreateDirectory(targetFolder);
                        int fileIndex = 0;
                        int index = currentIndex;
                        Parallel.ForEach(folder, new ParallelOptions {CancellationToken = cancellationToken}, file => {
                            ProgressAction((float) index / addonFolders.Count, $"Moving '{Path.GetFileName(addonPair.Key)}'\n{fileIndex++} of {folder.Count()}");
                            File.Move(file, file.Replace(_repoPath, newLocation));
                            File.Delete(file);
                        });
                    }
                    _repoFileDictionary.Add(addonPair.Key.Replace(_repoPath, newLocation), addonPair.Value);
                    Directory.Delete(addonPair.Key, true);
                    currentIndex++;
                }
                CleanRepo();
                _repoPath = newLocation;
                WriteRepoFile();
                ProgressAction(1, "stop");
            } catch (OperationCanceledException) { }
        }

        private void CleanRepo() {
            List<string> files = Directory.EnumerateFiles(_repoLocalPath, "*", SearchOption.AllDirectories).Where(file => !file.Contains(".urf")).ToList();
            if (!Directory.Exists(_repoPath)) return;
            files.AddRange(Directory.EnumerateFiles(_repoPath, "*", SearchOption.AllDirectories).Where(file => file.Contains(".urf")).ToList());
            foreach (string file in files) {
                while (File.Exists(file)) {
                    try {
                        File.Delete(file);
                    } catch (Exception) {
                        Thread.Sleep(100);
                    }
                }
            }
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (string directory in Directory.GetDirectories(_repoPath, "*", SearchOption.AllDirectories)
                                                  .Where(directory => Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 0)) {
                if (Directory.Exists(directory)) {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
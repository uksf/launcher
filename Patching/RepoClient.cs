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
    public class RepoClient : ProgressReporter {
        private const string USERNAME = "launcher";
        private const string PASSWORD = "sneakysnek";
        private static Action<float, string> _progressUpdate;
        private static long _bytesRate;
        private static int _completedDeltas;
        private static DateTime _lastReport = DateTime.Now;
        private readonly string _repoFilePath;
        private ConcurrentBag<RepoAction> _actions;
        private ConcurrentDictionary<string, bool> _deltas;

        private CancellationTokenSource _downloadCancellationTokenSource;
        private Dictionary<string, string[]> _repoFileDictionary;

        public RepoClient(string path, string localPath, string name, Action<string> progressAction) : base(progressAction) {
            RepoPath = path;
            RepoName = name;
            RepoLocalPath = Path.Combine(localPath, RepoName);
            _repoFilePath = string.IsNullOrEmpty(RepoLocalPath) ? Path.Combine(RepoPath, ".repo", ".repo.urf") : Path.Combine(RepoLocalPath, ".repo.urf");
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));
            _repoFileDictionary = new Dictionary<string, string[]>();
        }

        private string RepoPath { get; }
        private string RepoLocalPath { get; }
        public string RepoName { get; }
        public event EventHandler<Tuple<string, string>> UploadEvent;
        public event EventHandler<string> DeleteEvent;

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string[]> addonPair in from pair in _repoFileDictionary orderby pair.Key select pair) {
                    streamWriter.WriteLine($"{addonPair.Key};{addonPair.Value[0]}:{addonPair.Value[1]}:{addonPair.Value[2]}");
                }
                ProgressAction.Invoke("Repo file written");
            }
        }

        public bool CheckLocalRepo(string remoteRepoData, Action<float, string> progressUpdate) {
            _downloadCancellationTokenSource = new CancellationTokenSource();
            try {
                CleanRepo();
                _progressUpdate = progressUpdate;
                if (!Directory.Exists(RepoPath)) {
                    ProgressAction.Invoke($"Creating mods location for {RepoName}");
                    Directory.CreateDirectory(RepoPath);
                }
                if (!Directory.Exists(RepoLocalPath)) {
                    ProgressAction.Invoke($"Creating repo location for {RepoName}");
                    Directory.CreateDirectory(RepoLocalPath);
                }
                if (!File.Exists(_repoFilePath)) {
                    ProgressAction.Invoke($"Creating repo file for {RepoName}");
                    File.Create(_repoFilePath).Close();
                }
                int progressIndex = 0;
                _repoFileDictionary = File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
                Dictionary<string, string[]> remoteRepoDictionary = remoteRepoData.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries)
                                                                                  .Select(repoLine => repoLine.Split(';'))
                                                                                  .ToDictionary(values => values[0], values => values[1].Split(':'));
                List<Addon> changedAddons = new List<Addon>();
                foreach (KeyValuePair<string, string[]> remoteAddonPair in remoteRepoDictionary) {
                    string addonName = Path.GetFileName(remoteAddonPair.Key);
                    _progressUpdate.Invoke((float) progressIndex / remoteRepoDictionary.Count, $"Checking '{addonName}'");
                    string localAddon = _repoFileDictionary.Keys.FirstOrDefault(key => key != null && Path.GetFileName(key).Equals(addonName));
                    localAddon = string.IsNullOrEmpty(localAddon) ? Path.Combine(RepoPath, addonName) : localAddon;
                    // ReSharper disable once PossibleNullReferenceException
                    if (!Path.GetDirectoryName(localAddon).Equals(RepoPath)) {
                        localAddon = Path.Combine(RepoPath, addonName);
                    }
                    Addon addon = new Addon(localAddon, new DirectoryInfo(RepoLocalPath));
                    if (!Directory.Exists(addon.FolderPath)) {
                        ProgressAction.Invoke($"Could not find mod folder for '{addon.Name}'");
                        changedAddons.Add(addon);
                    } else {
                        CheckAddonCache(addon, progressIndex, remoteRepoDictionary.Count);
                        // Local repo.urf is up to date with local files, start checking against remote
                        if (_repoFileDictionary[addon.FolderPath][0] != remoteAddonPair.Value[0] ||
                            Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) != Convert.ToInt32(remoteAddonPair.Value[1])) {
                            changedAddons.Add(addon);
                        }
                    }
                    progressIndex++;
                }
                progressIndex = 0;
                foreach (KeyValuePair<string, string[]> localDataPair in _repoFileDictionary) {
                    string addonName = Path.GetFileName(localDataPair.Key);
                    if (changedAddons.All(addon => !addon.Name.Equals(addonName))) {
                        if (remoteRepoDictionary.Keys.All(key => key != null && !Path.GetFileName(key).Equals(addonName))) {
                            _progressUpdate.Invoke((float) progressIndex++ / remoteRepoDictionary.Count, $"Deleting '{addonName}'");
                            Directory.Delete(localDataPair.Key, true);
                            Directory.Delete(Path.Combine(RepoLocalPath, addonName), true);
                            File.Delete(Path.Combine(RepoLocalPath, $"{addonName}.urf"));
                        }
                    }
                    progressIndex++;
                }
                ProgressAction.Invoke($"{changedAddons.Count} changes");
                if (changedAddons.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, "stop");
                    return false;
                }
                progressIndex = 0;
                _actions = new ConcurrentBag<RepoAction>();
                Parallel.ForEach(changedAddons, changedAddon => {
                    _progressUpdate.Invoke((float) progressIndex++ / changedAddons.Count, "Finding changes");
                    WebClient webClient = new WebClient();
                    Dictionary<string, string[]> remoteAddonDictionary = new Dictionary<string, string[]>();
                    try {
                        using (Stream stream = webClient.OpenRead($"http://www.uk-sf.com/launcher/repos/{RepoName}/.repo/{changedAddon.Name}.urf")) {
                            using (StreamReader reader = new StreamReader(stream)) {
                                string line;
                                while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
                                    remoteAddonDictionary.Add(line.Split(';')[0], line.Split(';')[1].Split(':'));
                                }
                            }
                        }
                    } catch (Exception) {
                        ProgressAction.Invoke($"Could not get remote addon data for '{changedAddon.Name}'");
                    }
                    if (!File.Exists(Path.Combine(RepoLocalPath, $"{changedAddon.Name}.urf"))) {
                        foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                            _actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                        }
                    } else {
                        Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(RepoLocalPath, $"{changedAddon.Name}.urf"))
                                                                                .Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
                        foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                            if (localAddonDictionary.Keys.Any(key => key.Equals(remoteAddonFile.Key))) {
                                if (localAddonDictionary[remoteAddonFile.Key][0] != remoteAddonFile.Value[0] ||
                                    localAddonDictionary[remoteAddonFile.Key][1] != remoteAddonFile.Value[1]) {
                                    _actions.Add(new RepoAction.ModifiedAction(changedAddon, remoteAddonFile.Key));
                                }
                            } else {
                                _actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                            }
                        }
                        foreach (KeyValuePair<string, string[]> localAddonFile in remoteAddonDictionary.Where(localFile => !remoteAddonDictionary.Keys.Contains(localFile.Key))) {
                            _actions.Add(new RepoAction.DeletedAction(changedAddon, localAddonFile.Key));
                        }
                    }
                });
                ProgressAction.Invoke($"{_actions.Count} changes");
                if (_actions.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, "stop");
                    return false;
                }
                _completedDeltas = 0;
                HashSet<Addon> invalidatedAddons = new HashSet<Addon>();
                _deltas = new ConcurrentDictionary<string, bool>();
                Parallel.ForEach(_actions.Where(action => action is RepoAction.ModifiedAction),
                                 new ParallelOptions {MaxDegreeOfParallelism = 20, CancellationToken = _downloadCancellationTokenSource.Token}, UploadFile);
                Parallel.ForEach(_actions, new ParallelOptions {MaxDegreeOfParallelism = 20, CancellationToken = _downloadCancellationTokenSource.Token}, action => {
                    switch (action) {
                        case RepoAction.AddedAction _:
                            string filePath = Path.Combine(action.Addon.FolderPath, action.AddonFile);
                            if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            }
                            DownloadWholeFile($"{action.Addon.Name}/{action.AddonFile}", filePath);
                            invalidatedAddons.Add(action.Addon.GenerateHash(filePath));
                            _completedDeltas++;
                            break;
                        case RepoAction.DeletedAction _:
                            File.Delete(Path.Combine(action.Addon.FolderPath, action.AddonFile));
                            File.Delete(Path.Combine(RepoLocalPath, action.Addon.Name, $"{action.AddonFile}.urf"));
                            _completedDeltas++;
                            break;
                    }
                });
                if (_deltas.Keys.Count > 0) {
                    while (_deltas.Values.Any(state => !state) && !_downloadCancellationTokenSource.IsCancellationRequested) {
                        Task.Delay(100).Wait();
                    }
                }
                foreach (RepoAction action in _actions.Where(action => action is RepoAction.ModifiedAction)) {
                    invalidatedAddons.Add(action.Addon.GenerateHash(Path.Combine(action.Addon.FolderPath, action.AddonFile)));
                }
                Parallel.ForEach(invalidatedAddons, addon => addon.GenerateFullHash());
                WriteRepoFile();
                _progressUpdate.Invoke(1, "stop");
                return true;
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                _progressUpdate.Invoke(1, "stop");
                ProgressAction.Invoke($"An error occured during local repo check\n{exception}");
                throw;
            }
        }

        private void CheckAddonCache(Addon addon, int progressIndex, int progressCount) {
            string[] addonFiles = Directory.GetFiles(addon.FolderPath, "*", SearchOption.AllDirectories);
            long ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            // Does addon exist in repo urf and does addon.urf exist
            if (!_repoFileDictionary.Keys.Any(key => key != null && Path.GetFileName(key).Equals(addon.Name)) || !File.Exists(Path.Combine(RepoLocalPath, $"{addon.Name}.urf"))) {
                if (!File.Exists(Path.Combine(RepoLocalPath, $"{addon.Name}.urf"))) {
                    ProgressAction.Invoke($"Could not find addon.urf '{addon.Name}'");
                    _progressUpdate.Invoke((float) progressIndex / progressCount, $"Generating cache '{addon.Name}'");
                    addon.GenerateAllHashes();
                    addon.GenerateFullHash();
                } else {
                    _progressUpdate.Invoke((float) progressIndex / progressCount, $"Updating cache '{addon.Name}'");
                    ProgressAction.Invoke($"Could not find addon in repo.urf for '{addon.Name}'");
                    addon.GenerateFullHash();
                }
                UpdateAddonCache(addon, addonFiles.Length, ticks);
            }
            Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(RepoLocalPath, $"{addon.Name}.urf")).Select(line => line.Split(';'))
                                                                    .ToDictionary(values => values[0], values => values[1].Split(':'));

            foreach (FileInfo addonFile in from addonFilePair in localAddonDictionary
                                           let addonFile = new FileInfo(Path.Combine(addon.FolderPath, addonFilePair.Key))
                                           where Convert.ToInt32(addonFilePair.Value[1]) != addonFile.Length ||
                                                 Convert.ToInt64(addonFilePair.Value[2]) != addonFile.LastWriteTime.Ticks
                                           select addonFile) {
                addon.GenerateHash(addonFile.FullName);
            }
            addon.GenerateFullHash();
            ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            if (_repoFileDictionary[addon.FolderPath][0] == addon.FullHash && Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == addonFiles.Length &&
                Convert.ToInt64(_repoFileDictionary[addon.FolderPath][2]) == ticks && localAddonDictionary.Count == addonFiles.Length &&
                Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == localAddonDictionary.Count) {
                return;
            }
            ProgressAction.Invoke($"Addon hash does not match repo.urf for '{addon.Name}'");
            UpdateAddonCache(addon, addonFiles.Length, ticks);
        }

        private void UpdateAddonCache(Addon addon, int files, long ticks) {
            _repoFileDictionary[addon.FolderPath] = new[] {addon.FullHash, Convert.ToString(files), Convert.ToString(ticks)};
            WriteRepoFile();
            _repoFileDictionary = File.ReadAllLines(_repoFilePath).Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
        }

        private void DownloadWholeFile(string sourcePath, string filePath) {
            try {
                FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://uk-sf.com/{RepoName}/{sourcePath}");
                ftpWebRequest.KeepAlive = true;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                byte[] buffer = new byte[4096];
                using (FtpWebResponse response = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                    using (FileStream fileStream = File.OpenWrite(filePath)) {
                        using (Stream responseStream = response.GetResponseStream()) {
                            int count = -1;
                            while ((uint) count > 0U) {
                                if (responseStream != null) count = responseStream.Read(buffer, 0, buffer.Length);
                                ReportProgress(count);
                                fileStream.Write(buffer, 0, count);
                            }
                        }
                    }
                }
                ProgressAction.Invoke($"Downloaded '{filePath}'");
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured downloading '{sourcePath}'\n{exception}");
            }
        }

        private void UploadFile(RepoAction action) {
            string signaturePath = action.Addon.GenerateSignature(Path.Combine(action.Addon.FolderPath, action.AddonFile));
            try {
                _deltas.TryAdd(Path.Combine(action.Addon.Name, action.AddonFile), false);
                string remotePath = Path.GetRandomFileName();
                using (WebClient webClient = new WebClient()) {
                    webClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    webClient.UploadFile($"ftp://uk-sf.com/{RepoName}/.repo/{remotePath}", "STOR", signaturePath);
                }
                UploadEvent?.Invoke(this, new Tuple<string, string>(Path.Combine(action.Addon.Name, action.AddonFile), remotePath));
                ProgressAction.Invoke($"Uploaded '{signaturePath}'");
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured uploading '{signaturePath}'\n{exception}");
            } finally {
                File.Delete(signaturePath);
            }
        }

        private void ReportProgress(long change) {
            _bytesRate += change;
            if (DateTime.Now < _lastReport) return;
            _lastReport = DateTime.Now.AddMilliseconds(100);
            _progressUpdate.Invoke((float) _completedDeltas / _actions.Count, $"Downloading files\n{_completedDeltas} of {_actions.Count} ({_bytesRate * 80 / 1024 / 1000} Mbps)");
            _bytesRate = 0L;
        }

        public void ProcessDelta(string response) {
            string[] parts = response.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
            string addonPath = parts[0];
            string remoteDeltaPath = parts[1];
            try {
                string deltaPath = Path.Combine(RepoLocalPath, Path.GetFileName(remoteDeltaPath));
                DownloadWholeFile($".repo/{remoteDeltaPath}", deltaPath);
                DeltaApplier deltaApplier = new DeltaApplier {SkipHashCheck = true};
                using (FileStream basisStream = new FileStream(Path.Combine(RepoPath, addonPath), FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        using (FileStream newFileStream = new FileStream(Path.Combine(RepoPath, $"{addonPath}.urf"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                            deltaApplier.Apply(basisStream, new BinaryDeltaReader(deltaStream), newFileStream);
                        }
                    }
                }
                File.Delete(Path.Combine(RepoPath, addonPath));
                File.Delete(deltaPath);
                File.Copy(Path.Combine(RepoPath, $"{addonPath}.urf"), Path.Combine(RepoPath, $"{addonPath}"));
                File.Delete(Path.Combine(RepoPath, $"{addonPath}.urf"));
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured processing delta '{response}'\n{exception}");
            } finally {
                _deltas.TryUpdate(addonPath, true, false);
                DeleteEvent?.Invoke(this, Path.Combine(RepoName, ".repo", remoteDeltaPath));
                _completedDeltas++;
            }
        }

        private void CleanRepo() {
            foreach (string file in Directory.GetFiles(RepoLocalPath, "*", SearchOption.AllDirectories).Where(file => !file.Contains(".urf"))) {
                File.Delete(file);
            }
            foreach (string file in Directory.GetFiles(RepoPath, "*", SearchOption.AllDirectories).Where(file => file.Contains(".urf"))) {
                File.Delete(file);
            }
        }
    }
}
﻿using System;
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
        private static Action<float, Tuple<string, float>> _progressUpdate;
        private static long _bytesRate;
        private static int _completedDeltas;
        private static DateTime _lastReport = DateTime.Now;
        private readonly string _repoFilePath;
        private readonly string _repoLocalPath;
        public readonly string RepoName;
        private ConcurrentBag<RepoAction> _actions;
        private ConcurrentDictionary<string, bool> _deltas;

        private CancellationTokenSource _downloadCancellationTokenSource;
        private Dictionary<string, string[]> _repoFileDictionary;

        private string _repoPath;

        public RepoClient(string path, string localPath, string name, Action<string> progressAction) : base(progressAction) {
            _repoPath = path;
            RepoName = name;
            _repoLocalPath = Path.Combine(localPath, RepoName);
            _repoFilePath = string.IsNullOrEmpty(_repoLocalPath) ? Path.Combine(_repoPath, ".repo", ".repo.urf") : Path.Combine(_repoLocalPath, ".repo.urf");
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));
            _repoFileDictionary = new Dictionary<string, string[]>();
        }

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

        public bool CheckLocalRepo(string remoteRepoData, Action<float, Tuple<string, float>> progressUpdate, CancellationTokenSource tokenSource) {
            _downloadCancellationTokenSource = tokenSource;
            try {
                CleanRepo();
                _progressUpdate = progressUpdate;
                if (!Directory.Exists(_repoPath)) {
                    ProgressAction.Invoke($"Creating mods location for {RepoName}");
                    Directory.CreateDirectory(_repoPath);
                }
                if (!Directory.Exists(_repoLocalPath)) {
                    ProgressAction.Invoke($"Creating repo location for {RepoName}");
                    Directory.CreateDirectory(_repoLocalPath);
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
                    _progressUpdate.Invoke((float) progressIndex / remoteRepoDictionary.Count, new Tuple<string, float>($"Checking '{addonName}'", 0));
                    string localAddon = _repoFileDictionary.Keys.FirstOrDefault(key => key != null && Path.GetFileName(key).Equals(addonName));
                    localAddon = string.IsNullOrEmpty(localAddon) ? Path.Combine(_repoPath, addonName) : localAddon;
                    if (!Path.GetDirectoryName(localAddon).Equals(_repoPath)) {
                        localAddon = Path.Combine(_repoPath, addonName);
                    }
                    Addon addon = new Addon(localAddon, new DirectoryInfo(_repoLocalPath));
                    if (!Directory.Exists(addon.FolderPath)) {
                        ProgressAction.Invoke($"Could not find mod folder for '{addon.Name}'");
                        changedAddons.Add(addon);
                    } else {
                        CheckAddonCache(addon, progressIndex, remoteRepoDictionary.Count);
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
                    if (changedAddons.Any(addon => addon.Name.Equals(addonName)) ||
                        !remoteRepoDictionary.Keys.All(key => key != null && !Path.GetFileName(key).Equals(addonName))) {
                        continue;
                    }
                    _progressUpdate.Invoke((float) progressIndex++ / remoteRepoDictionary.Count, new Tuple<string, float>($"Deleting '{addonName}'", 0));
                    Directory.Delete(localDataPair.Key, true);
                    Directory.Delete(Path.Combine(_repoLocalPath, addonName), true);
                    File.Delete(Path.Combine(_repoLocalPath, $"{addonName}.urf"));
                }
                ProgressAction.Invoke($"{changedAddons.Count} changes");
                if (changedAddons.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
                    return false;
                }
                progressIndex = 0;
                _actions = new ConcurrentBag<RepoAction>();
                Parallel.ForEach(changedAddons, new ParallelOptions {CancellationToken = _downloadCancellationTokenSource.Token}, changedAddon => {
                    _progressUpdate.Invoke((float) progressIndex / changedAddons.Count, new Tuple<string, float>($"Finding changes '{changedAddon.Name}'", 0));
                    WebClient webClient = new WebClient {Credentials = new NetworkCredential(USERNAME, PASSWORD)};
                    Dictionary<string, string[]> remoteAddonDictionary = new Dictionary<string, string[]>();
                    try {
                        using (Stream stream = webClient.OpenRead($"ftp://uk-sf.com/{RepoName}/.repo/{changedAddon.Name}.urf")) {
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
                    if (!File.Exists(Path.Combine(_repoLocalPath, $"{changedAddon.Name}.urf"))) {
                        foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                            _actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                        }
                    } else {
                        Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(_repoLocalPath, $"{changedAddon.Name}.urf"))
                                                                                .Select(line => line.Split(';')).ToDictionary(values => values[0], values => values[1].Split(':'));
                        int currentFile = 0;
                        foreach (KeyValuePair<string, string[]> remoteAddonFile in remoteAddonDictionary) {
                            _progressUpdate.Invoke((float) progressIndex++ / changedAddons.Count,
                                                   new Tuple<string, float>($"Finding changes '{changedAddon.Name}'", (float) currentFile++ / remoteAddonDictionary.Count));
                            if (localAddonDictionary.Keys.Any(key => key.Equals(remoteAddonFile.Key))) {
                                if (localAddonDictionary[remoteAddonFile.Key][0] == remoteAddonFile.Value[0] &&
                                    localAddonDictionary[remoteAddonFile.Key][1] == remoteAddonFile.Value[1]) {
                                    continue;
                                }
                                if (Convert.ToInt64(localAddonDictionary[remoteAddonFile.Key][1]) > 1048576) {
                                    _actions.Add(new RepoAction.ModifiedAction(changedAddon, remoteAddonFile.Key));
                                } else {
                                    _actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                                }
                            } else {
                                _actions.Add(new RepoAction.AddedAction(changedAddon, remoteAddonFile.Key));
                            }
                        }
                        foreach (KeyValuePair<string, string[]> localAddonFile in localAddonDictionary.Where(localFile => !remoteAddonDictionary.Keys.Contains(localFile.Key))) {
                            _actions.Add(new RepoAction.DeletedAction(changedAddon, localAddonFile.Key));
                        }
                    }
                    progressIndex++;
                });
                ProgressAction.Invoke($"{_actions.Count} actions");
                if (_actions.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
                    return false;
                }
                _completedDeltas = 0;
                HashSet<Addon> invalidatedAddons = new HashSet<Addon>();
                _deltas = new ConcurrentDictionary<string, bool>();
                // TODO: Add total size of files to replace number of files
                Parallel.ForEach(_actions.Where(action => action is RepoAction.ModifiedAction),
                                 new ParallelOptions {MaxDegreeOfParallelism = 5, CancellationToken = _downloadCancellationTokenSource.Token}, UploadFile);
                Parallel.ForEach(_actions.Where(action => action is RepoAction.AddedAction),
                                 new ParallelOptions {MaxDegreeOfParallelism = 5, CancellationToken = _downloadCancellationTokenSource.Token}, action => {
                                     string filePath = Path.Combine(action.Addon.FolderPath, action.AddonFile);
                                     if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                                         Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                                     }
                                     if (!DownloadWholeFile($"{action.Addon.Name}/{action.AddonFile}", filePath)) return;
                                     lock (_actions) {
                                         invalidatedAddons.Add(action.Addon.GenerateHash(filePath));
                                     }
                                     _completedDeltas++;
                                 });
                foreach (RepoAction action in _actions.Where(action => action is RepoAction.DeletedAction)) {
                    string filePath = Path.Combine(action.Addon.FolderPath, action.AddonFile);
                    invalidatedAddons.Add(action.Addon.RemoveHash(Path.Combine(action.Addon.FolderPath, action.AddonFile)));
                    File.Delete(filePath);
                    _completedDeltas++;
                }
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
                _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
                return true;
            } catch (OperationCanceledException) {
                _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
                ProgressAction.Invoke("Repo check cancelled");
                return true;
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
                ProgressAction.Invoke($"An error occured during local repo check\n{exception}");
                return false;
            } finally {
                CleanRepo();
            }
        }

        private void CheckAddonCache(Addon addon, int progressIndex, int progressCount) {
            string[] addonFiles = Directory.GetFiles(addon.FolderPath, "*", SearchOption.AllDirectories);
            long ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            if (!_repoFileDictionary.Keys.Any(key => key != null && Path.GetFileName(key).Equals(addon.Name)) || !File.Exists(Path.Combine(_repoLocalPath, $"{addon.Name}.urf"))) {
                if (!File.Exists(Path.Combine(_repoLocalPath, $"{addon.Name}.urf"))) {
                    ProgressAction.Invoke($"Could not find addon.urf '{addon.Name}'");
                    _progressUpdate.Invoke((float) progressIndex / progressCount, new Tuple<string, float>($"Generating cache '{addon.Name}'", 0));
                    addon.GenerateAllHashes(_downloadCancellationTokenSource.Token);
                    addon.GenerateFullHash();
                } else {
                    _progressUpdate.Invoke((float) progressIndex / progressCount, new Tuple<string, float>($"Updating cache '{addon.Name}'", 0));
                    ProgressAction.Invoke($"Could not find addon in repo.urf for '{addon.Name}'");
                    addon.GenerateFullHash();
                }
                UpdateAddonCache(addon, addonFiles.Length, ticks);
            }
            Dictionary<string, string[]> localAddonDictionary = File.ReadAllLines(Path.Combine(_repoLocalPath, $"{addon.Name}.urf")).Select(line => line.Split(';'))
                                                                    .ToDictionary(values => values[0], values => values[1].Split(':'));
            int currentFile = 0;
            foreach (KeyValuePair<string, string[]> addonFilePair in localAddonDictionary) {
                _progressUpdate.Invoke((float) progressIndex / progressCount,
                                       new Tuple<string, float>($"Checking cache '{addon.Name}'", (float) currentFile++ / localAddonDictionary.Count));
                FileInfo addonFile = new FileInfo(Path.Combine(addon.FolderPath, addonFilePair.Key));
                if (addonFile.Exists) {
                    if (Convert.ToInt32(addonFilePair.Value[1]) != addonFile.Length || Convert.ToInt64(addonFilePair.Value[2]) != addonFile.LastWriteTime.Ticks) {
                        addon.GenerateHash(addonFile.FullName);
                    }
                } else {
                    addon.RemoveHash(addonFile.FullName);
                }
            }
            currentFile = 0;
            foreach (string addonFile in addonFiles.Where(addonFile => !localAddonDictionary.ContainsKey(addonFile.Replace($"{addon.FolderPath}{Path.DirectorySeparatorChar}", "")))
            ) {
                _progressUpdate.Invoke((float) progressIndex / progressCount,
                                       new Tuple<string, float>($"Checking files '{addon.Name}'", (float) currentFile++ / addonFiles.Length));
                addon.GenerateHash(addonFile);
            }
            addon.GenerateFullHash();
            ticks = addonFiles.Length == 0 ? 0 : addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
            if (_repoFileDictionary[addon.FolderPath][0] == addon.FullHash && Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == addonFiles.Length &&
                localAddonDictionary.Count == addonFiles.Length && Convert.ToInt32(_repoFileDictionary[addon.FolderPath][1]) == localAddonDictionary.Count &&
                Convert.ToInt64(_repoFileDictionary[addon.FolderPath][2]) == ticks) {
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

        private bool DownloadWholeFile(string sourcePath, string filePath) {
            try {
                FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://uk-sf.com/{RepoName}/{sourcePath}");
                ftpWebRequest.KeepAlive = true;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                long bytesTotal = ftpWebRequest.GetResponse().ContentLength;
                ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://uk-sf.com/{RepoName}/{sourcePath}");
                ftpWebRequest.KeepAlive = true;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpWebRequest.Timeout = 60000;
                byte[] buffer = new byte[4096];
                using (FtpWebResponse response = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                    using (FileStream fileStream = File.OpenWrite(filePath)) {
                        using (Stream responseStream = response.GetResponseStream()) {
                            int count = -1;
                            long bytesDone = 0;
                            while ((uint) count > 0U) {
                                if (_downloadCancellationTokenSource.Token.IsCancellationRequested) return false;
                                if (responseStream != null) count = responseStream.Read(buffer, 0, buffer.Length);
                                bytesDone += count;
                                ReportProgress(count, bytesDone, bytesTotal);
                                fileStream.Write(buffer, 0, count);
                            }
                        }
                    }
                }
                ProgressAction.Invoke($"Downloaded '{filePath}'");
            } catch (OperationCanceledException) {
                return false;
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured downloading '{sourcePath}'\n{exception}");
                return false;
            }
            return true;
        }

        private void UploadFile(RepoAction action) {
            string signaturePath = action.Addon.GenerateSignature(Path.Combine(action.Addon.FolderPath, action.AddonFile));
            try {
                _deltas.TryAdd(Path.Combine(action.Addon.Name, action.AddonFile), false);
                string remotePath = Path.GetRandomFileName();
                using (WebClient webClient = new WebClient()) {
                    _downloadCancellationTokenSource.Token.Register(webClient.CancelAsync);
                    webClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    webClient.UploadFile(new Uri($"ftp://uk-sf.com/{RepoName}/.repo/{remotePath}"), "STOR", signaturePath);
                }
                UploadEvent?.Invoke(this, new Tuple<string, string>(Path.Combine(action.Addon.Name, action.AddonFile), remotePath));
                ProgressAction.Invoke($"Uploaded '{signaturePath}'");
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured uploading '{signaturePath}'\n{exception}");
            } finally {
                File.Delete(signaturePath);
            }
        }

        private void ReportProgress(long change, long done, long total) {
            _bytesRate += change;
            if (DateTime.Now < _lastReport) return;
            _lastReport = DateTime.Now.AddMilliseconds(100);
            _progressUpdate.Invoke((float) _completedDeltas / _actions.Count,
                                   new Tuple<string, float>($"Downloading files\n{_completedDeltas} of {_actions.Count} ({_bytesRate * 80 / 1024 / 1000} Mbps)",
                                                            (float) done / total));
            _bytesRate = 0L;
        }

        public void ProcessDelta(string response) {
            if (_downloadCancellationTokenSource.Token.IsCancellationRequested) return;
            string[] parts = response.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
            string addonPath = parts[0];
            string remoteDeltaPath = parts[1];
            try {
                string deltaPath = Path.Combine(_repoLocalPath, Path.GetFileName(remoteDeltaPath));
                if (!DownloadWholeFile($".repo/{remoteDeltaPath}", deltaPath)) return;
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
                ProgressAction.Invoke($"An error occured processing delta '{response}'\n{exception}");
            } finally {
                _deltas.TryUpdate(addonPath, true, false);
                DeleteEvent?.Invoke(this, Path.Combine(RepoName, ".repo", remoteDeltaPath));
                _completedDeltas++;
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
                    _progressUpdate.Invoke((float) currentIndex / addonFolders.Count, new Tuple<string, float>($"Moving '{Path.GetFileName(addonPair.Key)}'", 0));
                    IEnumerable<IGrouping<string, string>> files = Directory.EnumerateFiles(addonPair.Key, "*", SearchOption.AllDirectories).GroupBy(Path.GetDirectoryName);
                    foreach (IGrouping<string, string> folder in files) {
                        string targetFolder = folder.Key.Replace(_repoPath, newLocation);
                        Directory.CreateDirectory(targetFolder);
                        int fileIndex = 0;
                        int index = currentIndex;
                        Parallel.ForEach(folder, new ParallelOptions {CancellationToken = cancellationToken}, file => {
                            _progressUpdate.Invoke((float) index / addonFolders.Count,
                                                   new Tuple<string, float>($"Moving '{Path.GetFileName(addonPair.Key)}'", (float) fileIndex++ / folder.Count()));
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
                _progressUpdate.Invoke(1, new Tuple<string, float>("stop", 1));
            } catch (OperationCanceledException) { }
        }

        private void CleanRepo() {
            List<string> files = Directory.EnumerateFiles(_repoLocalPath, "*", SearchOption.AllDirectories).Where(file => !file.Contains(".urf")).ToList();
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
            foreach (string directory in Directory.GetDirectories(_repoPath, "*", SearchOption.AllDirectories)
                                                  .Where(directory => Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 0)) {
                Directory.Delete(directory, true);
            }
        }
    }
}
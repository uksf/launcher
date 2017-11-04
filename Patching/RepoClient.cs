using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FastRsync.Delta;
using FastRsync.Diagnostics;

namespace Patching {
    public class RepoClient : ProgressReporter {
        private const string USERNAME = "launcher";
        private const string PASSWORD = "sneakysnek";
        private static Action<float, string> _progressUpdate;
        private static long _bytesRate;
        private static int _completedDeltas;
        private static DateTime _lastReport = DateTime.Now;
        private readonly Dictionary<string, string> _repoDictionary;
        private readonly string _repoFilePath;
        private ConcurrentBag<RepoAction> _actions;
        private ConcurrentDictionary<string, bool> _deltas;

        private CancellationTokenSource _downloadCancellationTokenSource;
        private Dictionary<string, string> _remoteRepoDictionary;

        public RepoClient(string path, string localPath, string name, Action<string> progressAction) : base(progressAction) {
            RepoPath = path;
            RepoName = name;
            RepoLocalPath = Path.Combine(localPath, RepoName);
            _repoFilePath = string.IsNullOrEmpty(RepoLocalPath) ? Path.Combine(RepoPath, ".repo", ".repo.urf") : Path.Combine(RepoLocalPath, ".repo.urf");
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));
            _repoDictionary = new Dictionary<string, string>();
        }

        private string RepoPath { get; }
        private string RepoLocalPath { get; }
        public string RepoName { get; }
        public event EventHandler<Tuple<string, string>> UploadEvent;
        public event EventHandler<string> DeleteEvent;

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string> dataPair in from pair in _repoDictionary orderby pair.Key select pair) {
                    Addon addon = new Addon(dataPair.Key, new DirectoryInfo(RepoLocalPath));
                    addon.GenerateSignaturesChecksum();
                    string[] addonFiles = Directory.GetFiles(dataPair.Key, "*", SearchOption.AllDirectories);
                    string ticks = addonFiles.Length == 0 ? "" : Convert.ToString(addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks);
                    streamWriter.WriteLine($"{addon.FolderPath};{addon.SignaturesCheckSum}:{addonFiles.Length}:{ticks}");
                }
                ProgressAction.Invoke("Repo file written");
            }
        }

        public bool CheckLocalRepo(string remoteRepoData, Action<float, string> progressUpdate) {
            _downloadCancellationTokenSource = new CancellationTokenSource();
            try {
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
                _repoDictionary.Clear();
                Dictionary<string, string> localRepoDictionary =
                    File.ReadAllLines(_repoFilePath).Select(repoLine => repoLine.Split(';')).ToDictionary(values => values[0], values => values[1]);
                _remoteRepoDictionary = remoteRepoData.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries).Select(repoLine => repoLine.Split(';'))
                                                      .ToDictionary(values => values[0], values => values[1]);
                List<Addon> changedAddons = new List<Addon>();
                foreach (KeyValuePair<string, string> remoteDataPair in _remoteRepoDictionary) {
                    string addonName = Path.GetFileName(remoteDataPair.Key);
                    _progressUpdate.Invoke((float) progressIndex / _remoteRepoDictionary.Count, $"Checking '{addonName}'");
                    if (addonName != null) {
                        string localAddon = localRepoDictionary.Keys.FirstOrDefault(key => key != null && Path.GetFileName(key).Equals(addonName));
                        localAddon = string.IsNullOrEmpty(localAddon) ? Path.Combine(RepoPath, addonName) : localAddon;
                        // ReSharper disable once PossibleNullReferenceException
                        if (!Path.GetDirectoryName(localAddon).Equals(RepoPath)) {
                            localAddon = Path.Combine(RepoPath, addonName);
                        }
                        Addon addon = new Addon(localAddon, new DirectoryInfo(RepoLocalPath));
                        if (Directory.Exists(addon.FolderPath) && Directory.Exists(Path.Combine(RepoLocalPath, addon.Name)) &&
                            File.Exists(Path.Combine(RepoLocalPath, $"{addon.Name}.urf"))) {
                            if (localRepoDictionary.ContainsKey(localAddon)) {
                                string[] localData = localRepoDictionary[localAddon].Split(':');
                                string[] remoteData = remoteDataPair.Value.Split(':');
                                if (localData[0] == remoteData[0] && Convert.ToInt32(localData[1]) == Convert.ToInt32(remoteData[1])) {
                                    string[] localFiles = Directory.GetFiles(localAddon, "*", SearchOption.AllDirectories);
                                    string[] signatureFiles = Directory.GetFiles(Path.Combine(RepoLocalPath, addon.Name), "*", SearchOption.AllDirectories);
                                    if (Convert.ToInt32(localData[1]) == localFiles.Length && Convert.ToInt32(localData[1]) == signatureFiles.Length &&
                                        localFiles.Length == signatureFiles.Length) {
                                        long ticks = localFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks;
                                        if (Convert.ToInt64(localData[2]) == ticks) {
                                            Dictionary<string, string> localAddonDictionary =
                                                File.ReadAllLines(Path.Combine(RepoLocalPath, $"{addon.Name}.urf")).Select(line => line.Split(';'))
                                                    .ToDictionary(values => values[0], values => values[1]);
                                            WebClient webClient = new WebClient();
                                            Dictionary<string, string> remoteAddonDictionary = new Dictionary<string, string>();
                                            try {
                                                using (Stream stream = webClient.OpenRead($"http://www.uk-sf.com/launcher/repos/{RepoName}/.repo/{addon.Name}.urf")) {
                                                    using (StreamReader reader = new StreamReader(stream)) {
                                                        string line;
                                                        while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
                                                            remoteAddonDictionary.Add(line.Split(';')[0], line.Split(';')[1]);
                                                        }
                                                    }
                                                }
                                            } catch (Exception) {
                                                ProgressAction.Invoke($"Could not get remote addon data for '{addon.Name}'");
                                            }
                                            string localAddonHash = Utility.ShaFromDictionary(localAddonDictionary);
                                            string remoteAddonHash = Utility.ShaFromDictionary(remoteAddonDictionary);
                                            addon.GenerateSignaturesChecksum();
                                            if (addon.SignaturesCheckSum == localData[0] && addon.SignaturesCheckSum == remoteData[0] &&
                                                addon.SignaturesCheckSum == localAddonHash && localAddonHash == localData[0] && localAddonHash == remoteData[0] &&
                                                remoteAddonHash == localData[0] && remoteAddonHash == addon.SignaturesCheckSum && remoteAddonHash == localAddonHash) {
                                                ProgressAction.Invoke($"Full pass for '{addon.Name}'");
                                                _repoDictionary.Add(addon.FolderPath, localRepoDictionary[localAddon]);
                                            } else {
                                                ProgressAction.Invoke($"Hash mismatch for '{addon.Name}'");
                                                changedAddons.Add(addon);
                                                _repoDictionary.Add(addon.FolderPath, localRepoDictionary[localAddon]);
                                            }
                                        } else {
                                            ProgressAction.Invoke($"File last write does not match local file last write for '{addon.Name}'");
                                            changedAddons.Add(addon);
                                            _repoDictionary.Add(addon.FolderPath, localRepoDictionary[localAddon]);
                                        }
                                    } else {
                                        ProgressAction.Invoke($"Mod file count and/or signature file count do not match local file count '{addon.Name}'");
                                        changedAddons.Add(addon);
                                        _repoDictionary.Add(addon.FolderPath, localRepoDictionary[localAddon]);
                                    }
                                } else {
                                    ProgressAction.Invoke($"Local hash does not match remote hash or local file count does not match remote file counts for '{addon.Name}'");
                                    changedAddons.Add(addon);
                                    _repoDictionary.Add(addon.FolderPath, localRepoDictionary[localAddon]);
                                }
                            } else {
                                ProgressAction.Invoke($"Could not find key in repo file for '{addon.Name}'");
                                changedAddons.Add(addon);
                                _repoDictionary.Add(addon.FolderPath, remoteDataPair.Value);
                            }
                        } else {
                            ProgressAction.Invoke($"Could not find mods folder, repo folder, or repo file for '{addon.Name}'");
                            changedAddons.Add(addon);
                            _repoDictionary.Add(addon.FolderPath, remoteDataPair.Value);
                        }
                    }
                    progressIndex++;
                }
                progressIndex = 0;
                foreach (KeyValuePair<string, string> localDataPair in _repoDictionary) {
                    string addonName = Path.GetFileName(localDataPair.Key);
                    if (changedAddons.All(addon => !addon.Name.Equals(addonName))) {
                        if (_remoteRepoDictionary.Keys.All(key => key != null && !Path.GetFileName(key).Equals(addonName))) {
                            _progressUpdate.Invoke((float) progressIndex / _remoteRepoDictionary.Count, $"Deleting '{addonName}'");
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
                _actions = new ConcurrentBag<RepoAction>();
                progressIndex = 0;
                Parallel.ForEach(changedAddons, changedAddon => {
                    _progressUpdate.Invoke((float) progressIndex / changedAddons.Count, "Finding changes\nThis may take a minute or two");
                    if (Directory.Exists(changedAddon.FolderPath)) {
                        if (Directory.Exists(Path.Combine(RepoLocalPath, changedAddon.Name))) {
                            //changedAddon.GenerateAllSignatures();
                            if (File.Exists(Path.Combine(RepoLocalPath, $"{changedAddon.Name}.urf"))) {
                                string remoteKey = _remoteRepoDictionary.Keys.First(key => key != null && Path.GetFileName(key).Equals(changedAddon.Name));
                                if (changedAddon.SignaturesCheckSum != _remoteRepoDictionary[remoteKey].Split(':')[0]) {
                                    Dictionary<string, string> localAddonDictionary =
                                        File.ReadAllLines(Path.Combine(RepoLocalPath, $"{changedAddon.Name}.urf")).Select(line => line.Split(';'))
                                            .ToDictionary(values => values[0], values => values[1]);
                                    WebClient webClient = new WebClient();
                                    Dictionary<string, string> remoteAddonDictionary = new Dictionary<string, string>();
                                    try {
                                        using (Stream stream = webClient.OpenRead($"http://www.uk-sf.com/launcher/repos/{RepoName}/.repo/{changedAddon.Name}.urf")) {
                                            using (StreamReader reader = new StreamReader(stream)) {
                                                string line;
                                                while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
                                                    remoteAddonDictionary.Add(line.Split(';')[0], line.Split(';')[1]);
                                                }
                                            }
                                        }
                                    } catch (Exception) {
                                        ProgressAction.Invoke($"Could not get remote addon data for '{changedAddon.Name}'");
                                    }
                                    foreach (KeyValuePair<string, string> localDataPair in localAddonDictionary) {
                                        if (remoteAddonDictionary.Keys.Any(key => key.Equals(localDataPair.Key))) {
                                            if (localDataPair.Value == remoteAddonDictionary[localDataPair.Key]) continue;
                                            _actions.Add(new RepoAction.ModifiedAction(changedAddon, localDataPair.Key));
                                        } else {
                                            _actions.Add(new RepoAction.DeletedAction(changedAddon, localDataPair.Key));
                                        }
                                    }
                                    foreach (KeyValuePair<string, string> remoteDataPair in remoteAddonDictionary.Where(remoteDataPair =>
                                                                                                                            !localAddonDictionary.Keys.Contains(remoteDataPair.Key))
                                    ) {
                                        _actions.Add(new RepoAction.AddedAction(changedAddon, remoteDataPair.Key));
                                    }
                                }
                            }
                        } else {
                            Directory.CreateDirectory(Path.Combine(RepoLocalPath, changedAddon.Name));
                            changedAddon.GenerateAllSignatures();
                        }
                    } else {
                        Directory.CreateDirectory(changedAddon.FolderPath);
                        changedAddon.GenerateAllSignatures();
                    }
                    progressIndex++;
                });
                ProgressAction.Invoke($"{_actions.Count} signature changes");
                if (_actions.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, "stop");
                    return false;
                }
                _completedDeltas = 0;
                Parallel.ForEach(_actions, new ParallelOptions {MaxDegreeOfParallelism = 20, CancellationToken = _downloadCancellationTokenSource.Token}, action => {
                    switch (action) {
                        case RepoAction.AddedAction _:
                            string filePath = Path.Combine(action.Addon.FolderPath, action.SignatureFile);
                            if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            }
                            DownloadWholeFile($"{action.Addon.Name}/{action.SignatureFile}", filePath);
                            action.Addon.GenereateSignature(filePath);
                            _completedDeltas++;
                            break;
                        case RepoAction.DeletedAction _:
                            File.Delete(Path.Combine(action.Addon.FolderPath, action.SignatureFile));
                            File.Delete(Path.Combine(RepoLocalPath, action.Addon.Name, $"{action.SignatureFile}.urf"));
                            _completedDeltas++;
                            break;
                    }
                });
                _deltas = new ConcurrentDictionary<string, bool>();
                foreach (RepoAction action in _actions.Where(action => action is RepoAction.ModifiedAction)) {
                    UploadFile(action);
                }
                if (_deltas.Keys.Count > 0) {
                    while (_deltas.Values.Any(state => !state) && !_downloadCancellationTokenSource.IsCancellationRequested) {
                        Task.Delay(50);
                    }
                }
                foreach (RepoAction action in _actions.Where(action => action is RepoAction.ModifiedAction)) {
                    action.Addon.GenereateSignature(Path.Combine(action.Addon.FolderPath, action.SignatureFile));
                }
                WriteRepoFile();
                _progressUpdate.Invoke(1, "stop");
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                _progressUpdate.Invoke(1, "stop");
                ProgressAction.Invoke($"An error occured during local repo check\n{exception}");
                throw;
            }
            return true;
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
            string signaturePath = Path.Combine(RepoLocalPath, action.Addon.Name, $"{action.SignatureFile}.urf");
            try {
                _deltas.TryAdd(signaturePath, false);
                string remotePath = Path.GetRandomFileName();
                using (WebClient webClient = new WebClient()) {
                    webClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    webClient.UploadFile($"ftp://uk-sf.com/{RepoName}/.repo/{remotePath}", "STOR", signaturePath);
                }
                UploadEvent?.Invoke(this, new Tuple<string, string>(Path.Combine(action.Addon.Name, action.SignatureFile), remotePath));
                ProgressAction.Invoke($"Uploaded '{signaturePath}'");
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured uploading '{signaturePath}'\n{exception}");
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
                        using (FileStream newFileStream = new FileStream(Path.Combine(RepoPath, $"{addonPath}.TEMP"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                            deltaApplier.Apply(basisStream, new BinaryDeltaReader(deltaStream, new ConsoleProgressReporter()), newFileStream);
                        }
                    }
                }
                File.Delete(Path.Combine(RepoPath, addonPath));
                File.Delete(deltaPath);
                File.Copy(Path.Combine(RepoPath, $"{addonPath}.TEMP"), Path.Combine(RepoPath, $"{addonPath}"));
                File.Delete(Path.Combine(RepoPath, $"{addonPath}.TEMP"));
            } catch (Exception exception) {
                ProgressAction.Invoke($"An error occured processing delta '{response}'\n{exception}");
            }
            _deltas.TryUpdate(Path.Combine(RepoLocalPath, $"{addonPath}.urf"), true, false);
            DeleteEvent?.Invoke(this, Path.Combine(RepoName, ".repo", remoteDeltaPath));
            _completedDeltas++;
        }
    }
}
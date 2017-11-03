using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Patching {
    public class RepoClient : ProgressReporter {
        private static Action<float, string> _progressUpdate;
        private readonly string _repoFilePath;
        private ConcurrentBag<string> _changedSignatures;

        private CancellationTokenSource _downloadCancellationTokenSource;
        private readonly Dictionary<string, string> _repoDictionary;
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
        private string RepoName { get; }

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
                Dictionary<string, string> localRepoDictionary = File.ReadAllLines(_repoFilePath).Select(repoLine => repoLine.Split(';')).ToDictionary(values => values[0], values => values[1]);
                _remoteRepoDictionary = remoteRepoData.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries).Select(repoLine => repoLine.Split(';'))
                                                      .ToDictionary(values => values[0], values => values[1]);
                List<AddonNew> changedAddons = new List<AddonNew>();
                foreach (KeyValuePair<string, string> remoteDataPair in _remoteRepoDictionary) {
                    string addonName = Path.GetFileName(remoteDataPair.Key);
                    _progressUpdate.Invoke((float) progressIndex / _remoteRepoDictionary.Count, $"Checking '{addonName}'");
                    if (addonName != null) {
                        string localAddon = localRepoDictionary.Keys.FirstOrDefault(key => key != null && Path.GetFileName(key).Equals(addonName));
                        localAddon = string.IsNullOrEmpty(localAddon) ? Path.Combine(RepoPath, addonName) : localAddon;
                        AddonNew addon = new AddonNew(localAddon);
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
                                            addon.GenerateSignaturesChecksum(new DirectoryInfo(RepoLocalPath));
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
                _changedSignatures = new ConcurrentBag<string>();
                progressIndex = 0;
                foreach (AddonNew changedAddon in changedAddons) {
                    _progressUpdate.Invoke((float) progressIndex / changedAddons.Count, "Finding changes");
                    if (Directory.Exists(changedAddon.FolderPath)) {
                        if (Directory.Exists(Path.Combine(RepoLocalPath, changedAddon.Name))) {
                            changedAddon.GenerateAllSignatures(new DirectoryInfo(RepoLocalPath));
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
                                            // Add signature path as modified
                                            _changedSignatures.Add(localDataPair.Key);
                                            ProgressAction.Invoke($"Will modify '{localDataPair.Key}'");
                                        } else {
                                            // Add signature path as deleted
                                            _changedSignatures.Add(localDataPair.Key);
                                            ProgressAction.Invoke($"Will delete '{localDataPair.Key}'");
                                        }
                                    }
                                    foreach (KeyValuePair<string, string> remoteDataPair in remoteAddonDictionary.Where(remoteDataPair =>
                                                                                                                            !localAddonDictionary.Keys.Contains(remoteDataPair.Key))) {
                                        // Add signature path as added
                                        _changedSignatures.Add(remoteDataPair.Key);
                                        ProgressAction.Invoke($"Will add '{remoteDataPair.Key}'");
                                    }
                                }
                            }
                        } else {
                            Directory.CreateDirectory(Path.Combine(RepoLocalPath, changedAddon.Name));
                            changedAddon.GenerateAllSignatures(new DirectoryInfo(RepoLocalPath));
                        }
                    } else {
                        Directory.CreateDirectory(changedAddon.FolderPath);
                        changedAddon.GenerateAllSignatures(new DirectoryInfo(RepoLocalPath));
                    }
                    progressIndex++;
                }
                ProgressAction.Invoke($"{_changedSignatures.Count} signature changes");
                if (_changedSignatures.Count == 0) {
                    WriteRepoFile();
                    _progressUpdate.Invoke(1, "stop");
                    return false;
                }
                WriteRepoFile();
                _progressUpdate.Invoke(1, "stop");
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                _progressUpdate.Invoke(1, "stop");
                ProgressAction.Invoke($"An error occured during local repo check.\n{exception}");
                throw;
            }
            return true;
        }


        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string> dataPair in from pair in _repoDictionary orderby pair.Key select pair) {
                    AddonNew addon = new AddonNew(dataPair.Key);
                    addon.GenerateSignaturesChecksum(new DirectoryInfo(RepoLocalPath));
                    string[] addonFiles = Directory.GetFiles(dataPair.Key, "*", SearchOption.AllDirectories);
                    string ticks = addonFiles.Length == 0 ? "" : Convert.ToString(addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks);
                    streamWriter.WriteLine($"{addon.FolderPath};{addon.SignaturesCheckSum}:{addonFiles.Length}:{ticks}");
                }
                ProgressAction.Invoke("Repo file written");
            }
        }

        /*public void GetChanges(AddonNew addon) {
            DeltaBuilder delta = new DeltaBuilder { ProgressReport = new ConsoleProgressReporter() };
            using (FileStream updatedStream = new FileStream(updatedPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signaturePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        delta.BuildDelta(updatedStream, new SignatureReader(signatureStream, delta.ProgressReport),
                                         new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
                    }
                }
            }
        }*/
    }
}
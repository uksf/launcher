using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Patching {
    public class RepoServer : ProgressReporter {
        private readonly Dictionary<string, string> _repoFileDictionary;
        private readonly string _repoFilePath;

        public RepoServer(string path, string name, Action<string> progressAction) : base(progressAction) {
            RepoPath = path;
            RepoName = name;
            _repoFilePath = Path.Combine(RepoPath, ".repo", ".repo.urf");
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));
            _repoFileDictionary = new Dictionary<string, string>();
        }

        private string RepoPath { get; }
        public string RepoName { get; }

        public void CreateRepo() {
            ProgressAction.Invoke($"Creating directory '{RepoPath}'");
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(RepoPath, ".repo"));
            if (repoDirectory.Exists) {
                repoDirectory.Delete(true);
                repoDirectory.Refresh();
            }
            ProgressAction.Invoke("Creating .repo folder");
            repoDirectory.Create();
            foreach (AddonNew addon in GetAddonFolders().Select(addonFolder => new AddonNew(addonFolder.FullName))) {
                ProgressAction.Invoke($"Processing addon '{addon.Name}'");
                addon.GenerateAllSignatures(repoDirectory);
                _repoFileDictionary.Add(addon.FolderPath, addon.SignaturesCheckSum);
            }
            if (_repoFileDictionary.Count == 0) {
                throw new Exception("No addons processed");
            }
            WriteRepoFile();
        }

        public void UpdateRepo() {
            if (!Directory.Exists(RepoPath)) {
                throw new Exception($"Repo '{RepoName}' does not exist");
            }
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(RepoPath, ".repo"));
            if (!repoDirectory.Exists) {
                throw new Exception("Repo file does not exist");
            }
            Dictionary<string, string> currentFileDictionary = File.ReadAllLines(Path.Combine(RepoPath, ".repo", ".repo.urf")).Select(line => line.Split(';'))
                                                                   .ToDictionary(values => values[0], values => values[1]);
            List<AddonNew> addons = new List<AddonNew>();
            foreach (DirectoryInfo addonFolder in GetAddonFolders()) {
                if (string.IsNullOrEmpty(currentFileDictionary.Keys.FirstOrDefault(key => string.Equals(key, addonFolder.FullName, StringComparison.InvariantCultureIgnoreCase)))) {
                    ProgressAction.Invoke($"Found new addon '{addonFolder.Name}'");
                    AddonNew addon = new AddonNew(addonFolder.FullName);
                    addons.Add(addon);
                } else {
                    ProgressAction.Invoke($"Checking for changes in '{addonFolder.Name}'");
                    string[] addonFiles = Directory.GetFiles(addonFolder.FullName, "*", SearchOption.AllDirectories);
                    string[] signatureFiles = Directory.GetFiles(Path.Combine(repoDirectory.FullName, Path.GetFileName(addonFolder.FullName)), "*", SearchOption.AllDirectories);
                    string[] currentAddonData = currentFileDictionary[addonFolder.FullName].Split(':');
                    if (!File.Exists(Path.Combine(repoDirectory.FullName, $"{Path.GetFileName(addonFolder.FullName)}.urf"))) {
                        ProgressAction.Invoke($"\tAddon repo file does not exist for addon '{addonFolder.Name}'");
                    } else {
                        Dictionary<string, string> currentSignatureDictionary =
                            File.ReadAllLines(Path.Combine(repoDirectory.FullName, $"{Path.GetFileName(addonFolder.FullName)}.urf")).Select(line => line.Split(';'))
                                .ToDictionary(values => values[0], values => values[1]);
                        if (!Directory.Exists(Path.Combine(repoDirectory.FullName, Path.GetFileName(addonFolder.FullName)))) {
                            ProgressAction.Invoke($"\tRepo directory does not exist for addon '{addonFolder.Name}'");
                        } else if (currentAddonData.Length != 3) {
                            ProgressAction.Invoke($"\tRepo file elements differ for addon '{addonFolder.Name}'");
                        } else if (Convert.ToInt32(currentAddonData[1]) != addonFiles.Length) {
                            ProgressAction.Invoke($"\tNumber of files differ for addon '{addonFolder.Name}'");
                        } else if (Convert.ToInt32(currentAddonData[1]) != signatureFiles.Length || signatureFiles.Length != addonFiles.Length ||
                                   signatureFiles.Length != currentSignatureDictionary.Values.Count) {
                            ProgressAction.Invoke($"\tNumber of signatures/files differs for addon '{addonFolder.Name}'");
                        } else if (Convert.ToInt64(currentAddonData[2]) != addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks) {
                            ProgressAction.Invoke($"\tLast write time differs for addon '{addonFolder.Name}'");
                        } else if (!currentAddonData[0].Equals(Utility.ShaFromDictionary(currentSignatureDictionary))) {
                            ProgressAction
                                .Invoke($"\tSignature files hash is different for addon '{addonFolder.Name}'.\n\tRepo file: {currentAddonData[0]}\n\tAddon repo file : {Utility.ShaFromDictionary(currentSignatureDictionary)}");
                        } else {
                            _repoFileDictionary.Add(addonFolder.FullName, currentAddonData[0]);
                            continue;
                        }
                    }
                    AddonNew addon = new AddonNew(addonFolder.FullName);
                    addons.Add(addon);
                }
            }
            foreach (AddonNew addon in addons) {
                ProgressAction.Invoke($"Processing addon '{addon.Name}'");
                addon.GenerateAllSignatures(repoDirectory);
                _repoFileDictionary.Add(addon.FolderPath, addon.SignaturesCheckSum);
            }
            if (_repoFileDictionary.Count == 0) {
                ProgressAction.Invoke("No addons changed");
            } else {
                foreach (KeyValuePair<string, string> pair in currentFileDictionary.Where(pair => !_repoFileDictionary.ContainsKey(pair.Key))) {
                    ProgressAction.Invoke($"Addon deleted '{pair.Key}'");
                    Directory.Delete(Path.Combine(RepoPath, ".repo", Path.GetFileName(pair.Key)), true);
                    File.Delete(Path.Combine(RepoPath, ".repo", $"{Path.GetFileName(pair.Key)}.urf"));
                }
            }
            WriteRepoFile();
        }

        private IEnumerable<DirectoryInfo> GetAddonFolders() {
            List<DirectoryInfo> addonFolders = new DirectoryInfo(RepoPath).EnumerateDirectories("@*").ToList();
            if (addonFolders.Count == 0) {
                throw new Exception("There are no addons in this location");
            }
            return addonFolders;
        }

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string> keyValuePair in from pair in _repoFileDictionary orderby pair.Key select pair) {
                    string[] addonFiles = Directory.GetFiles(keyValuePair.Key, "*", SearchOption.AllDirectories);
                    string ticks = addonFiles.Length == 0 ? "" : Convert.ToString(addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks);
                    streamWriter.WriteLine($"{keyValuePair.Key};{keyValuePair.Value}:{addonFiles.Length}:{ticks}");
                }
                ProgressAction.Invoke("Repo file written");
            }
        }

        public void GetRepoData() {
            if (!Directory.Exists(RepoPath)) {
                throw new Exception($"Repo '{RepoName}' does not exist");
            }
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(RepoPath, ".repo"));
            if (!repoDirectory.Exists) {
                throw new Exception("Repo file does not exist");
            }
            string[] repoFileLines = File.ReadAllLines(Path.Combine(RepoPath, ".repo", ".repo.urf"));
            ProgressAction.Invoke(repoFileLines.Aggregate("command::repodata", (current, line) => string.Join("::", current, line)));
        }
    }
}
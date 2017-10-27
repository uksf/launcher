using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Patching.Pbo;

namespace Patching.Repo {
    public class Repo : ProgressReporter {
        private Dictionary<string, string> _repoFileDictionary;

        public Repo(string path, string name, Action<string> progressAction) : base(progressAction) {
            RepoPath = path;
            RepoName = name;
        }

        private string RepoPath { get; }
        public string RepoName { get; }

        public void CreateRepo() {
            Directory.CreateDirectory(RepoPath);
            ProgressAction.Invoke($"Creating directory '{RepoPath}'");
            ProgressAction.Invoke("Creating repo folder");
            if (Directory.Exists(Path.Combine(RepoPath, ".repo"))) {
                Directory.Delete(Path.Combine(RepoPath, ".repo"), true);
            }
            DirectoryInfo repoDirectory = Directory.CreateDirectory(Path.Combine(RepoPath, ".repo"));
            _repoFileDictionary = new Dictionary<string, string>();
            foreach (Addon addon in GetAddonFolders().Select(addonFolder => new Addon(addonFolder, ProgressAction))) {
                ProgressAction.Invoke($"Processing addon '{addon.AddonName}'");
                addon.Serialize(repoDirectory);
                _repoFileDictionary.Add(addon.AddonName, addon.CheckSum);
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
            Dictionary<string, string> currentRepoFileDictionary = File.ReadAllLines(Path.Combine(RepoPath, ".repo", ".repo.urf")).Select(line => line.Split(':'))
                                                                      .ToDictionary(values => values[0], values => values[1]);
            _repoFileDictionary = new Dictionary<string, string>();
            List<Addon> addons = new List<Addon>();
            foreach (string addonFolder in GetAddonFolders()) {
                if (string.IsNullOrEmpty(currentRepoFileDictionary.Keys.FirstOrDefault(key => string.Equals(key, Path.GetFileName(addonFolder),
                                                                                                           StringComparison.InvariantCultureIgnoreCase)))) {
                    Addon addon = new Addon(addonFolder, ProgressAction);
                    addons.Add(addon);
                    ProgressAction.Invoke($"Found new addon '{addon.AddonName}'");
                } else {
                    Addon addon = new Addon(addonFolder, ProgressAction);
                    ProgressAction.Invoke($"Checking for changes in '{addon.AddonName}'");
                    if (addon.CheckSum.Equals(currentRepoFileDictionary[Path.GetFileName(addonFolder)])) {
                        _repoFileDictionary.Add(addon.AddonName, addon.CheckSum);
                        continue;
                    }
                    ProgressAction.Invoke($"Changes in addon '{addon.AddonName}'");
                    addons.Add(addon);
                }
            }
            foreach (Addon addon in addons) {
                ProgressAction.Invoke($"Processing addon '{addon.AddonName}'");
                addon.Serialize(repoDirectory);
                _repoFileDictionary.Add(addon.AddonName, addon.CheckSum);
            }
            if (_repoFileDictionary.Count == 0) {
                throw new Exception("No addons processed");
            }
            foreach (KeyValuePair<string, string> pair in currentRepoFileDictionary.Where(pair => !_repoFileDictionary.ContainsKey(pair.Key))) {
                ProgressAction.Invoke($"Addon deleted '{pair.Key}'");
                File.Delete(Path.Combine(RepoPath, ".repo", pair.Key));
            }
            WriteRepoFile();
        }

        private IEnumerable<string> GetAddonFolders() {
            List<string> addonFolders = Directory.GetDirectories(RepoPath, "@*").ToList();
            if (addonFolders.Count == 0) {
                throw new Exception("There are no addons in this repo");
            }
            return addonFolders;
        }

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(RepoPath, ".repo", ".repo.urf")))) {
                List<string> keys = _repoFileDictionary.Keys.ToList();
                keys.Sort();
                foreach (string key in keys) {
                    streamWriter.WriteLine($"{key}:{_repoFileDictionary[key]}");
                }
                ProgressAction.Invoke("Repo files written");
            }
        }
    }
}
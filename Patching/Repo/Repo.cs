using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using Patching.Actions;
using Patching.Pbo;
using Action = Patching.Actions.Action;

namespace Patching.Repo {
    public class Repo : ProgressReporter {
        private static Action<float, string> _progressUpdate;
        private static long _bytesRate, _completedBytes, _totalBytes;

        private static DateTime _lastReport = DateTime.Now;

        private readonly Dictionary<string, string> _repoFileDictionary;
        private readonly string _repoFilePath;

        private List<Action> _actions;

        private CancellationTokenSource _downloadCancellationTokenSource;

        public Repo(string path, string name, Action<string> progressAction) : base(progressAction) {
            RepoPath = path;
            RepoName = name;
            _repoFilePath = Path.Combine(RepoPath, ".repo", ".repo.urf");
            Directory.CreateDirectory(RepoPath);
            Directory.CreateDirectory(Path.Combine(RepoPath, ".repo"));
            _repoFileDictionary = new Dictionary<string, string>();
        }

        private string RepoPath { get; }
        public string RepoName { get; }

        public void CreateRepo() {
            ProgressAction.Invoke($"Creating directory '{RepoPath}'");
            ProgressAction.Invoke("Creating repo folder");
            string repoDirectoryPath = Path.Combine(RepoPath, ".repo");
            if (Directory.Exists(repoDirectoryPath)) {
                Directory.Delete(repoDirectoryPath, true);
            }
            Directory.CreateDirectory(repoDirectoryPath);
            DirectoryInfo repoDirectory = new DirectoryInfo(repoDirectoryPath);
            foreach (Addon addon in GetAddonFolders().Select(addonFolder => new Addon(addonFolder, ProgressAction))) {
                ProgressAction.Invoke($"Processing addon '{addon.AddonName}'");
                addon.Serialize(repoDirectory);
                _repoFileDictionary.Add(addon.AddonPath, addon.CheckSum);
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
            Dictionary<string, string> currentRepoFileDictionary = File.ReadAllLines(Path.Combine(RepoPath, ".repo", ".repo.urf")).Select(line => line.Split(';'))
                                                                       .ToDictionary(values => values[0], values => values[1]);
            List<Addon> addons = new List<Addon>();
            foreach (string addonFolder in GetAddonFolders()) {
                if (string.IsNullOrEmpty(currentRepoFileDictionary.Keys.FirstOrDefault(key => string.Equals(key, Path.GetFullPath(addonFolder),
                                                                                                            StringComparison.InvariantCultureIgnoreCase)))) {
                    Addon addon = new Addon(addonFolder, ProgressAction);
                    addons.Add(addon);
                    ProgressAction.Invoke($"Found new addon '{addon.AddonName}'");
                } else {
                    Addon addon = new Addon(addonFolder, ProgressAction);
                    ProgressAction.Invoke($"Checking for changes in '{addon.AddonName}'");
                    if (addon.CheckSum.Equals(currentRepoFileDictionary[Path.GetFullPath(addonFolder)])) {
                        _repoFileDictionary.Add(addon.AddonPath, addon.CheckSum);
                        continue;
                    }
                    ProgressAction.Invoke($"Changes in addon '{addon.AddonName}'");
                    addons.Add(addon);
                }
            }
            foreach (Addon addon in addons) {
                ProgressAction.Invoke($"Processing addon '{addon.AddonName}'");
                addon.Serialize(repoDirectory);
                _repoFileDictionary.Add(addon.AddonPath, addon.CheckSum);
            }
            if (_repoFileDictionary.Count == 0) {
                throw new Exception("No addons processed");
            }
            foreach (KeyValuePair<string, string> pair in currentRepoFileDictionary.Where(pair => !_repoFileDictionary.ContainsKey(pair.Key))) {
                ProgressAction.Invoke($"Addon deleted '{pair.Key}'");
                File.Delete(Path.Combine(RepoPath, ".repo", Path.GetFileName(pair.Key)));
            }
            WriteRepoFile();
        }

        private IEnumerable<string> GetAddonFolders() {
            List<string> addonFolders = Directory.GetDirectories(RepoPath, "@*").ToList();
            if (addonFolders.Count == 0) {
                throw new Exception("There are no addons in this location");
            }
            return addonFolders;
        }

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                List<string> keys = _repoFileDictionary.Keys.ToList();
                keys.Sort();
                foreach (string key in keys) {
                    streamWriter.WriteLine($"{key};{_repoFileDictionary[key]}");
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

        private void GetLocalRepoFile() {
            _repoFileDictionary.Clear();
            if (!Directory.Exists(RepoPath)) {
                throw new Exception($"Repo '{RepoName}' does not exist");
            }
            if (!File.Exists(_repoFilePath)) return;
            string[] repoFileLines = File.ReadAllLines(_repoFilePath);
            foreach (string[] repoParts in repoFileLines.Select(repoLine => repoLine.Split(';'))) {
                _repoFileDictionary.Add(repoParts[0], repoParts[1]);
            }
        }

        public bool CheckLocalRepo(string remoteRepoData, Action<float, string> progressUpdate) {
            _downloadCancellationTokenSource = new CancellationTokenSource();
            try {
                _progressUpdate = progressUpdate;
                string[] remoteRepoDataLines = remoteRepoData.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                List<Addon> addons = new List<Addon>();
                GetLocalRepoFile();
                Dictionary<string, string> repoFileDictionaryCopy = _repoFileDictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
                _repoFileDictionary.Clear();
                int currentIndex = 0;
                foreach (string[] remoteRepoDataParts in remoteRepoDataLines.Select(remoteRepoDataPart => remoteRepoDataPart.Split(';'))) {
                    string remoteAddonFolder = Path.GetFileName(remoteRepoDataParts[0]);
                    string key = repoFileDictionaryCopy.Keys.FirstOrDefault(localRepoAddonPath =>
                                                                                string.Equals(Path.GetFileName(localRepoAddonPath), remoteAddonFolder,
                                                                                              StringComparison.InvariantCultureIgnoreCase));
                    if (remoteAddonFolder != null) key = string.IsNullOrEmpty(key) ? Path.Combine(RepoPath, remoteAddonFolder) : key;
                    _progressUpdate.Invoke((float) currentIndex / remoteRepoDataLines.Length, $"Checking '{Path.GetFileName(key)}'");
                    if (!CheckCache(key, remoteRepoDataParts[1], out Addon addon)) {
                        addons.Add(addon);
                        _repoFileDictionary.Add(addon.AddonPath, addon.CheckSum);
                    } else {
                        if (key != null) _repoFileDictionary.Add(key, remoteRepoDataParts[1]);
                    }
                    currentIndex++;
                }
                ProgressAction.Invoke($"{addons.Count} addons have changes");
                currentIndex = 0;
                _progressUpdate.Invoke(0, "Finding differences");
                _actions = new List<Action>();
                foreach (Addon addon in addons) {
                    _progressUpdate.Invoke((float) currentIndex / addons.Count, "Finding differences");
                    Addon remoteAddonData = GetRemoteAddonData(addon.AddonName);
                    _actions.AddRange(addon.Compare(remoteAddonData));
                    currentIndex++;
                }
                ProgressAction.Invoke($"{_actions.Count} changes");
                GetLocalRepoFile();
                WriteRepoFile();
                if (_actions.Count == 0) {
                    _progressUpdate.Invoke(1, "stop");
                    return false;
                }
            } catch (Exception exception) {
                _downloadCancellationTokenSource.Cancel();
                _progressUpdate.Invoke(1, "stop");
                ProgressAction.Invoke($"An error occured during repo check.\n{exception}");
                throw;
            }
            return true;
        }

        private bool CheckCache(string addonPath, string hash, out Addon addon) {
            bool result;
            if (!Directory.Exists(addonPath) || Directory.GetFiles(addonPath).Length == 0) {
                addon = new Addon(addonPath, ProgressAction);
                CreateCacheFile(addon);
                result = false;
                ProgressAction.Invoke($"Addon not found '{addonPath}'");
            } else {
                string[] addonFiles = Directory.GetFiles(addonPath, "*", SearchOption.AllDirectories);
                DateTime dateTime = addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime);
                string[] cache = GetCache(Path.GetFileName(addonPath));
                if (cache.Length == 0) {
                    addon = new Addon(addonPath, ProgressAction);
                    CreateCacheFile(addon);
                    result = string.Equals(addon.CheckSum, hash);
                    ProgressAction.Invoke($"Addon found, cache not found '{addonPath}'");
                } else {
                    if (cache[0] != hash || Convert.ToInt32(cache[1]) != addonFiles.Length || Convert.ToInt64(cache[2]) != dateTime.Ticks) {
                        addon = new Addon(addonPath, ProgressAction);
                        CreateCacheFile(addon);
                        result = false;
                        ProgressAction.Invoke($"Addon cache invalid '{addonPath}'");
                    } else {
                        addon = null;
                        result = true;
                        ProgressAction.Invoke($"Addon cache valid '{addonPath}'");
                    }
                }
            }
            return result;
        }

        private void CreateCacheFile(Addon addon) {
            string[] addonFiles = Directory.GetFiles(addon.AddonPath, "*", SearchOption.AllDirectories);
            if (addonFiles.Length == 0) return;
            DateTime dateTime = addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime);
            File.WriteAllLines(Path.Combine(RepoPath, ".repo", addon.AddonName), new List<string> {addon.CheckSum, $"{addonFiles.Length}", $"{dateTime.Ticks}"});
        }

        private string[] GetCache(string addonName) =>
            !File.Exists(Path.Combine(RepoPath, ".repo", addonName)) ? new string[] { } : File.ReadAllLines(Path.Combine(RepoPath, ".repo", addonName));

        private Addon GetRemoteAddonData(string addonName) {
            WebClient webClient = new WebClient();
            string addonData = "";
            try {
                addonData = webClient.DownloadString(new Uri($"http://www.uk-sf.com/launcher/repos/{RepoName}/.repo/{addonName}"));
            } catch (Exception) {
                ProgressAction.Invoke($"Could not get addon data for '{addonName}'");
            }
            return Addon.DeSerialize(addonData, ProgressAction);
        }

        public void SynchroniseLocalRepo() {
            Queue<KeyValuePair<AddonFile, Part>> downloadQueue = CheckExistingFiles();
            foreach (KeyValuePair<AddonFile, Part> pair in downloadQueue) {
                _totalBytes += pair.Value.Length;
            }
            _progressUpdate.Invoke(0, "Downloading files");
            Parallel.ForEach(downloadQueue, new ParallelOptions {MaxDegreeOfParallelism = 10, CancellationToken = _downloadCancellationTokenSource.Token}, DownloadFile);
            int currentIndex = 0;
            foreach (Action action in _actions) {
                AddonFile addonFile = action.GetAddonFile();
                _progressUpdate.Invoke((float) currentIndex / _actions.Count, "Patching files");
                action.Consume(Path.Combine(RepoPath, ".repo", "temp", addonFile.AddonName), Path.Combine(RepoPath, addonFile.AddonName));
                currentIndex++;
            }
            foreach (string directory in Directory.GetDirectories(Path.Combine(RepoPath, ".repo", "temp"), "*", SearchOption.TopDirectoryOnly)) {
                Directory.Delete(directory, true);
            }
            _progressUpdate.Invoke(1, "stop");
        }

        private Queue<KeyValuePair<AddonFile, Part>> CheckExistingFiles() {
            Queue<KeyValuePair<AddonFile, Part>> queue = new Queue<KeyValuePair<AddonFile, Part>>();
            foreach (Action action in _actions) {
                if (action is Modified) {
                    HandleModified(ref queue, action as Modified);
                } else if (action is Added) {
                    HandleAdded(ref queue, action as Added);
                }
            }
            return queue;
        }

        private void HandleAdded(ref Queue<KeyValuePair<AddonFile, Part>> queue, Added added) {
            AddonFile addonFile = added.AddonFile;
            Part part = addonFile.Parts.First().Value;
            string path = Path.Combine(RepoPath, ".repo", "temp", addonFile.AddonName);
            part.Path = addonFile.RelativeFilePath;
            string file = Path.Combine(path, part.Path);
            part.CheckSumBytes = addonFile.CheckSumBytes;
            part.Length = addonFile.Length;
            addonFile.Parts.Clear();
            addonFile.Parts[Utility.Hash(file)] = part;
            if (File.Exists(file) && part.Compare(file)) return;
            if (File.Exists(file)) {
                File.Delete(file);
            }
            queue.Enqueue(new KeyValuePair<AddonFile, Part>(addonFile, part));
        }

        private void HandleModified(ref Queue<KeyValuePair<AddonFile, Part>> queue, Modified modified) {
            AddonFile addonFile = modified.AddonFile;
            string path = Path.Combine(RepoPath, ".repo", "temp", addonFile.AddonName);
            List<Part> partList = new List<Part>();
            Part part = modified.Parts.First();
            int index = 0;
            foreach (Part nextPart in modified.Parts.Skip(1)) {
                if (part.Length < 50000000 && nextPart.Start == part.Start + part.Length) {
                    part.Length += nextPart.Length;
                    addonFile.Parts.Remove(Utility.Hash(nextPart.Path));
                    ++index;
                } else {
                    partList.Add(part);
                    if (index > 0) {
                        part.Path = Path.Combine(Path.GetDirectoryName(part.Path), $"chunk_{part.Start}_{part.Length}");
                    }
                    index = 0;
                    part = nextPart;
                }
            }
            if (!partList.Contains(part)) {
                partList.Add(part);
                if (index > 0) {
                    part.Path = Path.Combine(Path.GetDirectoryName(part.Path), $"blob_{part.Start}_{part.Length}");
                }
            }
            modified.Parts = partList;
            foreach (Part finalPart in partList) {
                finalPart.Path = Utility.CleanPath(finalPart.Path);
                if (Path.Combine(path, finalPart.Path).Length > 250) {
                    finalPart.Path = Utility.GenerateHash(finalPart.Path).Substring(0, 5);
                }
                string file = Path.Combine(path, finalPart.Path);
                if (File.Exists(file) && part.Compare(file)) continue;
                if (File.Exists(file)) {
                    File.Delete(file);
                }
                queue.Enqueue(new KeyValuePair<AddonFile, Part>(addonFile, finalPart));
            }
        }

        private void DownloadFile(KeyValuePair<AddonFile, Part> data) {
            try {
                string path = Path.Combine(RepoPath, ".repo", "temp", data.Key.AddonName, data.Value.Path);
                if (Directory.Exists(path)) {
                    Directory.Delete(path, true);
                }
                if (File.Exists(path)) {
                    // TODO: Check if file downloaded in temp, if yes, create hash, check hash, if same, skip.
                    data.Value.Path = data.Value.Path.Replace(Path.GetFileName(data.Value.Path), Path.GetRandomFileName());
                    path = Path.Combine(RepoPath, ".repo", "temp", data.Key.AddonName, data.Value.Path);
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
                if (!directoryInfo.Exists) {
                    directoryInfo.Create();
                }
                HttpWebRequest httpWebRequest =
                    (HttpWebRequest) WebRequest.Create($"http://www.uk-sf.com/launcher/repos/{RepoName}/{data.Key.AddonName}/{data.Key.RelativeFilePath}");
                httpWebRequest.AutomaticDecompression = data.Value.Length < 1572864.0 ? DecompressionMethods.None : DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpWebRequest.AddRange(data.Value.Start, data.Value.Start + data.Value.Length);
                byte[] buffer = new byte[4096];
                using (HttpWebResponse response = (HttpWebResponse) httpWebRequest.GetResponse()) {
                    using (FileStream fileStream = File.OpenWrite(path)) {
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
                ProgressAction.Invoke($"Downloaded '{data.Key.AddonName}\\{data.Value.Path}'");
            } catch (Exception exception) {
                ProgressAction.Invoke($"Download errored for {data.Value.Path}:\n{exception}");
                throw;
            }
        }

        private static void ReportProgress(long change) {
            _completedBytes += change;
            _bytesRate += change;
            if (DateTime.Now < _lastReport) return;
            _lastReport = DateTime.Now.AddMilliseconds(100);
            _progressUpdate.Invoke((float) _completedBytes / _totalBytes,
                                   $"Downloading files\n{ByteSize.FromBytes(_completedBytes)} of {ByteSize.FromBytes(_totalBytes)} ({_bytesRate * 80 / 1024 / 1000} Mbps)");
            _bytesRate = 0L;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Patching.Actions;
using Action = Patching.Actions.Action;

namespace Patching.Pbo {
    internal class Addon : ProgressReporter {
        private readonly Dictionary<string, AddonFile> _addonFileDictionary = new Dictionary<string, AddonFile>();

        private Addon(Action<string> progressAction) : base(progressAction) { }

        public Addon(string addonFolder, Action<string> progressAction) : base(progressAction) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            directoryInfo.Create();
            AddonPath = directoryInfo.FullName;
            AddonName = directoryInfo.Name;
            ConcurrentBag<AddonFile> addonFilesBag = new ConcurrentBag<AddonFile>();
            Parallel.ForEach(directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).ToList(), delegate(FileInfo file) {
                AddonFile addonFile = file.Extension == ".pbo"
                                          ? new AddonPbo(file.FullName, AddonPath, AddonName, ProgressAction)
                                          : new AddonFile(file.FullName, AddonPath, AddonName, ProgressAction);
                addonFile.ProcessFiles(file);
                addonFilesBag.Add(addonFile);
            });
            List<AddonFile> addonFiles = addonFilesBag.ToList();
            addonFiles.Sort((addonA, addonB) => string.Compare(addonA.RelativeFilePath, addonB.RelativeFilePath, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
            foreach (AddonFile addonFile in addonFiles) {
                _addonFileDictionary.Add(Utility.Hash(addonFile.RelativeFilePath), addonFile);
            }
            CalculateCheckSum();
        }

        public string AddonPath { get; }
        public string AddonName { get; set; }
        public string CheckSum { get; private set; }

        private void CalculateCheckSum() {
            SHA1 sha1 = SHA1.Create();
            List<AddonFile> addonList = _addonFileDictionary.Values.ToList();
            addonList.Sort((addonA, addonB) => string.Compare(addonA.FileName, addonB.FileName, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
            foreach (byte[] bytes in from addon in addonList select addon.CheckSumBytes) {
                sha1.TransformBlock(bytes, 0, 16, null, 0);
            }
            sha1.TransformFinalBlock(new byte[0], 0, 0);
            StringBuilder hexString = new StringBuilder();
            foreach (string hex in from hashByte in sha1.Hash select hashByte.ToString("x2")) {
                hexString.Append(hex);
            }
            sha1.Dispose();
            CheckSum = hexString.ToString();
        }

        public void Serialize(DirectoryInfo repoDirectory) {
            using (FileStream stream = File.Create(Path.Combine(repoDirectory.FullName, AddonName))) {
                byte[] bytes = Encoding.UTF8.GetBytes($"Addon:{AddonName}:{CheckSum}:{_addonFileDictionary.Count}\n");
                stream.Write(bytes, 0, bytes.Length);
                foreach (AddonFile addonFile in _addonFileDictionary.Values) {
                    addonFile.Serialize(stream);
                }
            }
        }

        public static Addon DeSerialize(string serialized, Action<string> progressAction) {
            Addon addon = new Addon(progressAction);
            using (StringReader stringReader = new StringReader(serialized)) {
                List<string> addonList = stringReader.ReadLine()?.Split(':').Skip(1).ToList();
                addon.AddonName = addonList?[0];
                int addonCount = Convert.ToInt32(addonList?[2]);
                for (int i = 0; i < addonCount; i++) {
                    List<string> fileString = stringReader.ReadLine()?.Split(':').ToList();
                    if (fileString?[0] != "File") {
                        throw new InvalidDataException();
                    }
                    AddonFile addonFile = new AddonFile(fileString, stringReader, progressAction) {AddonName = addon.AddonName};
                    addon._addonFileDictionary.Add(Utility.Hash(addonFile.RelativeFilePath), addonFile);
                }
            }
            return addon;
        }

        public IEnumerable<Action> Compare(Addon remoteAddon) {
            List<Action> actionList = new List<Action>();
            foreach (AddonFile remoteAddonFile in remoteAddon._addonFileDictionary.Values) {
                _addonFileDictionary.TryGetValue(Utility.Hash(remoteAddonFile.RelativeFilePath), out AddonFile addonFile);
                if (addonFile == null && File.Exists(Path.Combine(AddonPath, remoteAddonFile.RelativeFilePath))) {
                    addonFile = _addonFileDictionary.Values.FirstOrDefault(localAddonFile =>
                                                                               localAddonFile.RelativeFilePath.Equals(remoteAddonFile.RelativeFilePath,
                                                                                                                      StringComparison.InvariantCultureIgnoreCase));
                }
                if (addonFile == null) {
                    actionList.Add(new Added(remoteAddonFile));
                } else if (!remoteAddonFile.Compare(addonFile)) {
                    actionList.AddRange(addonFile.Differences(remoteAddonFile));
                }
            }
            foreach (AddonFile localAddonFile in _addonFileDictionary.Values) {
                remoteAddon._addonFileDictionary.TryGetValue(Utility.Hash(localAddonFile.RelativeFilePath), out AddonFile addonFile);
                if (addonFile != null) continue;
                addonFile =
                    remoteAddon._addonFileDictionary.Values.FirstOrDefault(file => file.RelativeFilePath.Equals(localAddonFile.RelativeFilePath,
                                                                                                                StringComparison.InvariantCultureIgnoreCase));
                if (addonFile == null) {
                    actionList.Add(new Deleted(localAddonFile));
                }
            }
            return actionList;
        }
    }
}
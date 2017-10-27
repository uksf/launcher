using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Patching.Pbo {
    internal class Addon : ProgressReporter {
        private readonly Dictionary<string, AddonFile> _addonFileDictionary;

        public Addon(string addonFolder, Action<string> progressAction) : base(progressAction) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            AddonPath = directoryInfo.FullName;
            AddonName = directoryInfo.Name;
            _addonFileDictionary = new Dictionary<string, AddonFile>();
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

        public string AddonPath { get; }
        public string AddonName { get; }
        public string CheckSum { get; private set; }
    }
}
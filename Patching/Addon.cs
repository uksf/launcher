using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastRsync.Core;
using FastRsync.Signature;

namespace Patching {
    internal class Addon {
        private readonly string _repoFolder;

        public readonly string FolderPath;
        public readonly string Name;
        public string FullHash;

        public Addon(string addonFolder, DirectoryInfo repoDirectory) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            directoryInfo.Create();
            FolderPath = directoryInfo.FullName;
            Name = directoryInfo.Name;
            _repoFolder = repoDirectory.FullName;
        }

        public void GenerateFullHash() {
            Dictionary<string, string> hashDictionary = File.ReadAllLines(Path.Combine(_repoFolder, $"{Name}.urf")).Select(line => line.Split(';'))
                                                            .ToDictionary(values => values[0], values => values[1].Split(':')[0]);
            FullHash = Utility.ShaFromDictionary(hashDictionary);
        }

        public void GenerateAllHashes() {
            ConcurrentDictionary<string, string> hashDictionary = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(new DirectoryInfo(FolderPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList(),
                             file => hashDictionary.TryAdd(file.FullName, Utility.ShaFromFile(file.FullName)));
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(_repoFolder, $"{Name}.urf")))) {
                foreach (string key in from file in hashDictionary.Keys orderby file select file) {
                    streamWriter.WriteLine($"{key.Replace($"{FolderPath}{Path.DirectorySeparatorChar}", "")};{hashDictionary[key]}:{new FileInfo(key).Length}:{new FileInfo(key).LastWriteTime.Ticks}");
                }
            }
        }

        public Addon GenerateHash(string addonFile) {
            Dictionary<string, string> hashDictionary = File.ReadAllLines(Path.Combine(_repoFolder, $"{Name}.urf")).Select(line => line.Split(';'))
                                                            .ToDictionary(values => values[0], values => values[1].Split(':')[0]);
            string key = addonFile.Replace($"{FolderPath}{Path.DirectorySeparatorChar}", "");
            if (hashDictionary.ContainsKey(key)) {
                hashDictionary[key] = Utility.ShaFromFile(addonFile);
            } else {
                hashDictionary.Add(key, Utility.ShaFromFile(addonFile));
            }
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(_repoFolder, $"{Name}.urf")))) {
                foreach (string file in from file in hashDictionary.Keys orderby file select file) {
                    streamWriter.WriteLine($"{file};{hashDictionary[file]}:{new FileInfo(Path.Combine(FolderPath, file)).Length}:{new FileInfo(Path.Combine(FolderPath, file)).LastWriteTime.Ticks}");
                }
            }
            return this;
        }

        public string GenerateSignature(string filePath) {
            FileInfo signatureFileInfo = new FileInfo(Path.Combine(_repoFolder, Path.GetRandomFileName()));
            if (!Directory.Exists(signatureFileInfo.DirectoryName)) {
                Directory.CreateDirectory(signatureFileInfo.DirectoryName);
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signatureFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    SignatureBuilder signatureBuilder = new SignatureBuilder {HashAlgorithm = SupportedAlgorithms.Hashing.Create("MD5")};
                    signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                    return signatureFileInfo.FullName;
                }
            }
        }
    }
}
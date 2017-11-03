using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastRsync.Signature;

namespace Patching {
    internal class AddonNew {
        public AddonNew(string addonFolder) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            directoryInfo.Create();
            FolderPath = directoryInfo.FullName;
            Name = directoryInfo.Name;
        }

        public string FolderPath { get; }
        public string Name { get; }
        public string SignaturesCheckSum { get; private set; }

        public void GenerateAllSignatures(DirectoryInfo repoDirectory) {
            if (Directory.Exists(Path.Combine(repoDirectory.FullName, Name))) {
                Directory.Delete(Path.Combine(repoDirectory.FullName, Name), true);
            }
            Directory.CreateDirectory(Path.Combine(repoDirectory.FullName, Name));
            ConcurrentBag<FileInfo> signatureFiles = new ConcurrentBag<FileInfo>();
            Parallel.ForEach(new DirectoryInfo(FolderPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList(), file => {
                FileInfo signatureFileInfo = new FileInfo(Path.Combine(repoDirectory.FullName,
                                                                       $"{file.FullName.Replace(Directory.GetParent(FolderPath).FullName + Path.DirectorySeparatorChar, "")}.urf"));
                if (!Directory.Exists(signatureFileInfo.DirectoryName)) {
                    Directory.CreateDirectory(signatureFileInfo.DirectoryName);
                }
                using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream signatureStream = new FileStream(signatureFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        SignatureBuilder signatureBuilder = new SignatureBuilder {ChunkSize = SignatureBuilder.MaximumChunkSize};
                        signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                        signatureFiles.Add(file);
                    }
                }
            });
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(repoDirectory.FullName, $"{Name}.urf")))) {
                foreach (FileInfo signatureFile in from values in signatureFiles orderby values.FullName select values) {
                    streamWriter.WriteLine($"{signatureFile.FullName.Replace(FolderPath + Path.DirectorySeparatorChar, "")};{Utility.ShaFromFile(signatureFile.FullName)}");
                }
            }
            GenerateSignaturesChecksum(repoDirectory);
        }

        public void GenerateSignaturesChecksum(DirectoryInfo repoDirectory) {
            Dictionary<string, string> signatureFilesDictionary = File.ReadAllLines(Path.Combine(repoDirectory.FullName, $"{Name}.urf")).Select(line => line.Split(';'))
                                                                      .ToDictionary(values => values[0], values => values[1]);
            SignaturesCheckSum = Utility.ShaFromDictionary(signatureFilesDictionary);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FastRsync.Signature;

namespace Patching {
    internal class Addon {
        public Addon(string addonFolder, DirectoryInfo repoDirectory) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            directoryInfo.Create();
            FolderPath = directoryInfo.FullName;
            Name = directoryInfo.Name;
            RepoFolder = repoDirectory.FullName;
        }

        public string FolderPath { get; }
        public string Name { get; }
        public string SignaturesCheckSum { get; private set; }
        private string RepoFolder { get; }

        public void GenerateAllSignatures() {
            if (Directory.Exists(Path.Combine(RepoFolder, Name))) {
                Directory.Delete(Path.Combine(RepoFolder, Name), true);
            }
            Directory.CreateDirectory(Path.Combine(RepoFolder, Name));
            Parallel.ForEach(new DirectoryInfo(FolderPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList(), file => {
                FileInfo signatureFileInfo =
                    new FileInfo(Path.Combine(RepoFolder, $"{file.FullName.Replace(Directory.GetParent(FolderPath).FullName + Path.DirectorySeparatorChar, "")}.urf"));
                if (!Directory.Exists(signatureFileInfo.DirectoryName)) {
                    Directory.CreateDirectory(signatureFileInfo.DirectoryName);
                }
                using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream signatureStream = new FileStream(signatureFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        SignatureBuilder signatureBuilder = new SignatureBuilder();
                        signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                    }
                }
            });
        }

        public void GenereateSignature(string filePath) {
            FileInfo signatureFileInfo =
                new FileInfo(Path.Combine(RepoFolder, $"{filePath.Replace(Directory.GetParent(FolderPath).FullName + Path.DirectorySeparatorChar, "")}.urf"));
            if (!Directory.Exists(signatureFileInfo.DirectoryName)) {
                Directory.CreateDirectory(signatureFileInfo.DirectoryName);
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signatureFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    SignatureBuilder signatureBuilder = new SignatureBuilder();
                    signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                }
            }
        }

        public void GenerateSignaturesChecksum() {
            if (!File.Exists(Path.Combine(RepoFolder, $"{Name}.urf"))) {
                GenerateAllSignatures();
                return;
            }
            List<string> signatureFiles = Directory.GetFiles(Path.Combine(RepoFolder, Name), "*", SearchOption.AllDirectories).ToList();
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(RepoFolder, $"{Name}.urf")))) {
                foreach (string signatureFile in from file in signatureFiles orderby file select file) {
                    streamWriter.WriteLine($"{signatureFile.Replace($"{RepoFolder}\\{Name}{Path.DirectorySeparatorChar}", "")};{Utility.ShaFromFile(signatureFile)}");
                }
            }
            Dictionary<string, string> signatureFilesDictionary = File.ReadAllLines(Path.Combine(RepoFolder, $"{Name}.urf")).Select(line => line.Split(';'))
                                                                      .ToDictionary(values => values[0], values => values[1]);
            SignaturesCheckSum = Utility.ShaFromDictionary(signatureFilesDictionary);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Patching.Pbo {
    internal class AddonFile : ProgressReporter {
        private string _fileName;

        public AddonFile(string filePath, string addonPath, string addonName, Action<string> progressAction) : base(progressAction) {
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
            RelativeFilePath = FilePath.Replace(addonPath + Path.DirectorySeparatorChar, "");
            AddonName = addonName;
            Parts = new Dictionary<string, Part>();
        }

        private string FilePath { get; }
        public string FileName { get => _fileName ?? Path.GetFileName(RelativeFilePath); private set => _fileName = value; }
        public string RelativeFilePath { get; }
        private string AddonName { get; }
        public byte[] CheckSumBytes { get; protected set; }
        protected Dictionary<string, Part> Parts { get; }

        public virtual void ProcessFiles(FileInfo file) {
            using (Stream stream = file.OpenRead()) {
                long fileLength = file.Length;
                int chunkSize = fileLength < 5000000L ? (int) fileLength : 5000000;
                MD5 md5 = MD5.Create();
                md5.Initialize();
                byte[] bytes = new byte[chunkSize];
                int partIndex = 0;
                while (fileLength >= 0) {
                    int chunkLength = stream.Read(bytes, 0, (int) Math.Min(fileLength, chunkSize));
                    fileLength -= chunkLength;
                    if (chunkLength == 0) break;
                    byte[] checkSumBytes = Utility.Generate(bytes, chunkLength);
                    md5.TransformBlock(bytes, 0, chunkLength, null, 0);
                    Part part = new Part($"{RelativeFilePath}_{partIndex}", checkSumBytes, Parts.Values.Sum(tempPart => tempPart.Length), chunkLength);
                    Parts.Add(Utility.Hash(part.Path), part);
                    partIndex++;
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                CheckSumBytes = md5.Hash;
                md5.Dispose();
            }
        }

        public void Serialize(FileStream stream) {
            byte[] bytes = Encoding.UTF8.GetBytes($"File:{RelativeFilePath}:{Parts.Values.Sum(part => part.Length)}:{Convert.ToBase64String(CheckSumBytes)}:{Parts.Count}\n");
            stream.Write(bytes, 0, bytes.Length);
            foreach (Part part in Parts.Values) {
                part.Serialize(stream);
            }
        }

        public bool Compare(AddonFile to) => Parts.Count == to.Parts.Count && CheckSumBytes.SequenceEqual(to.CheckSumBytes);
    }
}
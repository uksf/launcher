using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Patching.Actions;
using Action = Patching.Actions.Action;

namespace Patching.Pbo {
    public class AddonFile : ProgressReporter {
        private string _fileName;

        public AddonFile(string filePath, string addonPath, string addonName, Action<string> progressAction) : base(progressAction) {
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
            RelativeFilePath = FilePath.Replace(addonPath + Path.DirectorySeparatorChar, "");
            AddonName = addonName;
            Parts = new Dictionary<string, Part>();
        }

        public AddonFile(IReadOnlyList<string> fileList, TextReader stringReader, Action<string> progressAction) : base(progressAction) {
            RelativeFilePath = fileList[1];
            CheckSumBytes = Convert.FromBase64String(fileList[3]);
            Parts = new Dictionary<string, Part>();
            int fileCount = Convert.ToInt32(fileList[4]);
            for (int i = 0; i < fileCount; i++) {
                Part part = Part.DeSerialize(stringReader.ReadLine());
                Parts.Add(Utility.Hash(part.Path), part);
            }
        }

        private string FilePath { get; }
        public string FileName { get => _fileName ?? Path.GetFileName(RelativeFilePath); set => _fileName = value; }
        public string RelativeFilePath { get; }
        public string AddonName { get; set; }
        public byte[] CheckSumBytes { get; protected set; }
        public int Length { get { return Parts.Values.Sum(part => part.Length); } }
        public Dictionary<string, Part> Parts { get; }

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

        public List<Action> Differences(AddonFile remoteAddonFile) {
            List<Part> parts = new List<Part>();
            foreach (Part remotePart in remoteAddonFile.Parts.Values) {
                string key = Utility.Hash(remotePart.Path);
                if (!Parts.ContainsKey(key)) {
                    parts.Add(remotePart);
                } else {
                    if (!Parts[key].Compare(remotePart)) {
                        parts.Add(remotePart);
                    }
                }
            }
            return new List<Action> {new Modified(remoteAddonFile, this, parts)};
        }
    }
}
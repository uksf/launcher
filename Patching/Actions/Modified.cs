using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Patching.Pbo;

namespace Patching.Actions {
    internal class Modified : Added {
        private readonly AddonFile _oldAddonFile;

        public Modified(AddonFile addonFile, AddonFile oldAddonFile, List<Part> parts) : base(addonFile) {
            Parts = parts;
            _oldAddonFile = oldAddonFile;
        }

        public List<Part> Parts { get; set; }

        public override void Consume(string pathSource, string pathDestination) {
            string path = Path.Combine(pathDestination, AddonFile.RelativeFilePath);
            if (!Directory.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            string source = Path.Combine(pathSource, Path.GetRandomFileName());
            byte[] buffer = new byte[1000000];
            FileStream fileStream = File.Create(source);
            fileStream.SetLength(AddonFile.Length);
            FileStream fileStreamSource = File.OpenRead(path);
            while (fileStream.Position != fileStream.Length) {
                foreach (Part part in from addonFileParts in AddonFile.Parts.Values.ToList() orderby addonFileParts.Start select addonFileParts) {
                    string pathHash = Utility.Hash(part.Path);
                    Part secondPart = Parts.Find(hashPart => Utility.Hash(hashPart.Path).Equals(pathHash));
                    if (secondPart == null) {
                        if (!_oldAddonFile.Parts.ContainsKey(pathHash)) {
                            throw new FileNotFoundException();
                        }
                        Part originalPart = _oldAddonFile.Parts[pathHash];
                        fileStreamSource.Position = originalPart.Start;
                        WriteTo(fileStreamSource, fileStream, originalPart.Length, buffer);
                    } else {
                        using (FileStream fileStreamTemp = File.OpenRead(Path.Combine(pathSource, secondPart.Path))) {
                            WriteTo(fileStreamTemp, fileStream, secondPart.Length, buffer);
                        }
                    }
                }
            }
            fileStreamSource.Close();
            fileStream.Close();
            File.Copy(source, path, true);
            File.Delete(source);
        }

        private static void WriteTo(Stream fileStreamSource, Stream fileStream, int secondPartLength, byte[] buffer) {
            int index = 0;
            while (index < secondPartLength) {
                int count = fileStreamSource.Read(buffer, 0, Math.Min(secondPartLength - index, buffer.Length));
                if (count == 0) {
                    throw new EndOfStreamException($"File ran out. {index} of {secondPartLength}");
                }
                fileStream.Write(buffer, 0, count);
                index += count;
            }
        }

        public override List<string> GetTemporaryFiles(string sourceFolder) => Parts.Select(part => Path.Combine(sourceFolder, part.Path)).ToList();
    }
}
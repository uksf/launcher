using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Patching.Pbo {
    internal class AddonPbo : AddonFile {
        public AddonPbo(string filePath, string addonPath, string addonName, Action<string> progressAction) : base(filePath, addonPath, addonName, progressAction) { }
        public AddonPbo(IReadOnlyList<string> lines, TextReader stringReader, Action<string> progressAction) : base(lines, stringReader, progressAction) { }

        public override void ProcessFiles(FileInfo file) {
            using (FileStream fileStream = file.OpenRead()) {
                PboHeader pboHeader;
                try {
                    pboHeader = new PboHeader(fileStream);
                } catch (Exception exception) {
                    throw new Exception("Error casued in " + RelativeFilePath, exception);
                }
                int chunkSize = (int) fileStream.Position;
                fileStream.Position = 0;
                MD5 md5 = MD5.Create();
                md5.Initialize();
                byte[] bytes = new byte[chunkSize];
                fileStream.Read(bytes, 0, chunkSize);
                md5.TransformBlock(bytes, 0, chunkSize, null, 0);
                Parts.Add(Utility.Hash($"{Path.Combine(RelativeFilePath, "$HEADER$")}"),
                          new Part($"{Path.Combine(RelativeFilePath, "$HEADER$")}", Utility.Generate(bytes, chunkSize), 0, chunkSize));
                bytes = new byte[pboHeader.Files.Max(x => x.Size)];
                foreach (PboFile pboFile in pboHeader.Files) {
                    long chunkLength = fileStream.Position;
                    int num2 = fileStream.Read(bytes, 0, pboFile.Size);
                    md5.TransformBlock(bytes, 0, num2, null, 0);
                    Part part = new Part($"{Path.Combine(RelativeFilePath, pboFile.ToString().Replace("\t", "t"))}", Utility.Generate(bytes, num2), (int) chunkLength,
                                         pboFile.Size);
                    Parts.Add(Utility.Hash(part.Path), part);
                }
                bytes = new byte[fileStream.Length - fileStream.Position];
                fileStream.Read(bytes, 0, bytes.Length);
                md5.TransformBlock(bytes, 0, bytes.Length, null, 0);
                Parts.Add(Utility.Hash($"{Path.Combine(RelativeFilePath, "$END$")}"),
                          new Part($"{Path.Combine(RelativeFilePath, "$END$")}", Utility.Generate(bytes, bytes.Length), (int) (fileStream.Length - bytes.Length), bytes.Length));
                md5.TransformFinalBlock(new byte[0], 0, 0);
                CheckSumBytes = md5.Hash;
                md5.Dispose();
                fileStream.Close();
            }
        }
    }
}
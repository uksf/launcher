using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Patching.Pbo {
    internal class PboHeader {
        public readonly List<PboFile> Files = new List<PboFile>();

        public PboHeader(FileStream fileStream) {
            if (fileStream.ReadByte() != 0) {
                throw new Exception("First byte not zero");
            }
            byte[] bytes = new byte[4];
            fileStream.Read(bytes, 0, 4);
            if (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("sreV"))) {
                throw new Exception("sreV missing");
            }
            for (int i = 0; i < 16; i++) {
                fileStream.ReadByte();
            }
            if (fileStream.ReadByte() != 0) {
                long position = fileStream.Position;
                fileStream.Position = position - 1;
                Utility.ReadString(fileStream);
                Utility.ReadString(fileStream);
                do {
                    bytes = Utility.ReadString(fileStream);
                } while (bytes.Length != 0);
            }
            PboFile pboFile = new PboFile(fileStream);
            while (!pboFile.IsNull) {
                Files.Add(pboFile);
                pboFile = new PboFile(fileStream);
            }
        }
    }
}
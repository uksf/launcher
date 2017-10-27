using System;
using System.IO;
using System.Text;

namespace Patching.Pbo {
    internal class Part {
        public Part(string path, byte[] checkSumBytes, int start, int length) {
            Path = path;
            CheckSumBytes = checkSumBytes;
            Start = start;
            Length = length;
        }

        public string Path { get; }
        private byte[] CheckSumBytes { get; }
        private int Start { get; }
        public int Length { get; }

        public void Serialize(FileStream stream) {
            byte[] bytes = Encoding.UTF8.GetBytes($"{Path}:{Length}:{Convert.ToBase64String(CheckSumBytes)}:{Start}\n");
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
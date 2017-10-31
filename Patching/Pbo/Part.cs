using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Patching.Pbo {
    public class Part {
        public Part(string path, byte[] checkSumBytes, int start, int length) {
            Path = path;
            CheckSumBytes = checkSumBytes;
            Start = start;
            Length = length;
        }

        public string Path { get; set; }
        public byte[] CheckSumBytes { private get; set; }
        public int Start { get; }
        public int Length { get; set; }

        public void Serialize(FileStream stream) {
            byte[] bytes = Encoding.UTF8.GetBytes($"{Path}:{Convert.ToBase64String(CheckSumBytes)}:{Start}:{Length}\n");
            stream.Write(bytes, 0, bytes.Length);
        }

        public static Part DeSerialize(string partString) {
            List<string> partList = partString.Split(':').ToList();
            return new Part(partList[0], Convert.FromBase64String(partList[1]), Convert.ToInt32(partList[2]), Convert.ToInt32(partList[3]));
        }

        public bool Compare(Part remotePart) => Length == remotePart.Length && CheckSumBytes.SequenceEqual(remotePart.CheckSumBytes);

        public bool Compare(string file) {
            FileInfo fileInfo = new FileInfo(file);
            byte[] bytes;
            using (BufferedStream bufferedStream = new BufferedStream(fileInfo.OpenRead(), 1200000)) {
                bytes = Utility.Generate(bufferedStream);
            }
            return CheckSumBytes.SequenceEqual(bytes);
        }
    }
}
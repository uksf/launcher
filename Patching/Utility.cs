using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Patching {
    internal static class Utility {
        public static byte[] ReadString(FileStream stream) {
            List<byte> list = new List<byte>();
            for (;;) {
                int num = stream.ReadByte();
                if (num == 0) {
                    break;
                }
                if (list.Count > 1000) {
                    throw new ArgumentOutOfRangeException(nameof(stream));
                }
                list.Add((byte) num);
            }
            return list.ToArray();
        }

        public static ulong ReadULong(FileStream stream) {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static byte[] Generate(byte[] bytes, int length) => MD5.Create().ComputeHash(bytes, 0, length <= 0 ? bytes.Length : length);

        public static string Hash(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text.Replace("\\", "").Replace("/", "")));
    }
}
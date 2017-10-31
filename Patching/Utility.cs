using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Patching {
    internal static class Utility {
        private static readonly List<char> LITERAL_LIST = new List<char> {'\'', '"', '\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v'};

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

        public static byte[] Generate(byte[] bytes, int length) {
            byte[] hashBytes;
            using (MD5 md5 = MD5.Create()) {
                hashBytes = md5.ComputeHash(bytes, 0, length <= 0 ? bytes.Length : length);
            }
            return hashBytes;
        }

        public static string Hash(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text.Replace("\\", "").Replace("/", "")));

        internal static byte[] Generate(BufferedStream bufferedStream) {
            byte[] hashBytes;
            using (MD5 md5 = MD5.Create()) {
                hashBytes = md5.ComputeHash(bufferedStream);
            }
            bufferedStream.Position = 0L;
            return hashBytes;
        }

        public static string GenerateHash(string path) {
            string hash;
            using (SHA1Managed sha1Managed = new SHA1Managed()) {
                byte[] bytes = sha1Managed.ComputeHash(Encoding.UTF8.GetBytes(path));
                StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
                foreach (byte hashByte in bytes) {
                    stringBuilder.Append(hashByte.ToString("x2"));
                }
                hash = stringBuilder.ToString();
            }
            return hash;
        }

        public static string CleanPath(string path) {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            int index = path.LastIndexOf(Path.DirectorySeparatorChar);
            if (index < 0) {
                index = path.LastIndexOf(Path.AltDirectorySeparatorChar);
            }
            if (index <= 0) {
                char[] characters = path.ToCharArray();
                CleanFileName(characters);
                return new string(characters);
            }
            char[] characters1 = path.ToCharArray(0, index);
            char[] characters2 = path.ToCharArray(index + 1, path.Length - index - 1);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < characters1.Length; i++) {
                if (!Path.GetInvalidFileNameChars().ToList().Contains(path[i]) && path[i] != Path.AltDirectorySeparatorChar) {
                    stringBuilder.Append(path[i]);
                } else if (path[i] == Path.AltDirectorySeparatorChar) {
                    stringBuilder.Append(Path.DirectorySeparatorChar);
                }
            }
            CleanFileName(characters2);
            return Path.Combine(stringBuilder.ToString(), new string(characters2));
        }

        private static void CleanFileName(IList<char> characters) {
            for (int i = 0; i < characters.Count; i++) {
                char character = characters[i];
                if (!Path.GetInvalidFileNameChars().ToList().Contains(character) && character != '*' && !LITERAL_LIST.Contains(character)) continue;
                characters[i] = (char) Math.Min(90, 'A' + character % '\u0005');
            }
        }
    }
}
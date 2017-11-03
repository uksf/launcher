using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Patching {
    public static class Utility {
        public static string ShaFromConcurrent(ConcurrentDictionary<string, string> filesDictionary) {
            SHA1 sha1 = SHA1.Create();
            foreach (byte[] bytes in from pair in filesDictionary
                                     orderby pair.Key
                                     select Enumerable.Range(0, pair.Value.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pair.Value.Substring(x, 2), 16)).ToArray()) {
                sha1.TransformBlock(bytes, 0, 16, null, 0);
            }
            sha1.TransformFinalBlock(new byte[0], 0, 0);
            StringBuilder hexString = new StringBuilder();
            foreach (string hex in from hashByte in sha1.Hash select hashByte.ToString("x2")) {
                hexString.Append(hex);
            }
            sha1.Dispose();
            return hexString.ToString();
        }

        public static string ShaFromDictionary(Dictionary<string, string> filesDictionary) {
            SHA1 sha1 = SHA1.Create();
            foreach (byte[] bytes in from pair in filesDictionary
                                     orderby pair.Key
                                     select Enumerable.Range(0, pair.Value.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pair.Value.Substring(x, 2), 16)).ToArray()) {
                sha1.TransformBlock(bytes, 0, 16, null, 0);
            }
            sha1.TransformFinalBlock(new byte[0], 0, 0);
            StringBuilder hexString = new StringBuilder();
            foreach (string hex in from hashByte in sha1.Hash select hashByte.ToString("x2")) { // TODO: WRITE TESTS FOR THIS SHITE
                hexString.Append(hex);
            }
            sha1.Dispose();
            return hexString.ToString();
        }


        public static string ShaFromFile(string path) {
            string hash;
            using (FileStream stream = File.OpenRead(path)) {
                using (SHA1Managed sha = new SHA1Managed()) {
                    byte[] bytes = sha.ComputeHash(stream);
                    StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
                    foreach (byte hashByte in bytes) {
                        stringBuilder.Append(hashByte.ToString("x2"));
                    }
                    hash = stringBuilder.ToString();
                }
            }
            return hash;
        }
    }
}
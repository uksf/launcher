using System;
using System.Data.HashFunction;
using System.IO;
using FastRsync.Hash;

namespace Patching {
    internal class SmallHashAlgorithm : IHashAlgorithm {
        private readonly xxHash _algorithm;

        public SmallHashAlgorithm() => _algorithm = new xxHash(32);

        public string Name => "XXH1";

        public int HashLength => 2;

        public byte[] ComputeHash(Stream stream) => _algorithm.ComputeHash(stream);

        public byte[] ComputeHash(byte[] buffer, int offset, int length) {
            byte[] data = new byte[length];
            Buffer.BlockCopy(buffer, offset, data, 0, length);
            return _algorithm.ComputeHash(data);
        }
    }
}
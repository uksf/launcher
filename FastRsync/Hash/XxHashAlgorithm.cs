﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace FastRsync.Hash
{
    public class XxHashAlgorithm : IHashAlgorithm
    {
        public string Name => "XXH64";
        public int HashLength => 64/8;

        private readonly System.Data.HashFunction.xxHash _algorithm;

        public XxHashAlgorithm() => _algorithm = new System.Data.HashFunction.xxHash(64);

        public byte[] ComputeHash(Stream stream) => _algorithm.ComputeHash(stream);

        public async Task<byte[]> ComputeHashAsync(Stream stream) => await _algorithm.ComputeHashAsync(stream).ConfigureAwait(false);

        public byte[] ComputeHash(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            Buffer.BlockCopy(buffer, offset, data, 0, length);
            return _algorithm.ComputeHash(data);
        }
    }
}

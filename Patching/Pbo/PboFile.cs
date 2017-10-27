using System.IO;
using System.Text;

namespace Patching.Pbo {
    internal class PboFile {
        private readonly byte[] _fileName;
        private ulong _packingMethod;
        private ulong _reserved;
        private int _size;
        private ulong _timestamp;

        public PboFile(FileStream stream) {
            _fileName = Utility.ReadString(stream);
            _packingMethod = Utility.ReadULong(stream);
            _size = (int) Utility.ReadULong(stream);
            _reserved = Utility.ReadULong(stream);
            _timestamp = Utility.ReadULong(stream);
            Datasize = Utility.ReadULong(stream);
            Size = (int) Datasize;
        }

        public int Size { get; }
        public ulong Datasize { get; }

        public bool IsNull => _fileName == null || _fileName.Length == 0;

        public override string ToString() => _fileName == null ? base.ToString() : Encoding.UTF8.GetString(_fileName);
    }
}
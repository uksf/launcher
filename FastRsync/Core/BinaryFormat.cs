using System.Text;

namespace FastRsync.Core
{
    static class BinaryFormat
    {
        public static readonly byte[] SIGNATURE_HEADER = Encoding.ASCII.GetBytes("OCTOSIG");
        public static readonly byte[] DELTA_HEADER = Encoding.ASCII.GetBytes("OCTODELTA");
        public static readonly byte[] END_OF_METADATA = Encoding.ASCII.GetBytes(">>>");
        public const byte COPY_COMMAND = 0x60;
        public const byte DATA_COMMAND = 0x80;

        public const byte VERSION = 0x01;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Patching.Pbo;

namespace Patching.Actions {
    public abstract class Action {
        internal readonly string Name;

        protected Action(string name) => Name = name;

        public abstract AddonFile GetAddonFile();
        public abstract void Consume(string pathSource, string pathDestination);

        protected static FileStream WaitForFile(string path, FileMode mode, FileAccess access, FileShare share) {
            for (int i = 0; i < 10; i++) {
                try {
                    FileStream stream = new FileStream(path, mode, access, share);
                    stream.ReadByte();
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                } catch (IOException) {
                    Thread.Sleep(50);
                }
            }
            throw new IOException("Lock not released");
        }

        public abstract List<string> GetTemporaryFiles(string sourceFolder);
    }
}
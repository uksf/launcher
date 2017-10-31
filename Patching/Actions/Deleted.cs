using System.Collections.Generic;
using System.IO;
using Patching.Pbo;

namespace Patching.Actions {
    internal class Deleted : Added {
        public Deleted(AddonFile addonFile) : base(addonFile) { }

        public override void Consume(string pathSource, string pathDestination) {
            string path = Path.Combine(pathDestination, AddonFile.RelativeFilePath);
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }

        public override List<string> GetTemporaryFiles(string sourceFolder) => new List<string>();
    }
}
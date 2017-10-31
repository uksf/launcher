using System.Collections.Generic;
using System.IO;
using System.Linq;
using Patching.Pbo;

namespace Patching.Actions {
    internal class Added : Action {
        public readonly AddonFile AddonFile;

        public Added(AddonFile addonFile) : base(addonFile.RelativeFilePath) => AddonFile = addonFile;

        public override AddonFile GetAddonFile() => AddonFile;

        public override void Consume(string pathSource, string pathDestination) {
            string path = Path.Combine(pathDestination, AddonFile.RelativeFilePath);
            if (!Directory.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            AddonFile addonFile = AddonFile;
            if (addonFile.Parts.Values.Count == 1) {
                File.Copy(Path.Combine(pathSource, addonFile.Parts.Values.First().Path), path, true);
            } else {
                using (FileStream fileStream = File.Create(path)) {
                    foreach (Part part in addonFile.Parts.Values) {
                        using (FileStream stream = WaitForFile(Path.Combine(pathSource, part.Path), FileMode.Open, FileAccess.Read, FileShare.None)) {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }

        public override List<string> GetTemporaryFiles(string sourceFolder) => AddonFile.Parts.Values.Select(part => Path.Combine(sourceFolder, part.Path)).ToList();
    }
}
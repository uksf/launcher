using System.Collections.Generic;
using System.IO;
using System.Linq;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.Game {
    public static class MallocHandler {
        public static List<Malloc> GetMalloc(string directory) {
            LogHandler.LogSeverity(Global.Severity.INFO, $"Searching {directory} for malloc DLLs");
            List<string> files = new List<string>();
            if (Directory.Exists(directory)) {
                files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                                 .Where(file => !Path.GetFileNameWithoutExtension((string) file).Contains("x64"))
                                 .ToList();
            }

            return files.Select(file => new Malloc(file)).ToList();
        }

        public class Malloc {
            public readonly string Name;

            public Malloc(string fileName) {
                Name = Path.GetFileNameWithoutExtension(fileName);
                LogHandler.LogInfo("Found malloc: " + Name);
            }
        }
    }
}

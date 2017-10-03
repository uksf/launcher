using System.Collections.Generic;
using System.IO;
using System.Linq;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    public class MallocHandler {
        /// <summary>
        ///     Finds Malloc files. Creates Malloc objects for each malloc found.
        /// </summary>
        /// <returns>List of Malloc objects for all mallocs found</returns>
        public static List<Malloc> GetMalloc(string directory) {
            List<string> files = new List<string>();
            if (Directory.Exists(directory)) {
                files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                                 .Where(file => file != null && !Path.GetFileNameWithoutExtension(file).Contains("x64")).ToList();
            }
            return files.Select(file => new Malloc(file)).ToList();
        }

        public class Malloc {
            /// <summary>
            ///     Create new malloc from given file name.
            /// </summary>
            /// <param name="fileName">File name it create Malloc from</param>
            public Malloc(string fileName) {
                FilePath = fileName;
                Name = Path.GetFileNameWithoutExtension(fileName);
                LogHandler.LogInfo("Found malloc: " + Name);
            }

            public string FilePath { get; }
            public string Name { get; }
        }
    }
}
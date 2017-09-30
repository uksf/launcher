using System;
using System.Diagnostics;
using System.IO;

namespace VersionUpdater {
    internal class Program {
        private static void Main() {
            string newVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe")).FileVersion;
            string versionFile = Path.Combine(Environment.CurrentDirectory, "version");
            if (!File.Exists(versionFile)) {
                File.Create(versionFile).Close();
            }
            using (StreamWriter writer = new StreamWriter(versionFile)) {
                writer.Write(newVersion);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace VersionUpdater {
    class Program {
        static void Main(string[] args) {
            ZipFile.ExtractToDirectory(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.zip"), Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher"));
            string newVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher", "UKSF-Launcher.exe")).FileVersion;
            string versionFile = Path.Combine(Environment.CurrentDirectory, "version");
            if (!File.Exists(versionFile)) {
                File.Create(versionFile).Close();
            }
            using (StreamWriter writer = new StreamWriter(versionFile)) {
                writer.Write(newVersion);
            }
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher"), true);
        }
    }
}

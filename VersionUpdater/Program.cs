using System;
using System.Diagnostics;
using System.IO;

namespace VersionUpdater {
    internal static class Program {
        private static void Main() {
            Version newVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe")).FileVersion);
            string flags = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe")).ProductVersion;

            string versionFile = Path.Combine(Environment.CurrentDirectory, "version");
            using (StreamWriter writer = new StreamWriter(File.Create(versionFile))) {
                writer.Write(newVersion + flags);
            }

            Process.Start("git", @"clone https://github.com/uksf/launcher.git")?.WaitForExit();
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "launcher"));
            Process.Start("git", @"fetch origin")?.WaitForExit();
            Process.Start("git", @"merge origin/release")?.WaitForExit();

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories);
            foreach (string file in files) {
                string[] lines = File.ReadAllLines(file);
                for (int index = 0; index < lines.Length; index++) {
                    if (lines[index].Contains("AssemblyVersion")) {
                        lines[index] = "[assembly: AssemblyVersion(\"" + newVersion + "\")]";
                    }
                }
                File.WriteAllLines(file, lines);
            }
            string[] appveyorLines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "appveyor.yml"));
            appveyorLines[0] = "version: \"" + newVersion.Major + "." + newVersion.Minor + "." + newVersion.Build + ".{build}\"";
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "appveyor.yml"), appveyorLines);
            Process.Start("git", @"commit -am ""Release version: " + newVersion + "\"")?.WaitForExit();
            Process.Start("git", @"push")?.WaitForExit();

            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            SetAttributes(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "launcher")));
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "launcher"), true);
        }

        private static void SetAttributes(DirectoryInfo directory) {
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) {
                SetAttributes(subDirectory);
            }
            foreach (FileInfo file in directory.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }
}
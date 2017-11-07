using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Deploy {
    internal static class Program {
        private static void Main() {
            // Update version
            Version newVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "Launcher.exe")).FileVersion);
            string flags = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "Launcher.exe")).ProductVersion;
            
            using (StreamWriter writer = new StreamWriter(File.Create(@"C:\wamp\www\uksfnew\public\launcher\version"))) {
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
            
            // Copy Setup.msi
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Setup.msi"), @"C:\wamp\www\uksfnew\public\launcher\Setup.msi");

            // Restart Service
            ServiceController serviceController = new ServiceController {ServiceName = "ServerService"};
            serviceController.Stop();
            File.Copy(Path.Combine(Environment.CurrentDirectory, "ServerService.exe"), Path.Combine(Environment.CurrentDirectory, "service", "ServerService.exe"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Patching.dll"), Path.Combine(Environment.CurrentDirectory, "service", "Patching.dll"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Network.dll"), Path.Combine(Environment.CurrentDirectory, "service", "Network.dll"));
            File.Copy(Path.Combine(Environment.CurrentDirectory, "FastRsync.dll"), Path.Combine(Environment.CurrentDirectory, "service", "FastRsync.dll"));
            serviceController.Start();
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

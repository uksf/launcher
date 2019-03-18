using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace UKSF.Launcher.Deploy {
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
            Process.Start("git", @"commit -am ""Released version: " + newVersion + "\"")?.WaitForExit();
            Process.Start("git", @"push")?.WaitForExit();

            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            SetAttributes(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "launcher")));
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "launcher"), true);

            // Restart Service
            ServiceController serviceController = new ServiceController {ServiceName = "ServerService"};
            if (serviceController.CanStop) {
                serviceController.Stop();
            }
            while (serviceController.Status != ServiceControllerStatus.Stopped) {
                Task.Delay(100).Wait();
                serviceController.Refresh();
            }
            File.Copy(Path.Combine(Environment.CurrentDirectory, "ServerService.exe"), Path.Combine(Environment.CurrentDirectory, "..", "service", "ServerService.exe"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "ServerConsole.exe"), Path.Combine(Environment.CurrentDirectory, "..", "service", "ServerConsole.exe"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Patching.dll"), Path.Combine(Environment.CurrentDirectory, "..", "service", "Patching.dll"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Network.dll"), Path.Combine(Environment.CurrentDirectory, "..", "service", "Network.dll"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "FastRsync.dll"), Path.Combine(Environment.CurrentDirectory, "..", "service", "FastRsync.dll"), true);
            serviceController.Start();

            // Copy and sign Setup.msi
            File.Copy(Path.Combine(Environment.CurrentDirectory, "Setup.msi"), @"C:\wamp\www\uksfnew\public\launcher\Setup.msi", true);
            Process.Start(@"C:\Users\root\Documents\Code Signing\signtool.exe", @"sign /f ""C:\Users\root\Documents\Code Signing\uksf.pfx"" /p 1 /d ""UKSF Launcher Setup"" /t http://timestamp.verisign.com/scripts/timstamp.dll /v ""C:\wamp\www\uksfnew\public\launcher\Setup.msi""")?.WaitForExit();
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

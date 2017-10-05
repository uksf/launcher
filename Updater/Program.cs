using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Updater {
    internal static class Program {
        private static void Main() {
            foreach (Process process in Process.GetProcessesByName("UKSF-Launcher")) {
                process.WaitForExit(2500);
                if (!process.HasExited) {
                    process.Kill();
                }
            }

            string launcher = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe");
            File.SetAttributes(launcher, FileAttributes.Normal);
            File.Delete(launcher);
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/UKSF-Launcher.exe", launcher);
            Process launcherProcess = new Process {
                StartInfo = {UseShellExecute = false, FileName = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"), Arguments = "-u"}
            };
            launcherProcess.Start();
            Environment.Exit(0);
        }
    }
}
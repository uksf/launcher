using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Updater {
    internal static class Program {
        private static void Main() {
            Process[] processes = Process.GetProcessesByName("UKSF-Launcher");
            while (processes.Length > 0) {
                foreach (Process process in processes) {
                    process.WaitForExit(500);
                    if (!process.HasExited) {
                        process.Kill();
                    }
                }
                processes = Process.GetProcessesByName("UKSF-Launcher");
            }
            Thread.Sleep(500);
            
            string launcher = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe");
            string patching = Path.Combine(Environment.CurrentDirectory, "Patching.dll");
            string network = Path.Combine(Environment.CurrentDirectory, "Network.dll");
            if (File.Exists(launcher)) {
                File.SetAttributes(launcher, FileAttributes.Normal);
                File.Delete(launcher);
            }
            if (File.Exists(patching)) {
                File.SetAttributes(patching, FileAttributes.Normal);
                File.Delete(patching);
            }
            if (File.Exists(network)) {
                File.SetAttributes(network, FileAttributes.Normal);
                File.Delete(network);
            }

            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/UKSF-Launcher.exe", launcher);
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Patching.dll", patching);
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Network.dll", network);

            Process launcherProcess = new Process {
                StartInfo = {UseShellExecute = false, FileName = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"), Arguments = "-u"}
            };
            launcherProcess.Start();
            Environment.Exit(0);
        }
    }
}
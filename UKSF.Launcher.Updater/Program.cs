using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UKSF.Launcher.Updater {
    internal static class Program {
        private const string URL = "arma.uk-sf.com";

        private static void Main() {
            Task.Run(async () => {
                    Process[] processes = Process.GetProcessesByName("Launcher");
                    while (processes.Length > 0) {
                        foreach (Process process in processes) {
                            process.WaitForExit(500);
                            if (!process.HasExited) {
                                process.Kill();
                            }
                        }

                        processes = Process.GetProcessesByName("Launcher");
                    }

                    await Task.Delay(500);

                    foreach (string filePath in new[] {"UKSF Launcher.exe", "UKSF.Old.Launcher.Patching.dll", "UKSF.Old.Launcher.Network.dll", "UKSF.Old.Launcher.FastRsync.dll"}.Select(file => Path.Combine(Environment.CurrentDirectory, file)).Where(File.Exists)) {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }

//                    using (WebClient webClient = new WebClient()) {
//                        webClient.Credentials = new NetworkCredential("launcherdeploy", "sikrit");
//                        webClient.DownloadFile($"ftp://{URL}/release/Launcher.exe", launcher);
//                        webClient.DownloadFile($"ftp://{URL}/release/Patching.dll", patching);
//                        webClient.DownloadFile($"ftp://{URL}/release/Network.dll", network);
//                        webClient.DownloadFile($"ftp://{URL}/release/FastRsync.dll", fastrsync);
//                    }
                    using (HttpClient client = new HttpClient()) {
                        // TODO: Get launcher files
                    }

                    Process launcherProcess = new Process {
                        StartInfo = {UseShellExecute = false, FileName = Path.Combine(Environment.CurrentDirectory, "Launcher.exe"), Arguments = "-u"}
                    };
                    launcherProcess.Start();
                    Environment.Exit(0);
                })
                .Wait();
        }
    }
}

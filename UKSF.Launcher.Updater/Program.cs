using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKSF.Launcher.Updater {
    internal static class Program {
#if DEBUG
        public const string URL = "http://localhost:5000";
#else
        public const string URL = "https://api.uk-sf.co.uk";
#endif

        private static string token;
        private static string launcherDirectory;

        private static void Main(string[] args) {
            if (string.IsNullOrEmpty(args[0])) {
                return;
            }

            token = args[0];
            launcherDirectory = Environment.CurrentDirectory;

            Task.Run(async () => {
                    await CloseLauncher();
                    await Update();
                    Relaunch();
                })
                .Wait();
        }

        private static async Task CloseLauncher() {
            Process[] processes = Process.GetProcessesByName("UKSF Launcher");
            while (processes.Length > 0) {
                foreach (Process process in processes) {
                    process.WaitForExit(500);
                    process.Refresh();
                    if (!process.HasExited) {
                        process.Kill();
                    }
                }

                processes = Process.GetProcessesByName("UKSF Launcher");
            }

            await Task.Delay(500);
        }

        private static async Task Update() {
            List<LauncherFile> currentFiles = (from file in Directory.EnumerateFiles(launcherDirectory) let fileName = Path.GetFileName(file) let version = FileVersionInfo.GetVersionInfo(file).FileVersion select new LauncherFile {fileName = fileName, version = version}).ToList();
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(currentFiles), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{URL}/launcher/download/update", content);
                response.EnsureSuccessStatusCode();
                Stream stream = await response.Content.ReadAsStreamAsync();
            }
        }

        private static void Relaunch() {
            Process launcherProcess = new Process {StartInfo = {UseShellExecute = false, FileName = Path.Combine(launcherDirectory, "Launcher.exe"), Arguments = "-u"}};
            launcherProcess.Start();
            Environment.Exit(0);
        }
    }

    internal class LauncherFile {
        public string fileName;
        public string version;
    }
}


//                    foreach (string filePath in new[] {"UKSF Launcher.exe", "UKSF.Old.Launcher.Patching.dll", "UKSF.Old.Launcher.Network.dll", "UKSF.Old.Launcher.FastRsync.dll"}.Select(file => Path.Combine(Environment.CurrentDirectory, file)).Where(File.Exists)) {
//                        File.SetAttributes(filePath, FileAttributes.Normal);
//                        File.Delete(filePath);
//                    }
//
////                    using (WebClient webClient = new WebClient()) {
////                        webClient.Credentials = new NetworkCredential("launcherdeploy", "sikrit");
////                        webClient.DownloadFile($"ftp://{URL}/release/Launcher.exe", launcher);
////                        webClient.DownloadFile($"ftp://{URL}/release/Patching.dll", patching);
////                        webClient.DownloadFile($"ftp://{URL}/release/Network.dll", network);
////                        webClient.DownloadFile($"ftp://{URL}/release/FastRsync.dll", fastrsync);
////                    }
//                    using (HttpClient client = new HttpClient()) {
//                        // TODO: Get launcher files
//                    }

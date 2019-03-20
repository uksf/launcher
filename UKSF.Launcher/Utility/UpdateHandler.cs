using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UKSF.Launcher.Network;

namespace UKSF.Launcher.Utility {
    internal static class UpdateHandler {
        //private static HubConnection hubConnection;
        
        public static async void UpdateCheck(bool updated) {
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Checking for update");
            Global.Settings.VERSION = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            LogHandler.LogInfo($"Current version: {Global.Settings.VERSION}");

            try {
                string version = await ApiWrapper.Get("launcher/version");
                Version latestVersion = Version.Parse(version);
                bool force = latestVersion.Major > Global.Settings.VERSION.Major;
                LogHandler.LogInfo($"Latest version: {latestVersion} - Force update: {force}");

                if (Global.Settings.Autoupdatelauncher && Global.Settings.VERSION < latestVersion || force) {
                    LogHandler.LogInfo($"Updating to {latestVersion}");
                    await Update();
                }

                if (updated) {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
                }

                ConnectHub();
            } catch (Exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to get remote version information");
            }
        }

        private static async Task Update() {
            string path = Path.Combine(Environment.CurrentDirectory, "UKSF.Launcher.Updater.exe");
            using (Stream stream = await ApiWrapper.GetFile("launcher/download/updater")) {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    await stream.CopyToAsync(fileStream);
                }
            }
            
            try {
                Process updater = new Process {StartInfo = {Arguments = "", UseShellExecute = false, FileName = path, CreateNoWindow = false}};
                updater.Start();
                Core.ShutDown();
            } catch (Exception exception) {
                Core.Error(exception);
            }
        }

        private static void ConnectHub() {
            //hubConnection = new HubConnection();
            //UKSF.Launcher.NetworkCore.
        }
    }
}

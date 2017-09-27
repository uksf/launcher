using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    class UpdateHandler {

        public static void UpdateCheck(bool updated) {
            if (!AUTOUPDATE) return;
            LogHandler.LogHashSpace();
            Version currentVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            Version latestVersion = Version.Parse(new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version"));
            LogHandler.LogInfo("Current version: " + currentVersion);
            LogHandler.LogInfo("Latest version: " + latestVersion);
#if FORCEUPDATE
            currentVersion = Version.Parse("0.0.0");
            LogHandler.LogSeverity(Severity.INFO, "Force version: " + currentVersion);
#endif
            if (currentVersion < latestVersion) {
                LogHandler.LogInfo("Updating to " + latestVersion);
                Update();
            }
            if (updated) {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            }
        }

        private static void Update() {
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Updater.exe", Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            Process updater = new Process();
            try {
                updater.StartInfo.UseShellExecute = false;
                updater.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Updater.exe");
                updater.StartInfo.CreateNoWindow = false;
                updater.Start();
                Application.Current.Shutdown();
            }
            catch (Exception exception) {
                Core.Error(exception);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher.Utility {
    class UpdateHandler {

        public static void UpdateCheck() {
            if (!AUTOUPDATE) return;
            Version currentVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            Version latestVersion = Version.Parse(new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version"));
            LogHandler.LogSeverity(Severity.INFO, "Current version: " + currentVersion);
            LogHandler.LogSeverity(Severity.INFO, "Latest version: " + latestVersion);
#if FORCEUPDATE
            currentVersion = Version.Parse("0.0.0");
            LogHandler.LogSeverity(Severity.INFO, "Force version: " + currentVersion);
#endif
            if (currentVersion < latestVersion) {
                LogHandler.LogSeverity(Severity.INFO, "Updating to " + latestVersion);
                Update();
            }
            if (UPDATER) {
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

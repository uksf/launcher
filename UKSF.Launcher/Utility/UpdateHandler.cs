using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace UKSF.Launcher.Utility {
    internal static class UpdateHandler {
        public static void UpdateCheck(bool updated) {
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Checking for update");
            Global.Settings.VERSION = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            string[] currentFlags = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion.Split(':');
            LogHandler.LogInfo($"Current version: {Global.Settings.VERSION}");

            try {
                WebClient webClient = new WebClient();
                string[] versionString = webClient.DownloadString("http://arma.uk-sf.com/launcher/version").Split(':');
                // TODO: Get version

                Version latestVersion = Version.Parse(versionString[0]);
                bool force = HandleFlags(versionString, currentFlags);
                LogHandler.LogInfo($"Latest version: {latestVersion} - Force update: {force}");

                if ((Global.Settings.Autoupdatelauncher || force) && Global.Settings.VERSION < latestVersion) {
                    LogHandler.LogInfo($"Updating to {latestVersion}");
                    Update();
                }

                if (updated) {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
                }
            } catch (Exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to get remote version information");
            }
        }

        public static bool HandleFlags(IReadOnlyList<string> newFlags, IReadOnlyList<string> currentFlags) {
            bool force = newFlags[1].Equals(Global.Constants.UPDATE_FLAG_FORCE) && !currentFlags[1].Equals(Global.Constants.UPDATE_FLAG_FORCE);
            if (newFlags[2].Equals(Global.Constants.UPDATE_FLAG_RESET) && !currentFlags[2].Equals(Global.Constants.UPDATE_FLAG_RESET)) {
                LogHandler.LogHashSpaceMessage(Global.Severity.WARNING, "Resetting all settings");
                Core.ResetSettings();
                force = true;
            }

            if (!newFlags[3].Equals(Global.Constants.UPDATE_FLAG_CLEAN) || currentFlags[3].Equals(Global.Constants.UPDATE_FLAG_CLEAN)) return force;
            LogHandler.LogHashSpaceMessage(Global.Severity.WARNING, "Cleaning settings");
            Core.CleanSettings();
            return force;
        }

        private static void Update() {
//            using (WebClient webClient = new WebClient()) {
//                webClient.Credentials = new NetworkCredential("launcherdeploy", "sikrit");
//                webClient.DownloadFile("ftp://arma.uk-sf.com/Updater.exe", Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
//            }
            // TODO: Get updater file

            Process updater = new Process();
            try {
                updater.StartInfo.UseShellExecute = false;
                updater.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Updater.exe");
                updater.StartInfo.CreateNoWindow = false;
                updater.Start();
                Core.ShutDown();
            } catch (Exception exception) {
                Core.Error(exception);
            }
        }
    }
}

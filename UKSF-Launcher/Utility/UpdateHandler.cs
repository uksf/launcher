using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.Utility {
    internal static class UpdateHandler {

        /// <summary>
        ///     Checks for a new version of the launcher from the website.
        /// </summary>
        /// <param name="updated">Determines if the launcher has been updated</param>
        public static void UpdateCheck(bool updated) {
            LogHashSpaceMessage(Severity.INFO, "Checking for update");
            VERSION = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            string[] currentFlags = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion.Split(':');
            LogInfo($"Current version: {VERSION}");

            string[] versionString = new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version").Split(':');
            Version latestVersion = Version.Parse(versionString[0]);
            bool force = HandleFlags(versionString, currentFlags);
            LogInfo($"Latest version: {latestVersion} - Force update: {force}");

            if ((AUTOUPDATELAUNCHER || force) && VERSION < latestVersion) {
                LogInfo($"Updating to {latestVersion}");
                Update();
            }
            if (updated) {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            }
        }

        /// <summary>
        ///     Checks and handles remote version and current version flags.
        /// </summary>
        /// <param name="newFlags">Array of remote version flags</param>
        /// <param name="currentFlags">Array of current version flags</param>
        /// <returns>Force update</returns>
        public static bool HandleFlags(IReadOnlyList<string> newFlags, IReadOnlyList<string> currentFlags) {
            bool force = newFlags[1].Equals(UPDATE_FLAG_FORCE) && !currentFlags[1].Equals(UPDATE_FLAG_FORCE);
            if (newFlags[2].Equals(UPDATE_FLAG_RESET) && !currentFlags[2].Equals(UPDATE_FLAG_RESET)) {
                LogHashSpaceMessage(Severity.WARNING, "Resetting all settings");
                Core.ResetSettings();
                force = true;
            }
            if (newFlags[3].Equals(UPDATE_FLAG_CLEAN) && !currentFlags[3].Equals(UPDATE_FLAG_CLEAN)) {
                LogHashSpaceMessage(Severity.WARNING, "Cleaning settings");
                Core.CleanSettings();
            }
            return force;
        }

        /// <summary>
        ///     Downloads the latest updater file and runs it. Running instance of Launcher will shutdown.
        /// </summary>
        private static void Update() {
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Updater.exe", Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
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
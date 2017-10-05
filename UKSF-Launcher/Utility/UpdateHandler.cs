﻿using System;
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
            string[] oldFlags = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion.Split(':');
            LogInfo($"Current version: {VERSION}");

            string[] versionString = new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version").Split(':');
            Version latestVersion = Version.Parse(versionString[0]);
            bool force = HandleFlags(versionString, oldFlags);
            LogInfo($"Latest version: {latestVersion} - Force update: {force}");

            if ((AUTOUPDATELAUNCHER || force) && VERSION < latestVersion) {
                LogInfo($"Updating to {latestVersion}");
                Update();
            }
            if (updated) {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            }
        }

        private static bool HandleFlags(IReadOnlyList<string> flags, IReadOnlyList<string> oldFlags) {
            bool force = flags[1].Equals("F") && !oldFlags[1].Equals("F");
            if (flags[2].Equals("R") && !oldFlags[2].Equals("R")) {
                LogHashSpaceMessage(Severity.INFO, "Resetting all settings");
                Core.SettingsHandler.ResetSettings();
                force = true;
            }
            if (flags[3].Equals("C") && !oldFlags[3].Equals("C")) {
                LogHashSpaceMessage(Severity.INFO, "Cleaning settings");
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
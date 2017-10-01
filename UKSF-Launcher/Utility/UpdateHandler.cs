﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    internal static class UpdateHandler {
        /// <summary>
        ///     Checks for a new version of the launcher from the website.
        /// </summary>
        /// <param name="updated">Determines if the launcher has been updated</param>
        public static void UpdateCheck(bool updated) {
            LogHandler.LogHashSpace();
            VERSION = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            LogHandler.LogInfo("Current version: " + VERSION);
            if (!AUTOUPDATELAUNCHER) return;
            Version latestVersion = Version.Parse(new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version"));
            LogHandler.LogInfo("Latest version: " + latestVersion);
#if FORCEUPDATE
            VERSION = Version.Parse("0.0.0.0");
            LogHandler.LogSeverity(Severity.INFO, "Force version: " + VERSION);
#endif
            if (VERSION < latestVersion) {
                LogHandler.LogInfo("Updating to " + latestVersion);
                Update();
            }
            if (updated) {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            }
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
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.Game {
    internal static class GameHandler {
        // Registry key locaiton for arma installation
        private const string REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";

        /// <summary>
        ///     Finds the game installation from registry and selects the best exe to use. Will return an empty string if no exe or
        ///     installation found.
        /// </summary>
        /// <returns>Path to game exe. Empty string if no installation</returns>
        public static string GetGameInstallation() {
            string location = "";
            RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(REGISTRY);
            if (gameKey == null) return location;
            string install = gameKey.GetValue("main", "").ToString();
            if (!Directory.Exists(install)) return location;
            string exe = Path.Combine(install, IS64BIT ? "arma3_x64.exe" : "arma3.exe");
            if (File.Exists(exe)) {
                location = exe;
            }
            return location;
        }

        /// <summary>
        ///     Checks the drive space in bytes against the required drive space for the given path.
        /// </summary>
        /// <param name="path">Path to check drive space</param>
        /// <returns>True if enough space on drive</returns>
        public static bool CheckDriveSpace(string path) {
            return DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot(path))).ElementAt(0).AvailableFreeSpace >= REQUIREDSPACE;
        }

        /// <summary>
        ///     Starts the game.
        /// </summary>
        public static void StartGame() {
            LogHashSpaceMessage(Severity.INFO, "Starting game...");
            Process game = new Process();
            try {
                game.StartInfo.UseShellExecute = false;
                game.StartInfo.FileName = GAME_LOCATION;
                game.StartInfo.Arguments = GetStartupParameters();
                LogInfo($"Startup Parameters {game.StartInfo.Arguments}");
                game.Start();
            }
            catch (Exception exception) {
                Core.Error(exception);
            }
        }

        /// <summary>
        /// Compiles startup parameter variables into a startup string.
        /// </summary>
        /// <returns>String of parameters</returns>
        private static string GetStartupParameters() {
            StringBuilder startupString = new StringBuilder();
            startupString.Append($"-name={PROFILE}");
            startupString.Append(STARTUP_NOSPLASH ? " -nosplash" : "");
            startupString.Append(STARTUP_EMPTYWORLD ? " -world=empty" : "");
            startupString.Append(STARTUP_SCRIPTERRORS ? " -showScriptErrors" : "");
            startupString.Append(STARTUP_FILEPATCHING ? " -filePatching" : "");
            startupString.Append(STARTUP_HUGEPAGES ? " -hugepages" : "");
            if (STARTUP_MALLOC != MALLOC_SYSTEM_DEFAULT) {
                startupString.Append(GAME_LOCATION.Contains("x64") ? $" -malloc={STARTUP_MALLOC}_x64" : $" -malloc={STARTUP_MALLOC}");
            } else {
                startupString.Append(" -malloc=system");
            }
            return startupString.ToString();
        }
    }
}
using System.IO;
using System.Linq;
using Microsoft.Win32;

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
            string exe = Path.Combine(install, Global.IS64BIT ? "arma3_x64.exe" : "arma3.exe");
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
            return DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot(path))).ElementAt(0).AvailableFreeSpace >= Global.REQUIREDSPACE;
        }
    }
}
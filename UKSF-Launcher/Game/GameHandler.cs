using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace UKSF_Launcher.Game {
    internal class GameHandler {
        private const string REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";

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

        public static bool CheckDriveSpace(string path) {
            return DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot(path))).ElementAt(0).AvailableFreeSpace >= Global.REQUIREDSPACE;
        }
    }
}
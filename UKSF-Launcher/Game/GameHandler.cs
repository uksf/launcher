using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace UKSF_Launcher.Game {
    class GameHandler {

        private const string REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";

        public static string GetGameInstallation() {
            string location = "";
            RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(REGISTRY);
            if (gameKey != null) {
                string install = gameKey.GetValue("main", "").ToString();
                if (Directory.Exists(install)) {
                    string exe = Path.Combine(install, Global.IS64BIT ? "arma3_x64.exe" : "arma3.exe");
                    if (File.Exists(exe)) {
                        location = exe;
                    }
                }
            }
            return location;
        }

        public static bool CheckDriveSpace(string path) {
            string driveLetter = Path.GetPathRoot(path);
            DriveInfo info = DriveInfo.GetDrives().Where(drive => drive.Name.Equals(driveLetter)).ElementAt(0);
            if (info.AvailableFreeSpace >= Global.REQUIREDSPACE) {
                return true;
            }
            return false;
        }
    }
}

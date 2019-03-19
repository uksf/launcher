using System;
using System.Diagnostics;
using System.IO;
using UKSF.Launcher.Network;
using UKSF.Launcher.Patching;

namespace UKSF.Launcher {
    public static class Global {
        // Logging severity
        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public static Process GameProcess = null;

        public static RepoClient Repo = null;

        public class Constants {
            // Logging data format
            public const string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";

            // Logging time format
            public const string FORMAT_TIME = "HH:mm:ss";

            // Registry key location for arma installation
            public const string GAME_REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";

            // Display name for system default memory allocator
            public const string MALLOC_SYSTEM_DEFAULT = "System Default";
            // Settings registry key name
            public const string REGSITRY = @"SOFTWARE\UKSF Launcher";

            // Required drive space for mods
            public const long REQUIREDSPACE = 32212254720; // ~30GB // TODO: Get modpack size from api

            // Appdata directory path
            public static readonly string APPDATA = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UKSF Launcher");

            // Logging spacer
            public static readonly string HASHSPACE = Environment.NewLine + "##########################################################################################";

            // 64-bit OS
            public static readonly bool IS64_BIT = Environment.Is64BitOperatingSystem;

            // Double new line
            public static readonly string NL = Environment.NewLine + Environment.NewLine;

            // Folder name of default profile location
            public static readonly string PROFILE_LOCATION_DEFAULT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3");

            // Folder name of other profiles location
            public static readonly string PROFILE_LOCATION_OTHER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3 - Other Profiles");
        }


        public class Settings {
            // Automatically update the launcher
            public static bool Autoupdatelauncher = true;

            // First time setup complete
            public static bool Firsttimesetupdone = false;

            // Login email
            public static string LoginEmail = "";

            // Game exe path
            public static string GameLocation = "";

            // Mod download path
            public static string ModLocation = "";

            // Game profile
            public static string Profile = "";

            // Server
            public static Server Server = null;

            // File patching
            public static bool StartupFilepatching = true;

            // Huge pages
            public static bool StartupHugepages = true;

            // Memory Allocator
            public static string StartupMalloc = "";

            // No splash screen
            public static bool StartupNosplash = true;

            // Show script errors
            public static bool StartupScripterrors = false;

            // Launcher version
            public static Version VERSION = Version.Parse("0.0.0.0");
        }
    }
}

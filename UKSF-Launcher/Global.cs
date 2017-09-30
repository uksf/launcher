using System;

namespace UKSF_Launcher {
    public class Global {

        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public static Version VERSION = Version.Parse("0.0.0");

        public static bool IS64BIT = Environment.Is64BitOperatingSystem;
        public static long REQUIREDSPACE = 32212254720; // ~30GB
        public static string NL = Environment.NewLine + Environment.NewLine;

        // Launcher
        public static bool FIRSTTIMESETUPDONE = false;
        public static bool AUTOUPDATELAUNCHER = true;

        // Game
        public static string GAME_LOCATION = "";
        public static string MOD_LOCATION = "";
        public static string PROFILE = "";
    }
}

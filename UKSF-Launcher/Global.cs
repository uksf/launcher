using System;

// ReSharper disable InconsistentNaming

namespace UKSF_Launcher {
    public static class Global {
        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public const long REQUIREDSPACE = 32212254720; // ~30GB

        public static readonly string NL = Environment.NewLine + Environment.NewLine;

        public static Version VERSION = Version.Parse("0.0.0.0");

        public static readonly bool IS64BIT = Environment.Is64BitOperatingSystem;

        // Launcher
        public static bool FIRSTTIMESETUPDONE = false;

        public static bool AUTOUPDATELAUNCHER = true;

        // Game
        public static string GAME_LOCATION = "";

        public static string MOD_LOCATION = "";
        
        public static string PROFILE = "";
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

// ReSharper disable InconsistentNaming

namespace UKSF_Launcher {
    [ExcludeFromCodeCoverage]
    public static class Global {
        // Logging severity
        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        // Required drive space for mods
        public const long REQUIREDSPACE = 32212254720; // ~30GB

        // Double new line
        public static readonly string NL = Environment.NewLine + Environment.NewLine;

        // 64-bit OS
        public static readonly bool IS64BIT = Environment.Is64BitOperatingSystem;

        // -------------- Launcher -------------- \\
        // First time setup complete
        public static bool FIRSTTIMESETUPDONE = false;

        // Automatically update the launcher
        public static bool AUTOUPDATELAUNCHER = true;

        // Launcher version
        public static Version VERSION = Version.Parse("0.0.0.0");

        // ---------------- Game ---------------- \\
        // Game exe path
        public static string GAME_LOCATION = "";

        // Mod download path
        public static string MOD_LOCATION = "";

        // Game profile
        public static string PROFILE = "";

        // Folder name of default profile location
        public static string LOCATION_DEFAULT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3");

        // Folder name of other profiles location
        public static string LOCATION_OTHER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3 - Other Profiles");
    }
}
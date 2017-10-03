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

        // Display name for system default memory allocator
        public const string MALLOC_SYSTEM_DEFAULT = "System Default";

        // Double new line
        public static readonly string NL = Environment.NewLine + Environment.NewLine;

        // 64-bit OS
        public static readonly bool IS64BIT = Environment.Is64BitOperatingSystem;

        // Folder name of default profile location
        public static readonly string PROFILE_LOCATION_DEFAULT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3");

        // Folder name of other profiles location
        public static readonly string PROFILE_LOCATION_OTHER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3 - Other Profiles");

        // ######################################################################### Settings ######################################################################### \\

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

        // ---------------- Startup parameters ---------------- \\
        // No splash screen
        public static bool STARTUP_NOSPLASH = true;

        // Empty world
        public static bool STARTUP_EMPTYWORLD = true;

        // Show script errors
        public static bool STARTUP_SCRIPTERRORS = false;

        // File patching
        public static bool STARTUP_FILEPATCHING = true;

        // Huge pages
        public static bool STARTUP_HUGEPAGES = true;

        // Memory Allocator
        public static string STARTUP_MALLOC = "";

        // ---------------- Additional Mods ---------------- \\
        // ShackTac HUD
        public static bool MODS_SHACKTAC = true;
    }
}
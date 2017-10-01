using Microsoft.Win32;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    internal static class SettingsHandler {
        // Registry key location for storing settings
        private const string REGISTRY = @"SOFTWARE\UKSF-Launcher";

        /// <summary>
        ///     Reads all settings fromthe registry.
        /// </summary>
        public static void ReadSettings() {
            LogHandler.LogHashSpace();

            // Launcher
            FIRSTTIMESETUPDONE = ParseSetting("FIRSTTIMESETUPDONE", false);
            AUTOUPDATELAUNCHER = ParseSetting("AUTOUPDATELAUNCHER", true);

            // Games
            GAME_LOCATION = ParseSetting("GAME_LOCATION", "");
            MOD_LOCATION = ParseSetting("MOD_LOCATION", "");
            PROFILE = ParseSetting("PROFILE", "");
        }

        /// <summary>
        ///     Gets the registry key. Creates it if it does not exist.
        /// </summary>
        /// <returns>Registry key object</returns>
        private static RegistryKey GetRegistryKey() => Registry.CurrentUser.OpenSubKey(REGISTRY, true) ?? Registry.CurrentUser.CreateSubKey(REGISTRY, true);

        /// <summary>
        ///     Reads a settings from the registry. If it does not exist, write the key to the registry with the default value.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a string</returns>
        public static string ReadSetting(string name, object defaultValue) {
            RegistryKey registry = GetRegistryKey();
            object value = registry.GetValue(name);
            if (value == null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            LogHandler.LogInfo("Reading setting '" + name + "': " + value);
            return value.ToString();
        }

        /// <summary>
        ///     Writes the value to the key in the registry.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="value">Setting value</param>
        /// <returns>Value written to registry key</returns>
        public static object WriteSetting(string name, object value) {
            RegistryKey registry = GetRegistryKey();
            LogHandler.LogInfo("Writing setting '" + name + "': " + value);
            registry.SetValue(name, value);
            return value;
        }

        /// <summary>
        ///     Read given setting and parse as a string.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a string</returns>
        public static string ParseSetting(string name, string defaultValue) => ReadSetting(name, defaultValue);

        /// <summary>
        ///     Read given setting and parse as an integer.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as an integer</returns>
        public static int ParseSetting(string name, int defaultValue) => int.Parse(ReadSetting(name, defaultValue));

        /// <summary>
        ///     Read given setting and parse as a boolean.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a boolean</returns>
        public static bool ParseSetting(string name, bool defaultValue) => bool.Parse(ReadSetting(name, defaultValue));

        /// <summary>
        ///     Delete registry key.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        public static void DeleteSetting(string name) {
            RegistryKey registry = GetRegistryKey();
            if (registry.GetValue(name) != null) {
                registry.DeleteValue(name);
            }
        }
    }
}
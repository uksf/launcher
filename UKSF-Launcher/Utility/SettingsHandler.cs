using Microsoft.Win32;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    class SettingsHandler {

        private const string REGISTRY = @"SOFTWARE\UKSF-Launcher";

        private static RegistryKey registry;

        public static void ReadSettings() {
            LogHandler.LogHashSpace();
            registry = Registry.CurrentUser.OpenSubKey(REGISTRY, true);
            if (registry == null) {
                registry = Registry.CurrentUser.CreateSubKey(REGISTRY, true);
            }
            
            AUTOUPDATE = ParseSetting("AUTOUPDATE", true);
            PROFILE = ParseSetting("PROFILE", "");
        }

        public static string ReadSetting(string name, object defaultValue) {
            object value = registry.GetValue(name);
            if (value == null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            LogHandler.LogInfo("Reading setting '" + name + "': " + value);
            return value.ToString();
        }

        public static object WriteSetting(string name, object value) {
            LogHandler.LogInfo("Writing setting '" + name + "': " + value);
            registry.SetValue(name, value);
            return value;
        }

        public static string ParseSetting(string name, string defaultValue) {
            return ReadSetting(name, defaultValue);
        }

        public static int ParseSetting(string name, int defaultValue) {
            return int.Parse(ReadSetting(name, defaultValue));
        }

        public static bool ParseSetting(string name, bool defaultValue) {
            return bool.Parse(ReadSetting(name, defaultValue));
        }
    }
}

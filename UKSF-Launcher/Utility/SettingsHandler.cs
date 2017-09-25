using Microsoft.Win32;
using System;
using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher.Utility {
    class SettingsHandler {

        private static RegistryKey registry;

        public static void ReadSettings() {
            registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\UKSF-Launcher");
            if (registry == null) {
                registry = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UKSF-Launcher");
            }
            
            AUTOUPDATE = ReadSetting("AUTOUPDATE", true);
        }

        public static string ReadSetting(string name, string defaultValue) {
            LogHandler.LogSeverity(Severity.INFO, "Reading setting '" + name + "'");
            object value = registry.GetValue(name);
            if (value == null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            return value.ToString();
        }

        public static int ReadSetting(string name, int defaultValue) {
            LogHandler.LogSeverity(Severity.INFO, "Reading setting '" + name + "'");
            object value = registry.GetValue(name);
            if (value == null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            return int.Parse(value.ToString());
        }

        public static bool ReadSetting(string name, bool defaultValue) {
            LogHandler.LogSeverity(Severity.INFO, "Reading setting '" + name + "'");
            object value = registry.GetValue(name);
            if (value == null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            return bool.Parse(value.ToString());
        }

        public static void WriteSetting(string name, object value) {
            LogHandler.LogSeverity(Severity.INFO, "Writing setting '" + name + "': " + value);
            registry.SetValue(name, value);
        }
    }
}

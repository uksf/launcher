using Microsoft.Win32;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.Utility {
    public class SettingsHandler {
        // Registry key location
        private readonly string _registry;

        public SettingsHandler(string registryKey) => _registry = registryKey;

        /// <summary>
        ///     Gets the registry key. Creates it if it does not exist.
        /// </summary>
        /// <returns>Registry key object</returns>
        private RegistryKey GetRegistryKey() => Registry.CurrentUser.OpenSubKey(_registry, true) ?? Registry.CurrentUser.CreateSubKey(_registry, true);

        /// <summary>
        ///     Reads a settings from the registry. If it does not exist, write the key to the registry with the default value.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a string</returns>
        public string ReadSetting(string name, object defaultValue = null) {
            RegistryKey registry = GetRegistryKey();
            object value = registry.GetValue(name);
            if (value == null && defaultValue != null) {
                value = defaultValue;
                WriteSetting(name, value);
            }
            LogInfo("Reading setting '" + name + "': " + value);
            return value?.ToString();
        }

        /// <summary>
        ///     Writes the value to the key in the registry.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="value">Setting value</param>
        /// <returns>Value written to registry key</returns>
        public object WriteSetting(string name, object value) {
            RegistryKey registry = GetRegistryKey();
            LogInfo("Writing setting '" + name + "': " + value);
            registry.SetValue(name, value);
            return value;
        }

        /// <summary>
        ///     Read given setting and parse as a string.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a string</returns>
        public string ParseSetting(string name, string defaultValue) => ReadSetting(name, defaultValue);

        /// <summary>
        ///     Read given setting and parse as an integer.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as an integer</returns>
        public int ParseSetting(string name, int defaultValue) => int.Parse(ReadSetting(name, defaultValue));

        /// <summary>
        ///     Read given setting and parse as a boolean.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        /// <param name="defaultValue">Setting default value</param>
        /// <returns>Setting value as a boolean</returns>
        public bool ParseSetting(string name, bool defaultValue) => bool.Parse(ReadSetting(name, defaultValue));

        /// <summary>
        ///     Delete registry key.
        /// </summary>
        /// <param name="name">Setting registry key name</param>
        public void DeleteSetting(string name) {
            RegistryKey registry = GetRegistryKey();
            if (registry.GetValue(name) != null) {
                registry.DeleteValue(name);
            }
        }

        /// <summary>
        ///     Deletes regsitry settings subkey.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void ResetSettings() => Registry.CurrentUser.DeleteSubKey(_registry);
    }
}
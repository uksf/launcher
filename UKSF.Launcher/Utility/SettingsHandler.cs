using Microsoft.Win32;

namespace UKSF.Launcher.Utility {
    public class SettingsHandler {
        private readonly string _registry;

        public SettingsHandler(string registryKey) => _registry = registryKey;

        private RegistryKey GetRegistryKey() => Registry.CurrentUser.OpenSubKey(_registry, true) ?? Registry.CurrentUser.CreateSubKey(_registry, true);

        public string ReadSetting(string name, object defaultValue = null) {
            RegistryKey registry = GetRegistryKey();
            object value = registry.GetValue(name);
            if (value == null && defaultValue != null) {
                value = defaultValue;
                WriteSetting(name, value);
            }

            LogHandler.LogInfo("Reading setting '" + name + "': " + value);
            return value?.ToString();
        }

        public object WriteSetting(string name, object value) {
            RegistryKey registry = GetRegistryKey();
            LogHandler.LogInfo("Writing setting '" + name + "': " + value);
            registry.SetValue(name, value);
            return value;
        }

        public string ParseSetting(string name, string defaultValue) => ReadSetting(name, defaultValue);

        public int ParseSetting(string name, int defaultValue) => int.Parse(ReadSetting(name, defaultValue));

        public bool ParseSetting(string name, bool defaultValue) => bool.Parse(ReadSetting(name, defaultValue));

        public void DeleteSetting(string name) {
            RegistryKey registry = GetRegistryKey();
            if (registry.GetValue(name) != null) {
                registry.DeleteValue(name);
            }
        }

        public void ResetSettings() => Registry.CurrentUser.DeleteSubKey(_registry);
    }
}

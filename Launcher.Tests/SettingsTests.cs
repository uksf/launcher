using Microsoft.Win32;
using NUnit.Framework;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Tests {
    internal class SettingsTests {

        private const string REGISTRY = @"SOFTWARE\Launcher.Tests";

        [Test]
        public void SettingsTestsWrite() {
            string value = (string) new SettingsHandler(REGISTRY).WriteSetting("WRITE", "WRITE");

            Assert.AreEqual(value, "WRITE");
        }

        [Test]
        public void SettingsTestsReadString() {
            string value = new SettingsHandler(REGISTRY).ParseSetting("READSTRING", "READSTRING");

            Assert.AreEqual(value, "READSTRING");
        }

        [Test]
        public void SettingsTestsReadInt() {
            int value = new SettingsHandler(REGISTRY).ParseSetting("READINT", 10);

            Assert.AreEqual(value, 10);
        }

        [Test]
        public void SettingsTestsReadBool() {
            bool value = new SettingsHandler(REGISTRY).ParseSetting("READBOOL", true);

            Assert.AreEqual(value, true);
        }

        [Test]
        public void SettingsTestsReadNull() {
            SettingsHandler settingsHandler = new SettingsHandler(REGISTRY);
            settingsHandler.DeleteSetting("READNULL");
            string value = settingsHandler.ParseSetting("READNULL", "READNULL");

            Assert.AreEqual(value, "READNULL");
        }

        [Test]
        public void SettingsTestsDelete() {
            SettingsHandler settingsHandler = new SettingsHandler(REGISTRY);
            settingsHandler.WriteSetting("DELETE", "DELETE");
            settingsHandler.DeleteSetting("DELETE");
            string value = settingsHandler.ReadSetting("DELETE");

            Assert.IsNull(value);
        }

        [Test]
        public void SettingsTestsReset() {
            SettingsHandler settingsHandler = new SettingsHandler(REGISTRY);
            settingsHandler.ResetSettings();

            Assert.IsNull(Registry.CurrentUser.OpenSubKey(REGISTRY));
        }
    }
}

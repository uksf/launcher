using Microsoft.Win32;
using NUnit.Framework;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Tests {
    internal class SettingsTests {
        [Test]
        public void SettingsWrite() {
            string value = (string) new SettingsHandler(@"SOFTWARE\UKSF-Launcher.Tests").WriteSetting("WRITE", "WRITE");

            Assert.AreEqual(value, "WRITE");
        }

        [Test]
        public void SettingsReadString() {
            string value = new SettingsHandler(@"SOFTWARE\UKSF-Launcher.Tests").ParseSetting("READSTRING", "READSTRING");

            Assert.AreEqual(value, "READSTRING");
        }

        [Test]
        public void SettingsReadInt() {
            int value = new SettingsHandler(@"SOFTWARE\UKSF-Launcher.Tests").ParseSetting("READINT", 10);

            Assert.AreEqual(value, 10);
        }

        [Test]
        public void SettingsReadBool() {
            bool value = new SettingsHandler(@"SOFTWARE\UKSF-Launcher.Tests").ParseSetting("READBOOL", true);

            Assert.AreEqual(value, true);
        }
        
        [Test]
        public void SettingsDelete() {
            SettingsHandler settingsHandler = new SettingsHandler(@"SOFTWARE\UKSF-Launcher.Tests");
            settingsHandler.WriteSetting("DELETE", "DELETE");
            settingsHandler.DeleteSetting("DELETE");
            string value = settingsHandler.ReadSetting("DELETE");

            Assert.IsNull(value);
        }
    }
}
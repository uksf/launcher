using Microsoft.Win32;
using NUnit.Framework;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Tests {
    internal class SettingsTests {
        [Test]
        public void SettingsWrite() {
            string value = (string) SettingsHandler.WriteSetting("WRITE", "WRITE");

            Assert.AreEqual(value, "WRITE");
            SettingsHandler.DeleteSetting("WRITE");
        }

        [Test]
        public void SettingsReadString() {
            string value = SettingsHandler.ParseSetting("READSTRING", "READSTRING");

            Assert.AreEqual(value, "READSTRING");
            SettingsHandler.DeleteSetting("READSTRING");
        }

        [Test]
        public void SettingsReadInt() {
            int value = SettingsHandler.ParseSetting("READINT", 10);

            Assert.AreEqual(value, 10);
            SettingsHandler.DeleteSetting("READINT");
        }

        [Test]
        public void SettingsReadBool() {
            bool value = SettingsHandler.ParseSetting("READBOOL", true);

            Assert.AreEqual(value, true);
            SettingsHandler.DeleteSetting("READBOOL");
        }
        
        [Test]
        public void SettingsDelete() {
            SettingsHandler.WriteSetting("DELETE", "DELETE");
            SettingsHandler.DeleteSetting("DELETE");
            
            string value = (string)(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\UKSF-Launcher.Tests", true) ?? Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UKSF-Launcher.Tests", true)).GetValue("DELETE");

            Assert.IsNull(value);
        }
    }
}
using Microsoft.Win32;
using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class GameTests {
        [Test]
        public void GetGameInstallation() {
            string installation = GameHandler.GetGameInstallation();

            if (Registry.LocalMachine.OpenSubKey(Global.GAME_REGISTRY) == null) {
                Assert.IsEmpty(installation);
            } else {
                Assert.That(installation.Contains("Arma 3"));
            }
        }

        [Test]
        public void StartupString() {
            Global.PROFILE = "TESTPROFILE";
            Global.STARTUP_NOSPLASH = true;
            Global.STARTUP_EMPTYWORLD = true;
            Global.STARTUP_SCRIPTERRORS = true;
            Global.STARTUP_FILEPATCHING = true;
            Global.STARTUP_HUGEPAGES = true;
            Global.STARTUP_MALLOC = Global.MALLOC_SYSTEM_DEFAULT;
            string startupString = GameHandler.GetStartupParameters();

            Assert.AreEqual(startupString, "-name=TESTPROFILE -nosplash -world=empty -showScriptErrors -filePatching -hugepages -malloc=system");
        }
    }
}
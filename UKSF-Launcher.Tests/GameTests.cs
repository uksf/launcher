using Microsoft.Win32;
using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class GameTests {
        [Test]
        public void GameTestsGetGameInstallation() {
            string installation = GameHandler.GetGameInstallation();

            if (Registry.LocalMachine.OpenSubKey(Global.GAME_REGISTRY) == null) {
                Assert.IsEmpty(installation);
            } else {
                Assert.That(installation.Contains("Arma 3"));
            }
        }

        [Test]
        public void GameTestsStartupString() {
            Global.PROFILE = "TESTPROFILE";
            Global.STARTUP_NOSPLASH = true;
            Global.STARTUP_SCRIPTERRORS = true;
            Global.STARTUP_FILEPATCHING = true;
            Global.STARTUP_HUGEPAGES = true;
            Global.STARTUP_MALLOC = Global.MALLOC_SYSTEM_DEFAULT;
            Global.SERVER = new ServerHandler.Server {Active = false, Ip = "test.com", Name = "Primary Server", Password = "l85", Port = 2303};
            string startupString = GameHandler.GetStartupParameters();

            Assert.AreEqual(startupString, "-name=TESTPROFILE -nosplash -showScriptErrors -filePatching -hugepages -malloc=system -connect=test.com -port=2303 -password=l85");
        }
    }
}
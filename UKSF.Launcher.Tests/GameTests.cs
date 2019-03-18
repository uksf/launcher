using Microsoft.Win32;
using NUnit.Framework;
using UKSF.Launcher.Game;
using UKSF.Launcher.Network;
using UKSF.Launcher.Patching;

namespace UKSF.Launcher.Tests {
    public class GameTests {
        [Test]
        public void GameTestsGetGameInstallation() {
            string installation = GameHandler.GetGameInstallation();

            if (Registry.LocalMachine.OpenSubKey(Global.Constants.GAME_REGISTRY) == null) {
                Assert.IsEmpty(installation);
            } else {
                Assert.That(installation.Contains("Arma 3"));
            }
        }

        [Test]
        public void GameTestsStartupString() {
            Global.Settings.Profile = "TESTPROFILE";
            Global.Settings.StartupNosplash = true;
            Global.Settings.StartupScripterrors = true;
            Global.Settings.StartupFilepatching = true;
            Global.Settings.StartupHugepages = true;
            Global.Settings.StartupMalloc = Global.Constants.MALLOC_SYSTEM_DEFAULT;
            Global.Settings.Server = new Server("Primary Server", "test.com", 2303, "l85", true);
            Global.Repo = new RepoClient(Global.Settings.ModLocation, Global.Constants.APPDATA, "uksf", null, null);
            string startupString = GameHandler.GetStartupParameters();

            Assert.That(startupString.Contains("-name=TESTPROFILE -nosplash -showScriptErrors -filePatching -hugepages -malloc=system -connect=test.com -port=2302 -password=l85"));
        }
    }
}

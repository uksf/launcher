using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class Tests {
        [Test]
        public void GameExe() {
            string exe = GameHandler.GetGameInstallation();

            Assert.AreEqual(exe, @"B:\Steam\steamapps\common\Arma 3\arma3_x64.exe");
        }

        [Test]
        public void ProfileCreation() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile("SqnLdr", "Beswick", "T");
            
            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, "");
        }
    }
}
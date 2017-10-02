using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class Tests {
        [Test]
        public void ProfileCreation() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile("SqnLdr", "Beswick", "T");
            
            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, "");
        }
    }
}
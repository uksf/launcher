using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class ProfileTests {
        [Test]
        public void ProfilesCreationFromParts() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile("SqnLdr", "Beswick", "T");

            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, "");
        }

        [Test]
        public void ProfilesCreationFromFile() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile(@"C:\this\is\a\test\SqnLdr%2eBeswick%2eT.arma3profile");

            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, @"C:\this\is\a\test\SqnLdr%2eBeswick%2eT.arma3profile");
        }

        [Test]
        public void ProfilesFindUksfProfile() {
            List<ProfileHandler.Profile> profiles = new List<ProfileHandler.Profile> {
                new ProfileHandler.Profile("SqnLdr", "Beswick", "T"),
                new ProfileHandler.Profile("Beswick.arma3profile"),
                new ProfileHandler.Profile("Cdt", "Jones", "A")
            };

            Assert.AreSame(ProfileHandler.FindUksfProfile(profiles), profiles.ElementAt(0));
        }
    }
}
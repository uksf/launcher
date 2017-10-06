﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.Tests {
    public class ProfileTests {
        [Test]
        public void ProfileTestsProfileCreationFromParts() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile("SqnLdr", "Beswick", "T");

            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, "");
        }

        [Test]
        public void ProfileTestsProfileCreationFromFile() {
            ProfileHandler.Profile profile = new ProfileHandler.Profile(@"C:\this\is\a\test\SqnLdr%2eBeswick%2eT.arma3profile");

            Assert.AreEqual(profile.Name, "SqnLdr%2eBeswick%2eT");
            Assert.AreEqual(profile.DisplayName, "SqnLdr.Beswick.T");
            Assert.AreEqual(profile.FilePath, @"C:\this\is\a\test\SqnLdr%2eBeswick%2eT.arma3profile");
        }

        [Test]
        public void ProfileTestsFindUksfProfile() {
            List<ProfileHandler.Profile> profiles = new List<ProfileHandler.Profile> {
                new ProfileHandler.Profile("SqnLdr", "Beswick", "T"),
                new ProfileHandler.Profile("Beswick.arma3profile"),
                new ProfileHandler.Profile("Cdt", "Jones", "A")
            };
            ProfileHandler.Profile profile = ProfileHandler.FindUksfProfile(profiles);

            Assert.AreSame(profile, profiles.ElementAt(0));
        }

        [Test]
        public void ProfileTestsGetProfiles() {
            List<ProfileHandler.Profile> profiles = null;
            if (Directory.Exists(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles")) {
                profiles = ProfileHandler.GetProfiles(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles");
                
                Assert.That(profiles.Any(profile => profile.DisplayName.Equals("Tim")) && profiles.Any(profile => profile.DisplayName.Equals("Maj.Dick.H")) &&
                            profiles.Any(profile => profile.DisplayName.Equals("SqnLdr.Beswick.T")));
            } else if (Directory.Exists(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles")) {
                profiles = ProfileHandler.GetProfiles(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles");
                
                Assert.That(profiles.Any(profile => profile.DisplayName.Equals("Tim")) && profiles.Any(profile => profile.DisplayName.Equals("Maj.Dick.H")) &&
                            profiles.Any(profile => profile.DisplayName.Equals("SqnLdr.Beswick.T")));
            } else {
                Assert.IsNull(profiles);
            }
        }

        [Test]
        public void ProfileTestsCopyProfile() {
            if (Directory.Exists(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles")) {
                ProfileHandler.Profile profile1 = new ProfileHandler.Profile(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles\Tim.Arma3Profile");
                ProfileHandler.Profile profile2 = new ProfileHandler.Profile("Cdt", "Twat", "C");
                ProfileHandler.CopyProfile(profile1, profile2, @"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles");

                Assert.True(File.Exists(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles\Cdt%2eTwat%2eC\Cdt%2eTwat%2eC.Arma3Profile"));
                Directory.Delete(@"E:\Workspace\UKSF-Launcher\UKSF-Launcher\UKSF-Launcher.Tests\Profiles\Cdt%2eTwat%2eC", true);
            } else if (Directory.Exists(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles")) {
                ProfileHandler.Profile profile1 = new ProfileHandler.Profile(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles\Tim.Arma3Profile");
                ProfileHandler.Profile profile2 = new ProfileHandler.Profile("Cdt", "Twat", "C");
                ProfileHandler.CopyProfile(profile1, profile2, @"C:\projects\launcher\UKSF-Launcher.Tests\Profiles");

                Assert.True(File.Exists(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles\Cdt%2eTwat%2eC\Cdt%2eTwat%2eC.Arma3Profile"));
                Directory.Delete(@"C:\projects\launcher\UKSF-Launcher.Tests\Profiles\Cdt%2eTwat%2eC", true);
            }
        }
    }
}
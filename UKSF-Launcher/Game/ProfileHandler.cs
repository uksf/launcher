
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Game {
    public class ProfileHandler {

        private const string LOCATION_DEFAULT = "Arma 3";
        private const string LOCATION_OTHER = "Arma 3 - Other Profiles";
        private const string PROFILE_EXTENSION = ".arma3profile";

        private static readonly string[] PROFILE_PREFIXES = {
            "maj", "sqnldr", "capt", "fltlt", "lt", "fgoff", "2lt", "pltoff", "ocdt", "offcdt", "sgt", "fs", "cpl", "sac", "lcpl", "lac", "pte", "ac", "rct", "cdt"
        };

        public static List<Profile> GetProfiles() {
            LogHandler.LogHashSpaceMessage(Severity.INFO, "Updating profiles");
            List<Profile>  profiles = new List<Profile>();
            string defaultLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_DEFAULT);
            string otherLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_OTHER);

            List<string> files = new List<string>();
            if (Directory.Exists(defaultLocation)) {
                files = GetFiles(defaultLocation);
            }
            if (Directory.Exists(otherLocation)) {
                var folders = Directory.GetDirectories(otherLocation);
                foreach (var folder in folders) {
                    files.AddRange(GetFiles(folder));
                }
            }

            foreach (string file in files) {
                profiles.Add(new Profile(file));
            }
            return profiles;
        }

        private static List<string> GetFiles(string directory) {
            return Directory.EnumerateFiles(directory).Where(file => file.ToLower().EndsWith(PROFILE_EXTENSION) && file.Count(count => count == '.') == 1).ToList();
        }

        public static Profile FindUKSFProfile(List<Profile> profiles) {
            foreach (Profile profile in profiles) {
                string[] parts = profile.DisplayName.Split('.');
                if (parts.Length == 3) {
                    if (PROFILE_PREFIXES.Contains(parts.ElementAt(0).ToLower())) {
                        return profile;
                    }
                }
            }
            return null;
        }

        public class Profile {
            private string name, displayName;

            public Profile(string fileName) {
                Name = Path.GetFileNameWithoutExtension(fileName);
                DisplayName = GetDisplayName(Name);
                LogHandler.LogInfo("Found profile: " + Name + " / " + DisplayName + "");
            }

            public string Name { get => name; set => name = value; }
            public string DisplayName { get => displayName; set => displayName = value; }

            private string GetDisplayName(string name) => Regex.Replace(name, "%2e", ".");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.Game {
    public static class ProfileHandler {
        private const string PROFILE_EXTENSION = "*.Arma3Profile";

        public static readonly string[] PROFILE_PREFIXES = {
            "Maj", "SqnLdr", "Capt", "FltLt", "Lt", "FgOff", "2Lt", "PltOff", "OCdt", "OffCdt", "Sgt", "FS", "Cpl", "SAC", "LCpl", "LAC", "Pte", "AC", "Rct",
            "Cdt" // TODO: Get ranks from api
        };

        public static List<Profile> GetProfilesAll() {
            List<Profile> profiles = GetProfiles(Global.Constants.PROFILE_LOCATION_DEFAULT);
            profiles.AddRange(GetProfiles(Global.Constants.PROFILE_LOCATION_OTHER));
            return profiles;
        }

        public static List<Profile> GetProfiles(string directory) {
            LogHandler.LogSeverity(Global.Severity.INFO, $"Searching {directory} for profiles");
            List<string> files = new List<string>();
            if (Directory.Exists(directory)) {
                files = Directory.EnumerateFiles(directory, PROFILE_EXTENSION, SearchOption.AllDirectories)
                                 .Where(file => (Path.GetFileName(file) ?? throw new InvalidOperationException()).Count(count => count == '.') == 1)
                                 .ToList();
            }

            return files.Select(file => new Profile(file)).ToList();
        }

        public static Profile FindUksfProfile(List<Profile> profiles) {
            Profile selectedProfile = null;
            foreach (string prefix in PROFILE_PREFIXES) {
                selectedProfile = (from profile in profiles
                                   let parts = profile.DisplayName.Split('.')
                                   where parts.Length == 3
                                   where parts.ElementAt(0).ToLower().Contains(prefix.ToLower())
                                   select profile).FirstOrDefault();
                if (selectedProfile != null) break;
            }

            return selectedProfile;
        }

        public static void CopyProfile(Profile profile, Profile newProfile, string newDirectory) {
            if (!File.Exists(profile.FilePath)) return;
            string directory = Path.GetDirectoryName(profile.FilePath);
            if (!Directory.Exists(directory)) return;
            List<string> files = Directory.EnumerateFiles(directory, PROFILE_EXTENSION).ToList();
            Directory.CreateDirectory(Path.Combine(newDirectory, newProfile.Name));
            foreach (string file in files) {
                string fileName = Path.GetFileName(file);
                if (fileName != null) {
                    File.Copy(file, Path.Combine(newDirectory, newProfile.Name, fileName.Replace(fileName.Split('.')[0], newProfile.Name)));
                }
            }
        }

        public static void UpdateProfileSquad(string profileName) {
            Profile profile = GetProfilesAll().FirstOrDefault(profileCheck => profileCheck.DisplayName.Equals(profileName));
            if (profile == null) return;
            if (!File.Exists(profile.FilePath)) return;
            if (!PROFILE_PREFIXES.Any(prefix => profile.DisplayName.Contains(prefix))) {
                LogHandler.LogSeverity(Global.Severity.WARNING, $"Profile '{profile.DisplayName}' is not a UKSF profile, will not force Squad info.");
                return;
            }

            string[] lines = File.ReadAllLines(profile.FilePath);
            for (int index = 0; index < lines.Length; index++) {
                if (lines[index].Contains("glasses=")) {
                    lines[index] = @"	glasses=""None"";";
                } else if (lines[index].Contains("unitType=")) {
                    lines[index] = @"	unitType=1;";
                } else if (lines[index].Contains("unitId=")) {
                    lines[index] = @"	unitId=-1;";
                } else if (lines[index].Contains("squad=")) {
                    lines[index] = @"	squad=""http://arma.uk-sf.com/squadtag/A3/squad.xml"";";
                }
            }

            File.WriteAllLines(profile.FilePath, lines);
            LogHandler.LogInfo($"Squad info forced for profile '{profile.DisplayName}'");
        }

        public class Profile {
            private const string PROFILE_JOINER = "%2e";
            public readonly string DisplayName;
            public readonly string FilePath;
            public readonly string Name;

            public Profile(string fileName) {
                FilePath = fileName;
                Name = Path.GetFileNameWithoutExtension(fileName);
                DisplayName = Regex.Replace(Name ?? throw new InvalidOperationException(), PROFILE_JOINER, ".");
                LogHandler.LogInfo($"Found profile: {Name} / {DisplayName}");
            }

            public Profile(string rank, string surname, string initial) {
                FilePath = "";
                Name = rank + PROFILE_JOINER + surname + PROFILE_JOINER + initial.ToUpper();
                DisplayName = Regex.Replace(Name, PROFILE_JOINER, ".");
                LogHandler.LogInfo($"Found profile: {Name} / {DisplayName}");
            }
        }
    }
}

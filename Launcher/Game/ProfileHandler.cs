﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.Game {
    public static class ProfileHandler {
        // File extension of profile files
        private const string PROFILE_EXTENSION = "*.Arma3Profile";

        // Ranks for prefix of profile
        public static readonly string[] PROFILE_PREFIXES =
            {"Maj", "SqnLdr", "Capt", "FltLt", "Lt", "FgOff", "2Lt", "PltOff", "OCdt", "OffCdt", "Sgt", "FS", "Cpl", "SAC", "LCpl", "LAC", "Pte", "AC", "Rct", "Cdt"};

        /// <summary>
        ///     Gets profiles from both default and other locations.
        /// </summary>
        /// <returns>List of Profile objects for all profiles found</returns>
        public static List<Profile> GetProfilesAll() {
            List<Profile> profiles = GetProfiles(PROFILE_LOCATION_DEFAULT);
            profiles.AddRange(GetProfiles(PROFILE_LOCATION_OTHER));
            return profiles;
        }

        /// <summary>
        ///     Checks the default and other profiles locations for profile files. Creates Profile objects for each profile found.
        /// </summary>
        /// <returns>List of Profile objects for all profiles found</returns>
        public static List<Profile> GetProfiles(string directory) {
            LogSeverity(Severity.INFO, $"Searching {directory} for profiles");
            List<string> files = new List<string>();
            if (Directory.Exists(directory)) {
                files = Directory.EnumerateFiles(directory, PROFILE_EXTENSION, SearchOption.AllDirectories)
                                 .Where(file => (Path.GetFileName(file) ?? throw new InvalidOperationException()).Count(count => count == '.') == 1).ToList();
            }
            return files.Select(file => new Profile(file)).ToList();
        }

        /// <summary>
        ///     Checks if any of the profiles in the list of Profile objects contains a rank prefix and is formatted correctly
        ///     (SqnLdr.Beswick.T).
        /// </summary>
        /// <param name="profiles">List of Profile objects to check</param>
        /// <returns>Profile with a rank or no Profile if no rank</returns>
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

        /// <summary>
        ///     Prompts user to select a rank, enter a surname and first initial. Then creates a new Profile and directory in other
        ///     profiles location. Files of the given Profile are copied and renamed to copy user settings.
        /// </summary>
        /// <param name="profile">Profile to copy settings from</param>
        /// <param name="newProfile">New profile to copy settings to</param>
        /// <param name="newDirectory">Directory to create new profile in</param>
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

        /// <summary>
        ///     Updates profile file with correct squad URL settings.
        /// </summary>
        /// <param name="profileName">Profile to update</param>
        public static void UpdateProfileSquad(string profileName) {
            Profile profile = GetProfilesAll().FirstOrDefault(profileCheck => profileCheck.DisplayName.Equals(profileName));
            if (profile == null) return;
            if (!File.Exists(profile.FilePath)) return;
            if (!PROFILE_PREFIXES.Any(prefix => profile.DisplayName.Contains(prefix))) {
                LogSeverity(Severity.WARNING, $"Profile '{profile.DisplayName}' is not a UKSF profile, will not force Squad info.");
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
                    lines[index] = @"	squad=""http://uk-sf.com/squadtag/A3/squad.xml"";";
                }
            }
            File.WriteAllLines(profile.FilePath, lines);
            LogInfo($"Squad info forced for profile '{profile.DisplayName}'");
        }

        public class Profile {
            // Delimiter for joining elements of profile string. Replaces '.' characters
            private const string PROFILE_JOINER = "%2e";

            /// <summary>
            ///     Create new profile from given file name.
            /// </summary>
            /// <param name="fileName">File name it create Profile from</param>
            public Profile(string fileName) {
                FilePath = fileName;
                Name = Path.GetFileNameWithoutExtension(fileName);
                DisplayName = Regex.Replace(Name ?? throw new InvalidOperationException(), PROFILE_JOINER, ".");
                LogInfo($"Found profile: {Name} / {DisplayName}");
            }

            /// <summary>
            ///     Create new profile from given parts.
            /// </summary>
            /// <param name="rank">Rank prefix</param>
            /// <param name="surname">Surname of user</param>
            /// <param name="initial">Initial of first name of user</param>
            public Profile(string rank, string surname, string initial) {
                FilePath = "";
                Name = rank + PROFILE_JOINER + surname + PROFILE_JOINER + initial.ToUpper();
                DisplayName = Regex.Replace(Name, PROFILE_JOINER, ".");
                LogInfo($"Found profile: {Name} / {DisplayName}");
            }

            public string FilePath { get; }
            public string Name { get; }
            public string DisplayName { get; }
        }
    }
}
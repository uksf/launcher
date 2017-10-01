using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using UKSF_Launcher.UI.Dialog;
using UKSF_Launcher.UI.FTS;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    public static class ProfileHandler {
        // Folder name of default profile location
        private const string LOCATION_DEFAULT = "Arma 3";

        // Folder name of other profiles location
        private const string LOCATION_OTHER = "Arma 3 - Other Profiles";

        // File extension of profile files
        private const string PROFILE_EXTENSION = ".arma3profile";

        // Ranks for prefix of profile
        public static readonly string[] PROFILE_PREFIXES =
            {"Maj", "SqnLdr", "Capt", "FltLt", "Lt", "FgOff", "2Lt", "PltOff", "OCdt", "OffCdt", "Sgt", "FS", "Cpl", "SAC", "LCpl", "LAC", "Pte", "AC", "Rct", "Cdt"};

        /// <summary>
        ///     Checks the default and other profiles locations for profile files. Creates Profile objects for each profile found.
        /// </summary>
        /// <returns>List of Profile objects for all profiles found</returns>
        public static List<Profile> GetProfiles() {
            string defaultLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_DEFAULT);
            string otherLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_OTHER);

            List<string> files = new List<string>();
            if (Directory.Exists(defaultLocation)) {
                files = GetFiles(defaultLocation);
            }
            if (!Directory.Exists(otherLocation)) return files.Select(file => new Profile(file)).ToList();
            string[] folders = Directory.GetDirectories(otherLocation);
            foreach (string folder in folders) {
                files.AddRange(GetFiles(folder));
            }

            return files.Select(file => new Profile(file)).ToList();
        }

        /// <summary>
        ///     Gets files from given directory with the profile extension and excludes files with chained extensions.
        /// </summary>
        /// <param name="directory">Directory path to search for profile files in</param>
        /// <returns>List of file paths for each profile</returns>
        private static List<string> GetFiles(string directory) => Directory.EnumerateFiles(directory)
                                                                           .Where(file => file.ToLower().EndsWith(PROFILE_EXTENSION) &&
                                                                                          file.Count(count => count == '.') == 1).ToList();

        /// <summary>
        ///     Checks if any of the profiles in the list of Profile objects contains a rank prefix and is formatted correctly
        ///     (SqnLdr.Beswick.T).
        /// </summary>
        /// <param name="profiles">List of Profile objects to check</param>
        /// <returns>Profile with a rank or no Profile if no rank</returns>
        public static Profile FindUksfProfile(IEnumerable<Profile> profiles) =>
            (from profile in profiles
             let parts = profile.DisplayName.Split('.')
             where parts.Length == 3
             where PROFILE_PREFIXES.Contains(parts.ElementAt(0), StringComparer.OrdinalIgnoreCase)
             select profile).FirstOrDefault();

        /// <summary>
        ///     Prompts user to select a rank, enter a surname and first initial. Then creates a new Profile and directory in other
        ///     profiles location. Files of the given Profile are copied and renamed to copy user settings.
        /// </summary>
        /// <param name="profile">Profile to copy settings from</param>
        public static void CopyProfile(Profile profile) {
            MessageBoxResult result =
                DialogWindow.Show("New Profile",
                                  "Select your rank, enter your last name, and the initial of your first name.\n\nIf you are a new member, your rank will be 'Cdt'.",
                                  DialogWindow.DialogBoxType.OK_CANCEL, new FtsProfileSelectionControl());
            if (result != MessageBoxResult.OK) return;
            Profile newProfile = new Profile(FtsProfileSelectionControl.Rank, FtsProfileSelectionControl.Surname, FtsProfileSelectionControl.Initial);
            if (!File.Exists(profile.FilePath)) return;
            string directory = Path.GetDirectoryName(profile.FilePath);
            if (!Directory.Exists(directory)) return;
            List<string> files = Directory.EnumerateFiles(directory).Where(file => file.ToLower().EndsWith(PROFILE_EXTENSION)).ToList();
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_OTHER, newProfile.Name));
            foreach (string file in files) {
                string fileName = Path.GetFileName(file);
                if (fileName != null) {
                    File.Copy(file,
                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LOCATION_OTHER, newProfile.Name,
                                           fileName.Replace(fileName.Split('.')[0], newProfile.Name)));
                }
            }
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
                LogHandler.LogInfo("Found profile: " + Name + " / " + DisplayName + "");
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
                LogHandler.LogInfo("Found profile: " + Name + " / " + DisplayName + "");
            }

            public string FilePath { get; }
            public string Name { get; }
            public string DisplayName { get; }
        }
    }
}
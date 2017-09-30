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
        private const string LOCATION_DEFAULT = "Arma 3";
        private const string LOCATION_OTHER = "Arma 3 - Other Profiles";
        private const string PROFILE_EXTENSION = ".arma3profile";

        public static readonly string[] PROFILE_PREFIXES =
            {"Maj", "SqnLdr", "Capt", "FltLt", "Lt", "FgOff", "2Lt", "PltOff", "OCdt", "OffCdt", "Sgt", "FS", "Cpl", "SAC", "LCpl", "LAC", "Pte", "AC", "Rct", "Cdt"};

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

        private static List<string> GetFiles(string directory) => Directory.EnumerateFiles(directory)
                                                                           .Where(file => file.ToLower().EndsWith(PROFILE_EXTENSION) &&
                                                                                          file.Count(count => count == '.') == 1).ToList();

        public static Profile FindUksfProfile(IEnumerable<Profile> profiles) =>
            (from profile in profiles
             let parts = profile.DisplayName.Split('.')
             where parts.Length == 3
             where PROFILE_PREFIXES.Contains(parts.ElementAt(0).ToLower(), StringComparer.OrdinalIgnoreCase)
             select profile).FirstOrDefault();

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
            private const string PROFILE_JOINER = "%2e";

            public Profile(string fileName) {
                FilePath = fileName;
                Name = Path.GetFileNameWithoutExtension(fileName);
                DisplayName = GetDisplayName(Name);
                LogHandler.LogInfo("Found profile: " + Name + " / " + DisplayName + "");
            }

            public Profile(string rank, string surname, string initial) {
                FilePath = "";
                Name = rank + PROFILE_JOINER + surname + PROFILE_JOINER + initial.ToUpper();
                DisplayName = GetDisplayName(Name);
                LogHandler.LogInfo("Found profile: " + Name + " / " + DisplayName + "");
            }

            public string FilePath { get; }
            public string Name { get; }
            public string DisplayName { get; }

            private static string GetDisplayName(string name) => Regex.Replace(name, PROFILE_JOINER, ".");
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Settings_LauncherControl.xaml
    /// </summary>
    public partial class Settings_LauncherControl : UserControl {

        private List<CustomComboBoxItem> items;

        public Settings_LauncherControl() {
            InitializeComponent();

            Settings_LauncherControl_Version.Content = "Version: " + VERSION.ToString();
            Settings_LauncherControl_Autoupdate.IsChecked = AUTOUPDATELAUNCHER;

            items = new List<CustomComboBoxItem>();
            AddProfiles();
        }

        private void Settings_LauncherControl_CheckBoxAUTOUPDATE_Click(object sender, RoutedEventArgs e) {
            AUTOUPDATELAUNCHER = (bool)SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", Settings_LauncherControl_Autoupdate.IsChecked);
        }

        private void Settings_LauncherControl_ProfileSelected(object sender, RoutedEventArgs e) {
            string profile = items.ElementAt(Settings_LauncherControl_Profile.SelectedIndex).ItemProfile.Name;
            if (PROFILE != profile) {
                PROFILE = (string)SettingsHandler.WriteSetting("PROFILE", profile);
            }
        }

        private void AddProfiles() {
            Settings_LauncherControl_Profile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfiles();
            foreach (ProfileHandler.Profile profile in profiles) {
                CustomComboBoxItem item = new CustomComboBoxItem(profile, FindResource("UKSF.ComboBoxItem") as Style);
                items.Add(item);
                Settings_LauncherControl_Profile.Items.Add(item);

                if (profile.Name == PROFILE) {
                    Settings_LauncherControl_Profile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            if (PROFILE == "") {
                ProfileHandler.Profile profile = ProfileHandler.FindUKSFProfile(profiles);
                if (profile != null) {
                    PROFILE = profile.Name;
                    Settings_LauncherControl_Profile.SelectedIndex = profiles.IndexOf(profile);
                } else {

                }
            }
        }

        public class CustomComboBoxItem : ComboBoxItem {

            private ProfileHandler.Profile itemProfile;

            public CustomComboBoxItem(ProfileHandler.Profile _itemProfile, Style style) {
                ItemProfile = _itemProfile;
                Content = ItemProfile.DisplayName;
                Style = style;
            }

            public ProfileHandler.Profile ItemProfile { get => itemProfile; set => itemProfile = value; }
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for LauncherSettingsControl.xaml
    /// </summary>
    public partial class LauncherSettingsControl : UserControl {

        private List<CustomComboBoxItem> items;

        public LauncherSettingsControl() {
            InitializeComponent();

            LauncherSettingsAutoupdate.IsChecked = AUTOUPDATE;

            items = new List<CustomComboBoxItem>();
            AddProfiles();
        }

        private void CheckBoxAUTOUPDATE_Click(object sender, RoutedEventArgs e) {
            AUTOUPDATE = (bool)SettingsHandler.WriteSetting("AUTOUPDATE", LauncherSettingsAutoupdate.IsChecked);
        }

        private void LauncherSettingsProfile_Selected(object sender, RoutedEventArgs e) {
            PROFILE = (string)SettingsHandler.WriteSetting("PROFILE", items.ElementAt(LauncherSettingsProfile.SelectedIndex).ItemProfile.Name);
        }

        private void AddProfiles() {
            LauncherSettingsProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfiles();
            foreach (ProfileHandler.Profile profile in profiles) {
                CustomComboBoxItem item = new CustomComboBoxItem(profile, FindResource("UKSF.ComboBoxItem") as Style);
                items.Add(item);
                LauncherSettingsProfile.Items.Add(item);

                if (profile.Name == PROFILE) {
                    LauncherSettingsProfile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            if (PROFILE == "") {
                ProfileHandler.Profile profile = ProfileHandler.FindUKSFProfile(profiles);
                if (profile != null) {
                    PROFILE = profile.Name;
                    LauncherSettingsProfile.SelectedIndex = profiles.IndexOf(profile);
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.UI.SettingsLauncherControl;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for Settings_LauncherControl.xaml
    /// </summary>
    public partial class SettingsLauncherControl {
        private readonly List<CustomComboBoxItem> _items;

        public SettingsLauncherControl() {
            InitializeComponent();

            SettingsLauncherControlVersion.Content = "Version: " + Global.VERSION;
            SettingsLauncherControlAutoupdate.IsChecked = Global.AUTOUPDATELAUNCHER;

            SettingsLauncherControlGameExecutable.Text = Global.GAME_LOCATION;
            SettingsLauncherControlDownloadLocation.Text = Global.MOD_LOCATION;

            _items = new List<CustomComboBoxItem>();
            AddProfiles();
        }

        private void SettingsLauncherControlCheckBoxAUTOUPDATE_Click(object sender, RoutedEventArgs e) {
            Global.AUTOUPDATELAUNCHER = (bool) SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", SettingsLauncherControlAutoupdate.IsChecked);
        }

        private void SettingsLauncherControlProfile_Selected(object sender, RoutedEventArgs e) {
            string profile = _items.ElementAt(SettingsLauncherControlProfile.SelectedIndex).ItemProfile.Name;
            if (Global.PROFILE != profile) {
                Global.PROFILE = (string) SettingsHandler.WriteSetting("PROFILE", profile);
            }
        }

        private void AddProfiles() {
            SettingsLauncherControlProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfiles();
            foreach (ProfileHandler.Profile profile in profiles) {
                CustomComboBoxItem item = new CustomComboBoxItem(profile, FindResource("Uksf.ComboBoxItem") as Style);
                _items.Add(item);
                SettingsLauncherControlProfile.Items.Add(item);

                if (profile.Name == Global.PROFILE) {
                    SettingsLauncherControlProfile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            if (Global.PROFILE != "") return;
            if (ProfileHandler.FindUksfProfile(profiles) == null) return;
            Global.PROFILE = ProfileHandler.FindUksfProfile(profiles).Name;
            SettingsLauncherControlProfile.SelectedIndex = profiles.IndexOf(ProfileHandler.FindUksfProfile(profiles));
        }
    }
}
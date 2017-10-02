using System.Collections.Generic;
using System.Linq;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for SettingsLauncherControl.xaml
    /// </summary>
    public partial class SettingsLauncherControl {
        private readonly List<CustomComboBoxItem> _items;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new SettingsLauncherControl object.
        /// </summary>
        public SettingsLauncherControl() {
            InitializeComponent();

            SettingsLauncherControlVersion.Content = "Version: " + Global.VERSION;
            SettingsLauncherControlAutoupdate.IsChecked = Global.AUTOUPDATELAUNCHER;

            SettingsLauncherControlGameExecutable.Text = Global.GAME_LOCATION;
            SettingsLauncherControlDownloadLocation.Text = Global.MOD_LOCATION;

            _items = new List<CustomComboBoxItem>();
            AddProfiles();
        }

        /// <summary>
        ///     Triggered when auto-update launcher checkbox is clicked. Writes state to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void SettingsLauncherControlCheckBoxAUTOUPDATELAUNCHER_Click(object sender, RoutedEventArgs args) {
            Global.AUTOUPDATELAUNCHER = (bool) Core.SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", SettingsLauncherControlAutoupdate.IsChecked);
        }

        /// <summary>
        ///     Triggered when a profile is selected. Writes profile to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void SettingsLauncherControlProfile_Selected(object sender, RoutedEventArgs args) {
            string profile = _items.ElementAt(SettingsLauncherControlProfile.SelectedIndex).ItemProfile.Name;
            if (Global.PROFILE != profile) {
                Global.PROFILE = (string) Core.SettingsHandler.WriteSetting("PROFILE", profile);
            }
        }

        /// <summary>
        ///     Adds profiles to profile dropdown. Sets selected profile to PROFILE value if it is set, otherwise attempts to find
        ///     a UKSF profile.
        /// </summary>
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
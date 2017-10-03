using System.Windows;
using UKSF_Launcher.UI.General;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for SettingsGameControl.xaml
    /// </summary>
    public partial class SettingsGameControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainMainControl object.
        /// </summary>
        public SettingsGameControl() {
            InitializeComponent();

            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsLauncherControlProfile_Update));
        }

        /// <summary>
        ///     Triggered by eventhanlder. Writes profile to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void SettingsLauncherControlProfile_Update(object sender, RoutedEventArgs args) {
            Global.PROFILE =
                Global.PROFILE != ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem).Profile.Name
                    ? (string) Core.SettingsHandler.WriteSetting("PROFILE",
                                                                 ((ProfileComboBoxItem) SettingsGameControlDropdownProfileSelectionControl
                                                                     .ProfileSelectionControlDropdownProfile.SelectedItem).Profile.Name)
                    : Global.PROFILE;
        }
    }
}
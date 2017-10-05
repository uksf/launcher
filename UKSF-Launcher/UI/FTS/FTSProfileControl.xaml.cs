using System.Windows;
using UKSF_Launcher.UI.General;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsProfileControl.xaml
    /// </summary>
    public partial class FtsProfileControl {
        private const string TITLE = "Game Profile";

        private static readonly string DESCRIPTION = "We have selected the Arma 3 profile we think you use for UKSF." + NL +
                                                     "If this is incorrect, select the profile you wish to use from the list below." + NL +
                                                     "Alternatively, select a profile from the list below and press 'Copy' to create a new profile and copy your game settings.";

        private static readonly string DESCRIPTION_NOPROFILE = "We can't find an Arma 3 profile suitable for UKSF." + NL +
                                                               "Select a profile from the list below and press 'Copy' to create a new profile and copy your game settings." +
                                                               NL + "You may skip this step if you do not wish to create a new profile. (Not recommended)";

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsProfileControl object.
        /// </summary>
        public FtsProfileControl() {
            AddHandler(ProfileSelectionControl.PROFILE_SELECTION_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsProfileControlProfile_Update));

            InitializeComponent();
        }

        /// <summary>
        ///     Shows the profile selection control. Adds profiles to the selection.
        /// </summary>
        public void Show() {
            if (Visibility == Visibility.Visible) return;
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = ((ProfileComboBoxItem) FtsProfileControlProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem).Profile != null
                           ? DESCRIPTION
                           : DESCRIPTION_NOPROFILE
            });
            UpdateWarning();
        }

        /// <summary>
        ///     Hides the profile selection control.
        /// </summary>
        public void Hide() => Visibility = Visibility.Collapsed;

        /// <summary>
        ///     Checks if a warning needs to be displayed and raises a warning event.
        /// </summary>
        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (FtsProfileControlProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedIndex == -1) {
                warning = "Please select a profile";
                block = true;
            }
            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        private void FtsProfileControlProfile_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
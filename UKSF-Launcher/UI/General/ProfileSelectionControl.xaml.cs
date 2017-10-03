using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.Dialog;

namespace UKSF_Launcher.UI.General {
    /// <summary>
    ///     Interaction logic for ProfileSelectionControl.xaml
    /// </summary>
    public partial class ProfileSelectionControl {
        public static readonly RoutedEvent PROFILE_SELECTION_CONTROL_UPDATE_EVENT =
            EventManager.RegisterRoutedEvent("PROFILE_SELECTION_CONTROL_UPDATE_EVENT", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(ProfileSelectionControl));

        public ProfileSelectionControl() {
            InitializeComponent();

            if (ProfileSelectionControlDropdownProfile.Items.IsEmpty) {
                AddProfiles();
            }
        }

        /// <summary>
        ///     Adds profiles to profile dropdown. Sets selected profile to PROFILE value if it is set, otherwise attempts to find
        ///     a UKSF profile.
        /// </summary>
        private void AddProfiles() {
            ProfileSelectionControlDropdownProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfilesAll();
            foreach (ProfileHandler.Profile profile in profiles) {
                ProfileComboBoxItem item = new ProfileComboBoxItem(profile, FindResource("Uksf.ComboBoxItem") as Style);
                ProfileSelectionControlDropdownProfile.Items.Add(item);

                if (Global.FIRSTTIMESETUPDONE && profile.Name == Global.PROFILE) {
                    ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            if (ProfileHandler.FindUksfProfile(profiles) != null) {
                if (Global.FIRSTTIMESETUPDONE && Global.PROFILE == "") {
                    Global.PROFILE = ProfileHandler.FindUksfProfile(profiles).Name;
                } else {
                    ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(ProfileHandler.FindUksfProfile(profiles));
                }
            }

            RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
        }

        /// <summary>
        ///     Triggered when profile selection is changed. Updates profile value and warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void ProfileSelectionControlDropdownProfile_Selected(object sender, SelectionChangedEventArgs args) {
            if (ProfileSelectionControlDropdownProfile.SelectedIndex > -1) {
                RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
            }
        }

        /// <summary>
        ///     Triggered when copy button is clicked. Copies selected profile and refreshes profile list in dropdown.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void ProfileSelectionControlButtonCopy_Click(object sender, RoutedEventArgs args) {
            ProfileCreationControl control = new ProfileCreationControl();
            MessageBoxResult result =
                DialogWindow.Show("New Profile", "Select your rank, enter your last name, and the initial of your first name.\n\nIf you are a new member, your rank will be 'Cdt'.",
                                  DialogWindow.DialogBoxType.OK_CANCEL, control);
            if (result != MessageBoxResult.OK) return;
            ProfileHandler.CopyProfile(((ProfileComboBoxItem) ProfileSelectionControlDropdownProfile.SelectedItem).Profile,
                                       new ProfileHandler.Profile(control.Rank, control.Surname, control.Initial));
            AddProfiles();
        }
    }
}
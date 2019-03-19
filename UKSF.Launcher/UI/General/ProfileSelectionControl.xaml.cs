using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UKSF.Launcher.Game;
using UKSF.Launcher.UI.Dialog;

namespace UKSF.Launcher.UI.General {
    public partial class ProfileSelectionControl {
        public static readonly RoutedEvent PROFILE_SELECTION_CONTROL_UPDATE_EVENT =
            EventManager.RegisterRoutedEvent("PROFILE_SELECTION_CONTROL_UPDATE_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ProfileSelectionControl));

        public ProfileSelectionControl() {
            InitializeComponent();

            if (ProfileSelectionControlDropdownProfile.Items.IsEmpty) {
                AddProfiles();
            }
        }

        private void AddProfiles() {
            ProfileSelectionControlDropdownProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfilesAll();
            foreach (ProfileHandler.Profile profile in profiles) {
                ProfileComboBoxItem item = new ProfileComboBoxItem(profile, FindResource("Uksf.ComboBoxItem") as Style);
                ProfileSelectionControlDropdownProfile.Items.Add(item);

                if (Global.Settings.Firsttimesetupdone && profile.DisplayName == Global.Settings.Profile) {
                    ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(profile);
                }
            }

            if (ProfileSelectionControlDropdownProfile.SelectedIndex == -1 && ProfileHandler.FindUksfProfile(profiles) != null) {
                if (!Global.Settings.Firsttimesetupdone || Global.Settings.Firsttimesetupdone && Global.Settings.Profile == "") {
                    ProfileSelectionControlDropdownProfile.SelectedIndex = profiles.IndexOf(ProfileHandler.FindUksfProfile(profiles));
                }
            }

            RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
        }

        private void ProfileSelectionControlDropdownProfile_Selected(object sender, SelectionChangedEventArgs args) {
            if (ProfileSelectionControlDropdownProfile.SelectedIndex > -1) {
                RaiseEvent(new RoutedEventArgs(PROFILE_SELECTION_CONTROL_UPDATE_EVENT));
            }
        }

        private void ProfileSelectionControlButtonCopy_Click(object sender, RoutedEventArgs args) {
            ProfileCreationControl control = new ProfileCreationControl();
            MessageBoxResult result = DialogWindow.Show("New Profile",
                                                        "Select your rank, enter your last name, and the initial of your first name.\n\nIf you are a new member, your rank will be 'Cdt'.",
                                                        DialogWindow.DialogBoxType.OK_CANCEL,
                                                        control);
            if (result != MessageBoxResult.OK) return;
            ProfileHandler.CopyProfile(((ProfileComboBoxItem) ProfileSelectionControlDropdownProfile.SelectedItem).Profile,
                                       new ProfileHandler.Profile(control.Rank, control.Surname, control.Initial),
                                       Global.Constants.PROFILE_LOCATION_OTHER);
            AddProfiles();
        }
    }
}

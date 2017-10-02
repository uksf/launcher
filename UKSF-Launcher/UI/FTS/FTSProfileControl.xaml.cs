using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsProfileControl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class FtsProfileControl {
        private const string TITLE = "Game Profile";

        private static readonly string DESCRIPTION = "We have selected the Arma 3 profile we think you use for UKSF." + Global.NL +
                                                     "If this is incorrect, select the profile you wish to use from the list below.";

        private static readonly string DESCRIPTION_NOPROFILE = "We can't find an Arma 3 profile suitable for UKSF." + Global.NL +
                                                               "Select a profile from the list below and press 'Copy' to create a new profile with your existing settings." +
                                                               Global.NL + "You may skip this step if you do not wish to create a new profile. (Not recommended)";

        private static FtsProfileControl _instance;
        private List<CustomComboBoxItem> _items;
        private string _profile = "";

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsProfileControl object.
        /// </summary>
        public FtsProfileControl() {
            InitializeComponent();
            _instance = this;
        }

        /// <summary>
        ///     Shows the profile selection control. Adds profiles to the selection.
        /// </summary>
        public static void Show() {
            if (_instance.Visibility == Visibility.Visible) return;
            if (_instance._items == null) {
                _instance.AddProfiles();
            }
            _instance.Visibility = Visibility.Visible;
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = _instance._profile != "" ? DESCRIPTION : DESCRIPTION_NOPROFILE
            });
            _instance.FtsProfileControlButtonCopy.Visibility = _instance._profile == "" ? Visibility.Visible : Visibility.Hidden;
            _instance.UpdateWarning();
        }

        /// <summary>
        ///     Hides the profile selection control.
        /// </summary>
        public static void Hide() => _instance.Visibility = Visibility.Collapsed;

        /// <summary>
        ///     Checks if a warning needs to be displayed and raises a warning event.
        /// </summary>
        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            Visibility visibility = Visibility.Hidden;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(_profile)) {
                visibility = Visibility.Visible;
                warning = "Please select a profile";
                block = true;
            }
            RaiseEvent(new FtsMainControl.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {
                Visibility = visibility,
                Warning = warning,
                Block = block
            });
        }

        /// <summary>
        ///     Adds profiles to profile dropdown. Attempts to find a UKSF profile and selectes it if one is found.
        /// </summary>
        private void AddProfiles() {
            _items = new List<CustomComboBoxItem>();
            FtsProfileControlDropdownProfile.Items.Clear();
            List<ProfileHandler.Profile> profiles = ProfileHandler.GetProfiles();
            foreach (ProfileHandler.Profile profile in profiles) {
                CustomComboBoxItem item = new CustomComboBoxItem(profile, FindResource("Uksf.ComboBoxItem") as Style);
                _items.Add(item);
                FtsProfileControlDropdownProfile.Items.Add(item);
            }

            if (ProfileHandler.FindUksfProfile(profiles) == null) return;
            FtsProfileControlDropdownProfile.SelectedIndex = profiles.IndexOf(ProfileHandler.FindUksfProfile(profiles));
            _profile = _items.ElementAt(FtsProfileControlDropdownProfile.SelectedIndex).ItemProfile.Name;
        }

        /// <summary>
        ///     Triggered when profile selection is changed. Updates profile value and warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void FTSProfileControlProfile_Selected(object sender, SelectionChangedEventArgs args) {
            if (FtsProfileControlDropdownProfile.SelectedIndex > -1) {
                _profile = _items.ElementAt(FtsProfileControlDropdownProfile.SelectedIndex).ItemProfile.Name;
            }
            UpdateWarning();
        }

        /// <summary>
        ///     Triggered when copy button is clicked. Copies selected profile and refreshes profile list in dropdown.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSProfileControlButtonCopy_Click(object sender, RoutedEventArgs args) {
            ProfileHandler.CopyProfile(_items.ElementAt(FtsProfileControlDropdownProfile.SelectedIndex).ItemProfile);
            AddProfiles();
        }
    }
}
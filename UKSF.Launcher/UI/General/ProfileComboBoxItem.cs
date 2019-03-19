using System.Windows;
using System.Windows.Controls;
using UKSF.Launcher.Game;

namespace UKSF.Launcher.UI.General {
    public class ProfileComboBoxItem : ComboBoxItem {
        public ProfileComboBoxItem(ProfileHandler.Profile profile, Style style) {
            Profile = profile;
            Content = Profile.DisplayName;
            Style = style;
        }

        public ProfileHandler.Profile Profile { get; }
    }
}

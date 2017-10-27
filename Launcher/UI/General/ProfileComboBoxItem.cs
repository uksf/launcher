using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI.General {
    public class ProfileComboBoxItem : ComboBoxItem {
        /// <inheritdoc />
        /// <summary>
        ///     Creates ComboBoxItem with given Profile object and style.
        /// </summary>
        /// <param name="profile">Profile to assign ComboBoxItem to</param>
        /// <param name="style">Style to set ComboBoxItem to</param>
        public ProfileComboBoxItem(ProfileHandler.Profile profile, Style style) {
            Profile = profile;
            Content = Profile.DisplayName;
            Style = style;
        }

        public ProfileHandler.Profile Profile { get; }
    }
}
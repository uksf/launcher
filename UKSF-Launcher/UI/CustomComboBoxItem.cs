using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI {
    public class CustomComboBoxItem : ComboBoxItem {
        /// <inheritdoc />
        /// <summary>
        ///     Creates ComboBoxItem with given Profile object and style.
        /// </summary>
        /// <param name="itemProfile">Profile to assign ComboBoxItem to</param>
        /// <param name="style">Style to set ComboBoxItem to</param>
        public CustomComboBoxItem(ProfileHandler.Profile itemProfile, Style style) {
            ItemProfile = itemProfile;
            Content = ItemProfile.DisplayName;
            Style = style;
        }

        public ProfileHandler.Profile ItemProfile { get; }
    }
}
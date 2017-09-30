using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI {
    public class SettingsLauncherControl {
        public class CustomComboBoxItem : ComboBoxItem {
            public CustomComboBoxItem(ProfileHandler.Profile itemProfile, Style style) {
                ItemProfile = itemProfile;
                Content = ItemProfile.DisplayName;
                Style = style;
            }

            public ProfileHandler.Profile ItemProfile { get; }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl {

        public SettingsControl() {
            InitializeComponent();
        }

        private void ButtonLauncher_Click(object sender, RoutedEventArgs e) {
            SettingsButtonLauncher.IsEnabled = false;
            SettingsButtonGame.IsEnabled = true;
            LauncherSettingsControl.Visibility = Visibility.Visible;
            GameSettingsControl.Visibility = Visibility.Hidden;
        }

        private void ButtonGame_Click(object sender, RoutedEventArgs e) {
            SettingsButtonLauncher.IsEnabled = true;
            SettingsButtonGame.IsEnabled = false;
            LauncherSettingsControl.Visibility = Visibility.Hidden;
            GameSettingsControl.Visibility = Visibility.Visible;
        }
    }
}

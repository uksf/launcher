using System.Windows;
using System.Windows.Controls;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Main_SettingsControl.xaml
    /// </summary>
    public partial class Main_SettingsControl : UserControl {

        public Main_SettingsControl() {
            InitializeComponent();
        }

        private void Main_SettingsControl_ButtonLauncher_Click(object sender, RoutedEventArgs e) {
            Main_SettingsControl_ButtonLauncher.IsEnabled = false;
            Main_SettingsControl_ButtonGame.IsEnabled = true;
            Main_SettingsControl_LauncherSettingsControl.Visibility = Visibility.Visible;
            Main_SettingsControl_GameSettingsControl.Visibility = Visibility.Hidden;
        }

        private void Main_SettingsControl_ButtonGame_Click(object sender, RoutedEventArgs e) {
            Main_SettingsControl_ButtonLauncher.IsEnabled = true;
            Main_SettingsControl_ButtonGame.IsEnabled = false;
            Main_SettingsControl_LauncherSettingsControl.Visibility = Visibility.Hidden;
            Main_SettingsControl_GameSettingsControl.Visibility = Visibility.Visible;
        }
    }
}

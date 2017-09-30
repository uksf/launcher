using System.Windows;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for Main_SettingsControl.xaml
    /// </summary>
    public partial class MainSettingsControl {
        public MainSettingsControl() {
            InitializeComponent();
        }

        private void MainSettingsControlButtonLauncher_Click(object sender, RoutedEventArgs e) {
            MainSettingsControlButtonLauncher.IsEnabled = false;
            MainSettingsControlButtonGame.IsEnabled = true;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Visible;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Hidden;
        }

        private void MainSettingsControlButtonGame_Click(object sender, RoutedEventArgs e) {
            MainSettingsControlButtonLauncher.IsEnabled = true;
            MainSettingsControlButtonGame.IsEnabled = false;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Hidden;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Visible;
        }
    }
}
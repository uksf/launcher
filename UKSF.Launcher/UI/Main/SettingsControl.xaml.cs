using System.Windows;
using UKSF.Launcher.UI.Main.Settings;

namespace UKSF.Launcher.UI.Main {
    public partial class SettingsControl {
        public static readonly RoutedEvent MAIN_SETTINGS_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_SETTINGS_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsControl));

        public SettingsControl() {
            AddHandler(MAIN_SETTINGS_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsControlWarning_Update));

            InitializeComponent();

            SettingsControlButtonLauncher.IsEnabled = false;
            SettingsControlButtonGame.IsEnabled = true;
            SettingsControlLauncherSettingsControl.Visibility = Visibility.Visible;
            SettingsControlGameSettingsControl.Visibility = Visibility.Collapsed;
        }

        private void SettingsControlButtonLauncher_Click(object sender, RoutedEventArgs args) {
            SettingsControlButtonLauncher.IsEnabled = false;
            SettingsControlButtonGame.IsEnabled = true;
            SettingsControlLauncherSettingsControl.Visibility = Visibility.Visible;
            SettingsControlGameSettingsControl.Visibility = Visibility.Collapsed;
        }

        private void SettingsControlButtonGame_Click(object sender, RoutedEventArgs args) {
            SettingsControlButtonLauncher.IsEnabled = true;
            SettingsControlButtonGame.IsEnabled = false;
            SettingsControlLauncherSettingsControl.Visibility = Visibility.Collapsed;
            SettingsControlGameSettingsControl.Visibility = Visibility.Visible;
        }

        private void SettingsControlWarning_Update(object sender, RoutedEventArgs args) {
            if (MainWindow.Instance.SettingsControl.SettingsControlLauncherSettingsControl != null) {
                MainWindow.Instance.SettingsControl.SettingsControlLauncherSettingsControl.RaiseEvent(new RoutedEventArgs(SettingsLauncherControl
                                                                                                                              .SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT));
            }

            MainWindow.Instance.SettingsControl.SettingsControlGameSettingsControl.RaiseEvent(new RoutedEventArgs(SettingsGameControl.SETTINGS_GAME_CONTROL_WARNING_EVENT));
        }
    }
}

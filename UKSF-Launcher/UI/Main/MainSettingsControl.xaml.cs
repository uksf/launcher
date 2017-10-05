using System.Windows;
using UKSF_Launcher.UI.Main.Settings;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainSettingsControl.xaml
    /// </summary>
    public partial class MainSettingsControl {
        public static readonly RoutedEvent MAIN_SETTINGS_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_SETTINGS_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainSettingsControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainSettingsControl object.
        /// </summary>
        public MainSettingsControl() {
            AddHandler(MAIN_SETTINGS_CONTROL_WARNING_EVENT, new RoutedEventHandler(MainSettingsControlWarning_Update));

            InitializeComponent();

            MainSettingsControlButtonLauncher.IsEnabled = false;
            MainSettingsControlButtonGame.IsEnabled = true;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Visible;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Triggered when launcher tab button is clicked. Displays launcher settings.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainSettingsControlButtonLauncher_Click(object sender, RoutedEventArgs args) {
            MainSettingsControlButtonLauncher.IsEnabled = false;
            MainSettingsControlButtonGame.IsEnabled = true;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Visible;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Triggered when game tab button is clicked. Displays game settings.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainSettingsControlButtonGame_Click(object sender, RoutedEventArgs args) {
            MainSettingsControlButtonLauncher.IsEnabled = true;
            MainSettingsControlButtonGame.IsEnabled = false;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Collapsed;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Triggered by eventhandler to update warning text and toggle play button state.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Warning arguments</param>
        private void MainSettingsControlWarning_Update(object sender, RoutedEventArgs args) {
            if (MainWindow.Instance.MainSettingsControl.MainSettingsControlLauncherSettingsControl != null) {
                MainWindow.Instance.MainSettingsControl.MainSettingsControlLauncherSettingsControl.RaiseEvent(new RoutedEventArgs(SettingsLauncherControl
                                                                                                                                      .SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT));
            }
            MainWindow.Instance.MainSettingsControl.MainSettingsControlGameSettingsControl.RaiseEvent(new RoutedEventArgs(SettingsGameControl.SETTINGS_GAME_CONTROL_WARNING_EVENT));
        }
    }
}
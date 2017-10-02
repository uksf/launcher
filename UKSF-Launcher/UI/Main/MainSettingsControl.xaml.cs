using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainSettingsControl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MainSettingsControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainSettingsControl object.
        /// </summary>
        public MainSettingsControl() {
            InitializeComponent();
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
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Hidden;
        }

        /// <summary>
        ///     Triggered when game tab button is clicked. Displays game settings.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainSettingsControlButtonGame_Click(object sender, RoutedEventArgs args) {
            MainSettingsControlButtonLauncher.IsEnabled = true;
            MainSettingsControlButtonGame.IsEnabled = false;
            MainSettingsControlLauncherSettingsControl.Visibility = Visibility.Hidden;
            MainSettingsControlGameSettingsControl.Visibility = Visibility.Visible;
        }
    }
}
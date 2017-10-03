using System.Windows;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for SettingsLauncherControl.xaml
    /// </summary>
    public partial class SettingsLauncherControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new SettingsLauncherControl object.
        /// </summary>
        public SettingsLauncherControl() {
            InitializeComponent();

            SettingsLauncherControlVersion.Content = "Version: " + Global.VERSION;
            SettingsLauncherControlAutoupdate.IsChecked = Global.AUTOUPDATELAUNCHER;

            SettingsLauncherControlGameExeTextboxControl.Filter = "exe files|*.exe";
            SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.GAME_LOCATION;
            SettingsLauncherControlDownloadTextboxControl.Directory = true;
            SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.MOD_LOCATION;
        }

        /// <summary>
        ///     Triggered when auto-update launcher checkbox is clicked. Writes state to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void SettingsLauncherControlCheckBoxAUTOUPDATELAUNCHER_Click(object sender, RoutedEventArgs args) {
            Global.AUTOUPDATELAUNCHER = (bool) Core.SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", SettingsLauncherControlAutoupdate.IsChecked);
        }
    }
}
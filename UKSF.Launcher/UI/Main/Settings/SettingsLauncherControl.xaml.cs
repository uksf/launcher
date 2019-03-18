using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF.Launcher.Game;
using UKSF.Launcher.UI.Dialog;
using UKSF.Launcher.UI.General;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.UI.Main.Settings {
    public partial class SettingsLauncherControl {
        public static readonly RoutedEvent SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsLauncherControl));

        private Task _repoMoveTask;

        public SettingsLauncherControl() {
            InitializeComponent();
        }

        public void Initialise() {
            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsLauncherControlLocation_Update));
            AddHandler(SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsLauncherControlLocation_Update));

            SettingsLauncherControlVersion.Content = "Version: " + Global.Settings.VERSION;
            SettingsLauncherControlCheckboxAutoupdateLauncher.IsChecked = Global.Settings.Autoupdatelauncher;

            SettingsLauncherControlGameExeTextboxControl.Filter = "exe files|*.exe";
            SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.Settings.GameLocation;
            SettingsLauncherControlDownloadTextboxControl.Directory = true;
            SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text = Global.Settings.ModLocation;

            UpdateGameExeWarning();
            UpdateDownloadWarning();
        }

        private void SettingsLauncherControlCheckBoxAutoupdateLauncher_Click(object sender, RoutedEventArgs args) {
            Global.Settings.Autoupdatelauncher = (bool) Core.SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", SettingsLauncherControlCheckboxAutoupdateLauncher.IsChecked);
            if (Global.Settings.Autoupdatelauncher) {
                UpdateHandler.UpdateCheck(false);
            }
        }

        private void SettingsLauncherControlLocation_Update(object sender, RoutedEventArgs args) {
            if (Global.Settings.GameLocation != SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                Global.Settings.GameLocation =
                    (string) Core.SettingsHandler.WriteSetting("GAME_LOCATION", SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            }

            if (Global.Settings.ModLocation != SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                if (Global.GameProcess == null) {
                    if (_repoMoveTask == null) {
                        string newLocation = SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text;
                        if (!GameHandler.CheckDriveSpace(SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                            DialogWindow.Show("Drive Space",
                                              $"Not enough drive space at '{newLocation}'.\n\nPlease allow {Global.Constants.REQUIREDSPACE} of space.",
                                              DialogWindow.DialogBoxType.OK);
                        } else {
                            MessageBoxResult result = DialogWindow.Show("Move repo?",
                                                                        $"Are you sure you want to move the repo mods from\n\n'{Global.Settings.ModLocation}'\n to\n'{newLocation}'\n\n\nSelect 'No' to change the repo location without moving mod files",
                                                                        DialogWindow.DialogBoxType.YES_NO_CANCEL);
                            if (result != MessageBoxResult.Cancel) {
                                bool moveRepo = result != MessageBoxResult.No;
                                Core.CancellationTokenSource = new CancellationTokenSource();
                                Task.Run(() => {
                                    _repoMoveTask = Task.Run(() => {
                                                                 MainWindow.Instance.TitleBarControl.TitleBarControlButtonSettings_Click(this, new RoutedEventArgs());
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 1});
                                                                 string finalPath = Global.Repo.MoveRepo(newLocation, moveRepo, Core.CancellationTokenSource.Token);
                                                                 ServerHandler.SendServerMessage("reporequest uksf");
                                                                 MainWindow.Instance.HomeControl
                                                                           .RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                                                                 Global.Settings.ModLocation = (string) Core.SettingsHandler.WriteSetting("MOD_LOCATION", finalPath);
                                                                 Dispatcher.Invoke(() => SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text =
                                                                                             Global.Settings.ModLocation);
                                                             },
                                                             Core.CancellationTokenSource.Token);
                                    _repoMoveTask.Wait();
                                    _repoMoveTask = null;
                                });
                            }
                        }
                    }
                }
            }

            UpdateGameExeWarning();
            UpdateDownloadWarning();
        }

        private void UpdateGameExeWarning() {
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select an Arma 3 exe";
                block = true;
            } else if (Path.GetExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text) != ".exe") {
                warning = "File is not an exe";
                block = true;
            } else if (!Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("arma3")) {
                warning = "File is not an Arma 3 exe";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("battleye")) {
                warning = "Exe cannot be battleye";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("launcher")) {
                warning = "Exe cannot be launcher";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("server")) {
                warning = "Exe cannot be server";
                block = true;
            } else if (Global.Constants.IS64_BIT && !Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text)
                                                         .ToLower()
                                                         .Contains("x64")) {
                warning = "We recommend using the 'arma3_x64' exe";
            }

            SettingsLauncherControlGameExeWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.GAME_LOCATION
            });
        }

        private void UpdateDownloadWarning() {
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select a mod download location";
                block = true;
            } else if (!GameHandler.CheckDriveSpace(SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Not enough drive space";
                block = true;
            }

            SettingsLauncherControlDownloadWarningText.Text = warning;
            if (MainWindow.Instance.HomeControl == null) return;
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(HomeControl.HOME_CONTROL_WARNING_EVENT) {
                Warning = warning, Block = block, CurrentWarning = HomeControl.CurrentWarning.MOD_LOCATION
            });
        }
    }
}

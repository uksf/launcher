using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.Dialog;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.UI.Main.Settings {
    /// <summary>
    ///     Interaction logic for SettingsLauncherControl.xaml
    /// </summary>
    public partial class SettingsLauncherControl {
        public static readonly RoutedEvent SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SettingsLauncherControl));

        private Task _repoMoveTask;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new SettingsLauncherControl object.
        /// </summary>
        public SettingsLauncherControl() {
            InitializeComponent();
        }

        public void Initialise() {
            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(SettingsLauncherControlLocation_Update));
            AddHandler(SETTINGS_LAUNCHER_CONTROL_WARNING_EVENT, new RoutedEventHandler(SettingsLauncherControlLocation_Update));

            SettingsLauncherControlVersion.Content = "Version: " + VERSION;
            SettingsLauncherControlCheckboxAutoupdateLauncher.IsChecked = AUTOUPDATELAUNCHER;

            SettingsLauncherControlGameExeTextboxControl.Filter = "exe files|*.exe";
            SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text = GAME_LOCATION;
            SettingsLauncherControlDownloadTextboxControl.Directory = true;
            SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text = MOD_LOCATION;

            UpdateGameExeWarning();
            UpdateDownloadWarning();
        }

        /// <summary>
        ///     Triggered when auto-update launcher checkbox is clicked. Writes state to settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void SettingsLauncherControlCheckBoxAutoupdateLauncher_Click(object sender, RoutedEventArgs args) {
            AUTOUPDATELAUNCHER = (bool) Core.SettingsHandler.WriteSetting("AUTOUPDATELAUNCHER", SettingsLauncherControlCheckboxAutoupdateLauncher.IsChecked);
            if (AUTOUPDATELAUNCHER) {
                UpdateHandler.UpdateCheck(false);
            }
        }

        /// <summary>
        ///     Triggered by eventhanlder. Updates warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void SettingsLauncherControlLocation_Update(object sender, RoutedEventArgs args) {
            if (GAME_LOCATION != SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                GAME_LOCATION =
                    (string) Core.SettingsHandler.WriteSetting("GAME_LOCATION", SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            }
            if (MOD_LOCATION != SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text) {
                if (GAME_PROCESS == null) {
                    if (_repoMoveTask == null) {
                        MessageBoxResult result =
                            DialogWindow.Show("Move repo?",
                                              $"Are you sure you want to move the repo mods from\n\n'{MOD_LOCATION}'\nto\n'{SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text}'",
                                              DialogWindow.DialogBoxType.YES_NO);
                        if (result == MessageBoxResult.Yes) {
                            Task.Run(() => {
                                _repoMoveTask = Task.Run(() => {
                                    MainWindow.Instance.MainTitleBarControl.MainTitleBarControlButtonSettings_Click(this, new RoutedEventArgs());
                                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {
                                        State = false
                                    });
                                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_STATE_EVENT) {Value = 1});
                                    Core.CancellationTokenSource = new CancellationTokenSource();
                                    string finalPath = REPO.MoveRepo(SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text,
                                                                     Core.CancellationTokenSource.Token);
                                    ServerHandler.SendServerMessage("reporequest uksf");
                                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                                    MOD_LOCATION = (string) Core.SettingsHandler.WriteSetting("MOD_LOCATION", finalPath);
                                });
                                _repoMoveTask.Wait();
                                _repoMoveTask = null;
                            });
                        }
                        SettingsLauncherControlDownloadTextboxControl.LocationTextboxControlTextBoxLocation.Text = MOD_LOCATION;
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
            } else if (IS64BIT && !Path.GetFileNameWithoutExtension(SettingsLauncherControlGameExeTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower()
                                       .Contains("x64")) {
                warning = "We recommend using the 'arma3_x64' exe";
            }
            SettingsLauncherControlGameExeWarningText.Text = warning;
            if (MainWindow.Instance.MainMainControl == null) return;
            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_WARNING_EVENT) {
                Warning = warning,
                Block = block,
                CurrentWarning = MainMainControl.CurrentWarning.GAME_LOCATION
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
            if (MainWindow.Instance.MainMainControl == null) return;
            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.WarningRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_WARNING_EVENT) {
                Warning = warning,
                Block = block,
                CurrentWarning = MainMainControl.CurrentWarning.MOD_LOCATION
            });
        }
    }
}
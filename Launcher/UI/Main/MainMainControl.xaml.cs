using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Network;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.General;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainMainControl.xaml
    /// </summary>
    public partial class MainMainControl {
        public enum CurrentWarning {
            NONE,
            GAME_LOCATION,
            MOD_LOCATION,
            PROFILE
        }

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_PLAY_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_PLAY_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_PROGRESS_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_PROGRESS_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_SERVER_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_SERVER_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_STATE_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_STATE_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        private bool _block = true;

        private CurrentWarning _currentWarning = CurrentWarning.NONE;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainMainControl object.
        /// </summary>
        public MainMainControl() {
            AddHandler(MAIN_MAIN_CONTROL_PLAY_EVENT, new RoutedEventHandler(MainMainControlPlay_Update));
            AddHandler(MAIN_MAIN_CONTROL_PROGRESS_EVENT, new RoutedEventHandler(MainMainControlProgress_Update));
            AddHandler(MAIN_MAIN_CONTROL_WARNING_EVENT, new RoutedEventHandler(MainMainControlWarning_Update));
            AddHandler(MAIN_MAIN_CONTROL_SERVER_EVENT, new RoutedEventHandler(MainMainControlServer_Update));
            AddHandler(MAIN_MAIN_CONTROL_STATE_EVENT, new RoutedEventHandler(MainMainControlState_Update));

            InitializeComponent();

            MainMainControlProgressBar.Visibility = Visibility.Collapsed;
            MainMainControlSecondaryProgressBar.Visibility = Visibility.Collapsed;
            MainMainControlProgressText.Visibility = Visibility.Collapsed;
            MainMainControlDropdownServer.Visibility = Visibility.Collapsed;
            // TODO: Sims-esque loading messages
            // TODO: Implement background workers for all non-ui code, using progresschanged event for updating ui
        }

        private void MainMainControlPlay_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.BoolRoutedEventArgs boolArgs = (SafeWindow.BoolRoutedEventArgs) args;
                if (_block && !string.Equals(MainMainControlWarningText.Text, "", StringComparison.InvariantCultureIgnoreCase)) return;
                MainMainControlButtonPlay.IsEnabled = boolArgs.State;
                MainMainControlDropdownServer.IsEnabled = boolArgs.State;
            });
        }

        private void MainMainControlProgress_Update(object sender, RoutedEventArgs args) {
            SafeWindow.ProgressRoutedEventArgs progressArgs = (SafeWindow.ProgressRoutedEventArgs) args;
            if (!progressArgs.Message.Contains("stop")) {
                MainMainControlProgressBar.Visibility = Visibility.Visible;
                MainMainControlProgressText.Visibility = Visibility.Visible;
                MainMainControlSecondaryProgressBar.Visibility = Visibility.Visible;
                MainMainControlProgressBar.Value = progressArgs.Value;
                MainMainControlProgressText.Text = progressArgs.Message;
                MainMainControlSecondaryProgressBar.Value = progressArgs.SecondaryValue;
            } else {
                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                MainMainControlProgressBar.Visibility = Visibility.Collapsed;
                MainMainControlSecondaryProgressBar.Visibility = Visibility.Collapsed;
                MainMainControlProgressText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Triggered by eventhandler to update warning text and toggle play button state.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Warning arguments</param>
        private void MainMainControlWarning_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.WarningRoutedEventArgs warningArgs = (SafeWindow.WarningRoutedEventArgs) args;
                CurrentWarning previousWarning = _currentWarning;
                if (_currentWarning != CurrentWarning.NONE && _currentWarning != warningArgs.CurrentWarning) return;
                _block = warningArgs.Block;
                MainMainControlWarningText.Text = warningArgs.Warning;
                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MAIN_MAIN_CONTROL_PLAY_EVENT) {State = !_block});
                _currentWarning = warningArgs.Warning == "" ? CurrentWarning.NONE : warningArgs.CurrentWarning;
                if (_currentWarning != previousWarning) {
                    MainWindow.Instance.MainSettingsControl.RaiseEvent(new RoutedEventArgs(MainSettingsControl.MAIN_SETTINGS_CONTROL_WARNING_EVENT));
                }
            });
        }

        /// <summary>
        ///     Triggered by eventhandler to update server dropdown state.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Server arguments</param>
        private void MainMainControlServer_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.ServerRoutedEventArgs serverArgs = (SafeWindow.ServerRoutedEventArgs) args;
                List<Server> servers = serverArgs.Servers.Where(server => server.Active).ToList();
                if (servers.Count > 0) {
                    MainMainControlDropdownServer.Visibility = Visibility.Visible;
                    MainMainControlDropdownServer.Items.Clear();
                    MainMainControlDropdownServer.Items.Add(new ServerComboBoxItem(ServerHandler.NO_SERVER, FindResource("Uksf.ComboBoxItemPlay") as Style));
                    foreach (ServerComboBoxItem serverComboBoxItem in servers.Select(server => new ServerComboBoxItem(server, FindResource("Uksf.ComboBoxItemPlay") as Style))) {
                        MainMainControlDropdownServer.Items.Add(serverComboBoxItem);
                        if (Global.SERVER != null && serverComboBoxItem.Server.Name == Global.SERVER.Name) {
                            MainMainControlDropdownServer.SelectedItem = serverComboBoxItem;
                        }
                    }
                } else {
                    MainMainControlDropdownServer.Visibility = Visibility.Collapsed;
                    Global.SERVER = null;
                }
                MainMainControlDropdownServer_Selected(null, null);
            });
        }


        /// <summary>
        ///     Triggered by eventhandler to update refresh/cancel state.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Server arguments</param>
        private void MainMainControlState_Update(object sender, RoutedEventArgs args) {
            Dispatcher.Invoke(() => {
                SafeWindow.IntRoutedEventArgs stateArgs = (SafeWindow.IntRoutedEventArgs) args;
                switch (stateArgs.Value) {
                    case 1:
                        MainMainControlRefreshCancelButton.Content = "Cancel";
                        break;
                    case 2:
                        MainMainControlRefreshCancelButton.Content = "Kill Game";
                        break;
                    default:
                        MainMainControlRefreshCancelButton.Content = "Refresh";
                        break;
                }
            });
        }

        /// <summary>
        ///     Triggered when play button is clicked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainMainControlButtonPlay_Click(object sender, RoutedEventArgs args) {
            GameHandler.StartGame();
        }

        /// <summary>
        ///     Triggered when server dropdown selection changes.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selection arguments</param>
        private void MainMainControlDropdownServer_Selected(object sender, SelectionChangedEventArgs args) {
            if (MainMainControlDropdownServer.SelectedItem != null) {
                Global.SERVER = ((ServerComboBoxItem) MainMainControlDropdownServer.SelectedItem).Server;
                if (Equals(Global.SERVER, ServerHandler.NO_SERVER)) {
                    MainMainControlButtonPlay.Content = "Play";
                    MainMainControlButtonPlay.FontSize = 50;
                } else {
                    MainMainControlButtonPlay.Content = Global.SERVER.Name;
                    MainMainControlButtonPlay.FontSize = 30;
                }
            } else {
                MainMainControlButtonPlay.Content = "Play";
                MainMainControlButtonPlay.FontSize = 50;
            }
        }

        private async void MainMainControlRefreshCancelButton_Click(object sender, RoutedEventArgs args) {
            MainMainControlRefreshCancelButton.IsEnabled = false;
            if (Global.GAME_PROCESS == null) {
                if (MainMainControlRefreshCancelButton.Content.Equals("Cancel")) {
                    Core.CancellationTokenSource.Cancel();
                    await Task.Delay(250);
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(MAIN_MAIN_CONTROL_PROGRESS_EVENT) { Value = 1, Message = "stop" });
                } else if (MainMainControlRefreshCancelButton.Content.Equals("Refresh")) {
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MAIN_MAIN_CONTROL_STATE_EVENT) { Value = 1 });
                    ServerHandler.SendServerMessage("reporequest uksf");
                }
            } else {
                Global.GAME_PROCESS.Kill();
                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MAIN_MAIN_CONTROL_STATE_EVENT) { Value = 0 });
            }
            await Task.Delay(250);
            MainMainControlRefreshCancelButton.IsEnabled = true;
        }
    }
}
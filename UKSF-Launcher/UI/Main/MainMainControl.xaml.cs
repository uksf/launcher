using System.Windows;
using System.Windows.Controls;
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

        public static readonly RoutedEvent MAIN_MAIN_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_MAIN_CONTROL_WARNING_EVENT", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MainMainControl));

        private CurrentWarning _currentWarning = CurrentWarning.NONE;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainMainControl object.
        /// </summary>
        public MainMainControl() {
            AddHandler(MAIN_MAIN_CONTROL_WARNING_EVENT, new RoutedEventHandler(MainMainControlWarning_Update));

            InitializeComponent();
        }

        /// <summary>
        ///     Triggered by eventhandler to update warning text and toggle play button state.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Warning arguments</param>
        private void MainMainControlWarning_Update(object sender, RoutedEventArgs args) {
            SafeWindow.WarningRoutedEventArgs warningArgs = (SafeWindow.WarningRoutedEventArgs) args;
            CurrentWarning previousWarning = _currentWarning;
            if (_currentWarning != CurrentWarning.NONE && _currentWarning != warningArgs.CurrentWarning) return;
            MainMainControlWarningText.Text = warningArgs.Warning;
            MainMainControlButtonPlay.IsEnabled = !warningArgs.Block;
            _currentWarning = warningArgs.Warning == "" ? CurrentWarning.NONE : warningArgs.CurrentWarning;
            if (_currentWarning != previousWarning) {
                MainWindow.Instance.MainSettingsControl.RaiseEvent(new RoutedEventArgs(MainSettingsControl.MAIN_SETTINGS_CONTROL_WARNING_EVENT));
            }
        }

        /// <summary>
        ///     Triggered when play button is clicked. Does nothing.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainMainControlButtonPlay_Click(object sender, RoutedEventArgs args) {
            GameHandler.StartGame();
        }

        private void MainMainControlDropdownServer_Selected(object sender, SelectionChangedEventArgs args) {
            
        }
    }
}
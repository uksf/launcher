using System.Windows;

namespace UKSF_Launcher.UI.Main {
    /// <inheritdoc cref="SafeWindow" />
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public static MainMainControl MainControl;
        public static MainSettingsControl SettingsControl;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainWindow object.
        /// </summary>
        public MainWindow() {
            InitializeComponent();

            MainControl = MainMainControl;
            SettingsControl = MainSettingsControl;

            AddHandler(MainTitleBarControl.MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(MainTitleBar_MouseDown));
            AddHandler(MainTitleBarControl.MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT, new RoutedEventHandler(MainTitleBarButtonMinimize_Click));
        }

        /// <summary>
        ///     Triggered by eventhandler to move window when title bar is held.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void MainTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        /// <summary>
        ///     Triggered by eventhandler to minimize window when minimize button is clicked.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainTitleBarButtonMinimize_Click(object sender, RoutedEventArgs args) => WindowState = WindowState.Minimized;
    }
}
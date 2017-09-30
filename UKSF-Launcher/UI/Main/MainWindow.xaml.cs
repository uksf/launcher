using System.Windows;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public static MainMainControl MainControl;
        public static MainSettingsControl SettingsControl;

        public MainWindow() {
            InitializeComponent();

            MainControl = MainMainControl;
            SettingsControl = MainSettingsControl;

            AddHandler(MainTitleBarControl.MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(MainTitleBar_MouseDown));
            AddHandler(MainTitleBarControl.MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT, new RoutedEventHandler(MainTitleBarButtonMinimize_Click));
        }

        private void MainTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void MainTitleBarButtonMinimize_Click(object sender, RoutedEventArgs args) => WindowState = WindowState.Minimized;
    }
}
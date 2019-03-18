using System.Windows;

namespace UKSF.Launcher.UI.Main {
    public partial class MainWindow {
        public static MainWindow Instance;

        public MainWindow() {
            Instance = this;

            AddHandler(TitleBarControl.MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(MainTitleBar_MouseDown));
            AddHandler(TitleBarControl.MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT, new RoutedEventHandler(MainTitleBarButtonMinimize_Click));

            InitializeComponent();

            SettingsControl.SettingsControlLauncherSettingsControl.Initialise();
            SettingsControl.SettingsControlGameSettingsControl.Initialise();
        }

        private void MainTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void MainTitleBarButtonMinimize_Click(object sender, RoutedEventArgs args) => WindowState = WindowState.Minimized;
    }
}

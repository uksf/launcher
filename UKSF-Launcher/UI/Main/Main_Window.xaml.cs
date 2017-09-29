using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Main_Window : SafeWindow {

        public static Main_MainControl mainControl;
        public static Main_SettingsControl settingsControl;

        public Main_Window() {
            InitializeComponent();

            mainControl = Main_MainControl;
            settingsControl = Main_SettingsControl;

            AddHandler(Main_TitleBarControl.Main_TitleBarControl_MouseDown_Event, new RoutedEventHandler(Main_TitleBar_MouseDown));
            AddHandler(Main_TitleBarControl.Main_TitleBarControl_ButtonMinimizeClick_Event, new RoutedEventHandler(Main_TitleBar_ButtonMinimize_Click));
        }

        private void Main_TitleBar_MouseDown(object sender, RoutedEventArgs args) {
            DragMove();
        }

        private void Main_TitleBar_ButtonMinimize_Click(object sender, RoutedEventArgs args) {
            WindowState = WindowState.Minimized;
        }
    }
}

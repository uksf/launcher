using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Main_Window : Window {

        public static Main_MainControl mainControl;
        public static Main_SettingsControl settingsControl;

        public Main_Window() {
            InitializeComponent();

            mainControl = Main_MainControl;
            settingsControl = Main_SettingsControl;
        }
    }
}

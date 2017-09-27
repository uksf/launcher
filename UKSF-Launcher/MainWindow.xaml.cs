using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        public static TitleBarControl titleBarControl;
        public static MainControl mainControl;
        public static SettingsControl settingsControl;

        public MainWindow() {
            InitializeComponent();
            
            titleBarControl = TitleBarControl;
            mainControl = MainControl;
            settingsControl = SettingsControl;
        }
    }
}

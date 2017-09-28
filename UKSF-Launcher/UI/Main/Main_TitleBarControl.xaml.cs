using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Main_TitleBarControl.xaml
    /// </summary>
    public partial class Main_TitleBarControl : UserControl {
        public Main_TitleBarControl() {
            InitializeComponent();
        }

        private void Main_TitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                Application.Current.MainWindow.DragMove();
            }
        }

        private void Main_TitleBarControl_ButtonClose_Click(object sender, RoutedEventArgs args) {
            Application.Current.Shutdown();
        }

        private void Main_TitleBarControl_ButtonMinimize_Click(object sender, RoutedEventArgs args) {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Main_TitleBarControl_ButtonSettings_Click(object sender, RoutedEventArgs args) {
            var main = Main_Window.mainControl;
            var settings = Main_Window.settingsControl;
            if (main.Visibility == Visibility.Visible) {
                main.Visibility = Visibility.Hidden;
                settings.Visibility = Visibility.Visible;
            }
            else {
                main.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for TitleBarControl.xaml
    /// </summary>
    public partial class TitleBarControl : UserControl {
        public TitleBarControl() {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                Application.Current.MainWindow.DragMove();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs args) {
            Application.Current.Shutdown();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs args) {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs args) {
            var main = MainWindow.mainControl;
            var settings = MainWindow.settingsControl;
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();

            new LogHandler();
            new Core();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                Application.Current.MainWindow.DragMove();
            }
        }

        private void ButtonClose_Clicked(object sender, RoutedEventArgs args) {
            Application.Current.Shutdown();
        }

        private void ButtonMinimize_Clicked(object sender, RoutedEventArgs args) {
            WindowState = WindowState.Minimized;
        }

        private void ButtonSettings_Clicked(object sender, RoutedEventArgs args) {
            var main = (Grid) FindName("Main");
            var settings = (Grid) FindName("Settings");
            if (main.Visibility == Visibility.Visible) {
                main.Visibility = Visibility.Hidden;
                settings.Visibility = Visibility.Visible;
            }
            else {
                main.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonPlay_Clicked(object sender, RoutedEventArgs args) {
            Console.WriteLine("PRESSED");
        }
    }
}

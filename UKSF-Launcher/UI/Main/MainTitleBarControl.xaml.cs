using System.Windows;
using System.Windows.Input;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for Main_TitleBarControl.xaml
    /// </summary>
    public partial class MainTitleBarControl {
        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainTitleBarControl));

        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                                             typeof(MainTitleBarControl));

        public MainTitleBarControl() {
            InitializeComponent();
        }

        private void MainTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        private void MainTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) => Core.ShutDown();

        private void MainTitleBarControlButtonMinimize_Click(object sender, RoutedEventArgs args) => RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT));

        private void MainTitleBarControlButtonSettings_Click(object sender, RoutedEventArgs args) {
            MainMainControl main = MainWindow.MainControl;
            MainSettingsControl settings = MainWindow.SettingsControl;
            if (main.Visibility == Visibility.Visible) {
                main.Visibility = Visibility.Hidden;
                settings.Visibility = Visibility.Visible;
            } else {
                main.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
            }
        }
    }
}
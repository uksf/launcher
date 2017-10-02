using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainTitleBarControl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MainTitleBarControl {
        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainTitleBarControl));

        public static readonly RoutedEvent MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                                             typeof(MainTitleBarControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainTitleBarControl object.
        /// </summary>
        public MainTitleBarControl() {
            InitializeComponent();
        }

        /// <summary>
        ///     Triggered when title bar is held. Raises event for moving window if left mouse button is held.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void MainTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        /// <summary>
        ///     Triggered when close button is clicked. Runs application shutdown.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) => Core.ShutDown();

        /// <summary>
        ///     Triggered when minimize button is clicked. Minimizes window.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainTitleBarControlButtonMinimize_Click(object sender, RoutedEventArgs args) =>
            RaiseEvent(new RoutedEventArgs(MAIN_TITLE_BAR_CONTROL_BUTTON_MINIMIZE_CLICK_EVENT));

        /// <summary>
        ///     Triggered when settings button is clicked. Switches main control between main and settings display.
        /// </summary>
        /// <param name="sender">Sender bject</param>
        /// <param name="args">Click arguments</param>
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
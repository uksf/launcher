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

        public static readonly RoutedEvent Main_TitleBarControl_MouseDown_Event = EventManager.RegisterRoutedEvent("Main_TitleBarControl_MouseDown_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Main_TitleBarControl));
        public static readonly RoutedEvent Main_TitleBarControl_ButtonMinimizeClick_Event = EventManager.RegisterRoutedEvent("Main_TitleBarControl_ButtonMinimizeClick_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Main_TitleBarControl));

        public event RoutedEventHandler Main_TitleBarControl_MouseDown_EventHandler {
            add { AddHandler(Main_TitleBarControl_MouseDown_Event, value); }
            remove { RemoveHandler(Main_TitleBarControl_MouseDown_Event, value); }
        }
        public event RoutedEventHandler Main_TitleBarControl_ButtonMinimizeClick_EventHandler {
            add { AddHandler(Main_TitleBarControl_ButtonMinimizeClick_Event, value); }
            remove { RemoveHandler(Main_TitleBarControl_ButtonMinimizeClick_Event, value); }
        }

        private void Main_TitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(Main_TitleBarControl_MouseDown_Event));
            }
        }

        private void Main_TitleBarControl_ButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }

        private void Main_TitleBarControl_ButtonMinimize_Click(object sender, RoutedEventArgs args) {
            RaiseEvent(new RoutedEventArgs(Main_TitleBarControl_ButtonMinimizeClick_Event));
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

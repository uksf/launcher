using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Dialog_TitleBarControl.xaml
    /// </summary>
    public partial class Dialog_TitleBarControl : UserControl {
        public Dialog_TitleBarControl() {
            InitializeComponent();
        }

        public static readonly RoutedEvent Dialog_TitleBarControl_MouseDown_Event = EventManager.RegisterRoutedEvent("TitleBarMouseDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Dialog_TitleBarControl));

        public event RoutedEventHandler Dialog_TitleBarControl_MouseDown_EventHandler {
            add { AddHandler(Dialog_TitleBarControl_MouseDown_Event, value); }
            remove { RemoveHandler(Dialog_TitleBarControl_MouseDown_Event, value); }
        }

        private void Dialog_TitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(Dialog_TitleBarControl_MouseDown_Event));
            }
        }

        private void Dialog_TitleBarControl_ButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }
    }
}

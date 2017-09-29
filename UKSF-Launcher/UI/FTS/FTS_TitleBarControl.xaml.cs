using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_TitleBarControl.xaml
    /// </summary>
    public partial class FTS_TitleBarControl : UserControl {
        public FTS_TitleBarControl() {
            InitializeComponent();
        }

        public static readonly RoutedEvent FTS_TitleBarControl_MouseDown_Event = EventManager.RegisterRoutedEvent("FTS_TitleBarControl_MouseDown_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FTS_TitleBarControl));

        public event RoutedEventHandler FTS_TitleBarControl_MouseDown_EventHandler {
            add { AddHandler(FTS_TitleBarControl_MouseDown_Event, value); }
            remove { RemoveHandler(FTS_TitleBarControl_MouseDown_Event, value); }
        }

        private void FTS_TitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(FTS_TitleBarControl_MouseDown_Event));
            }
        }

        private void FTS_TitleBarControl_ButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Dialog_MainControl.xaml
    /// </summary>
    public partial class Dialog_MainControl : UserControl {

        public Dialog_MainControl() {
            InitializeComponent();
        }

        public static readonly RoutedEvent Dialog_MainControl_ButtonOKClick_Event = EventManager.RegisterRoutedEvent("Dialog_MainControl_ButtonOK_Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Dialog_MainControl));
        public static readonly RoutedEvent Dialog_MainControl_ButtonCancelClick_Event = EventManager.RegisterRoutedEvent("Dialog_MainControl_ButtonCancel_Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Dialog_MainControl));

        public event RoutedEventHandler Dialog_MainControl_ButtonOK_Click_EventHandler {
            add { AddHandler(Dialog_MainControl_ButtonOKClick_Event, value); }
            remove { RemoveHandler(Dialog_MainControl_ButtonOKClick_Event, value); }
        }
        public event RoutedEventHandler Dialog_MainControl_ButtonCancel_Click_EventHandler {
            add { AddHandler(Dialog_MainControl_ButtonCancelClick_Event, value); }
            remove { RemoveHandler(Dialog_MainControl_ButtonCancelClick_Event, value); }
        }

        private void Dialog_MainControl_Button_Click(object sender, RoutedEventArgs args) {
            if (sender == Dialog_MainControl_ButtonOK) {
                RaiseEvent(new RoutedEventArgs(Dialog_MainControl_ButtonOKClick_Event));
            }
            else if (sender == Dialog_MainControl_ButtonCancel) {
                RaiseEvent(new RoutedEventArgs(Dialog_MainControl_ButtonCancelClick_Event));
            }
        }
    }
}

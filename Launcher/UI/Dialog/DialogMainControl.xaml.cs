using System.Windows;

namespace UKSF_Launcher.UI.Dialog {
    /// <summary>
    ///     Interaction logic for DialogMainControl.xaml
    /// </summary>
    public partial class DialogMainControl {
        public static readonly RoutedEvent DIALOG_MAIN_CONTROL_BUTTON_OK_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_MAIN_CONTROL_BUTTON_OK_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DialogMainControl));

        public static readonly RoutedEvent DIALOG_MAIN_CONTROL_BUTTON_CANCEL_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_MAIN_CONTROL_BUTTON_CANCEL_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DialogMainControl));
        
        public static readonly RoutedEvent DIALOG_MAIN_CONTROL_BUTTON_YES_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_MAIN_CONTROL_BUTTON_YES_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DialogMainControl));

        public static readonly RoutedEvent DIALOG_MAIN_CONTROL_BUTTON_NO_CLICK_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_MAIN_CONTROL_BUTTON_NO_CLICK_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DialogMainControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new DialogMainControl object.
        /// </summary>
        public DialogMainControl() {
            InitializeComponent();
        }

        /// <summary>
        ///     Triggered when control button is clicked. Raises OK or Cancel event.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void DialogMainControlButton_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, DialogMainControlButtonOk)) {
                RaiseEvent(new RoutedEventArgs(DIALOG_MAIN_CONTROL_BUTTON_OK_CLICK_EVENT));
            } else if (Equals(sender, DialogMainControlButtonCancel)) {
                RaiseEvent(new RoutedEventArgs(DIALOG_MAIN_CONTROL_BUTTON_CANCEL_CLICK_EVENT));
            } else if (Equals(sender, DialogMainControlButtonYes)) {
                RaiseEvent(new RoutedEventArgs(DIALOG_MAIN_CONTROL_BUTTON_YES_CLICK_EVENT));
            } else if (Equals(sender, DialogMainControlButtonNo)) {
                RaiseEvent(new RoutedEventArgs(DIALOG_MAIN_CONTROL_BUTTON_NO_CLICK_EVENT));
            }
        }
    }
}
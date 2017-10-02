using System.Windows;
using System.Windows.Input;

namespace UKSF_Launcher.UI.Dialog {
    /// <summary>
    ///     Interaction logic for Dialog_TitleBarControl.xaml
    /// </summary>
    public partial class DialogTitleBarControl {
        public static readonly RoutedEvent DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                                             typeof(DialogTitleBarControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new DialogTitleBarControl object.
        /// </summary>
        public DialogTitleBarControl() {
            InitializeComponent();
        }

        /// <summary>
        ///     Triggered when title bar is held. Raises event for moving window if left mouse button is held.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void DialogTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        /// <summary>
        ///     Triggered when close button is clicked. Runs application shutdown.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void DialogTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }
    }
}
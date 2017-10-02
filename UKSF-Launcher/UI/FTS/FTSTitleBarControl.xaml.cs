using System.Windows;
using System.Windows.Input;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsTitleBarControl.xaml
    /// </summary>
    public partial class FtsTitleBarControl {
        public static readonly RoutedEvent FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT =
            EventManager.RegisterRoutedEvent("FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsTitleBarControl));

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsTitleBarControl object.
        /// </summary>
        public FtsTitleBarControl() {
            InitializeComponent();
        }

        /// <summary>
        ///     Triggered when title bar is held. Raises event for moving window if left mouse button is held.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void FTSTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                RaiseEvent(new RoutedEventArgs(FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT));
            }
        }

        /// <summary>
        ///     Triggered when close button is clicked. Runs application shutdown.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) => Core.ShutDown();
    }
}
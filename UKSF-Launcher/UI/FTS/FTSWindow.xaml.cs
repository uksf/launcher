using System.Diagnostics.CodeAnalysis;
using System.Windows;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <inheritdoc cref="SafeWindow" />
    /// <summary>
    ///     Interaction logic for FtsWindow.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class FtsWindow {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsWindow object.
        /// </summary>
        public FtsWindow() {
            InitializeComponent();

            AddHandler(FtsTitleBarControl.FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            AddHandler(FtsMainControl.FTS_MAIN_CONTROL_FINISH_EVENT, new RoutedEventHandler(FTS_Finish));
        }

        /// <summary>
        ///     Triggered by eventhandler to move window when title bar is held.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        /// <summary>
        ///     Triggered by eventhandler to finish first time setup and close the window.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTS_Finish(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("First time setup finished");
            Global.FIRSTTIMESETUPDONE = (bool) SettingsHandler.WriteSetting("FIRSTTIMESETUPDONE", true);
            Close();
        }
    }
}
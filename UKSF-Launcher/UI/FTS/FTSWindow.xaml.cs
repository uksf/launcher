using System.Windows;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FTS_Window.xaml
    /// </summary>
    public partial class FtsWindow {
        public FtsWindow() {
            InitializeComponent();

            AddHandler(FtsTitleBarControl.FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            AddHandler(FtsMainControl.FTS_MAIN_CONTROL_FINISH_EVENT, new RoutedEventHandler(FTS_Finish));
        }

        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void FTS_Finish(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("First time setup finished");
            Global.FIRSTTIMESETUPDONE = (bool)SettingsHandler.WriteSetting("FIRSTTIMESETUPDONE", true);
            Close();
        }
    }
}
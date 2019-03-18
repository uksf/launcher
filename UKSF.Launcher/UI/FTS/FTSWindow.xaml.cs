using System.Windows;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.UI.FTS {
    public partial class FtsWindow {
        public FtsWindow() {
            InitializeComponent();

            AddHandler(FtsTitleBarControl.FTS_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            AddHandler(FtsMainControl.FTS_MAIN_CONTROL_FINISH_EVENT, new RoutedEventHandler(FTS_Finish));
        }

        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private void FTS_Finish(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("First time setup finished");
            Global.Settings.Firsttimesetupdone = (bool) Core.SettingsHandler.WriteSetting("FIRSTTIMESETUPDONE", true);
            Close();
        }
    }
}

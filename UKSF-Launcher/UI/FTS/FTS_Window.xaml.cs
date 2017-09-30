using System.Windows;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_Window.xaml
    /// </summary>
    public partial class FTS_Window : SafeWindow {

        public FTS_Window() {
            InitializeComponent();

            AddHandler(FTS_TitleBarControl.FTS_TitleBarControl_MouseDown_Event, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            AddHandler(FTS_MainControl.FTS_MainControl_Finish_Event, new RoutedEventHandler(FTS_Finish));
        }

        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) {
            DragMove();
        }

        private void FTS_Finish(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("First time setup finished");
            Close();
        }
    }
}

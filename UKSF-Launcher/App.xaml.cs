using System;
using System.Windows;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application {

        void App_Startup(object sender, StartupEventArgs e) {
            bool updated = false;

            for (int i = 0; i != e.Args.Length; ++i) {
                if (e.Args[i] == "-u") {
                    updated = true;
                }
            }

            new Core(updated);
        }

        private void Application_Exit(object sender, EventArgs args) {
            Utility.LogHandler.CloseLog();
        }
    }
}

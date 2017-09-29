using System;
using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application {

        void App_Startup(object sender, StartupEventArgs args) {
            bool updated = false;

            for (int i = 0; i != args.Args.Length; ++i) {
                if (args.Args[i] == "-u") {
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

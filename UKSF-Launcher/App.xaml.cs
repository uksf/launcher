using System;
using System.Windows;

using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application {

        void App_Startup(object sender, StartupEventArgs e) {
            for (int i = 0; i != e.Args.Length; ++i) {
                if (e.Args[i] == "-u") {
                    UPDATER = true;
                }
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, EventArgs args) {
            Utility.LogHandler.CloseLog();
        }
    }
}

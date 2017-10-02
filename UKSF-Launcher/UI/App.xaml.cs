using System;
using System.Windows;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI {
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        /// <summary>
        ///     Entry point for Application. Reads and extracts arguments, and runs program start routine.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Startup arguments</param>
        private void App_Startup(object sender, StartupEventArgs args) {
            bool updated = false;

            for (int i = 0; i != args.Args.Length; ++i) {
                if (args.Args[i] == "-u") {
                    updated = true;
                }
            }

            new Core(updated);
        }

        /// <summary>
        ///     Triggered on Application exit to close the log file.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Shutdown arguments</param>
        private void Application_Exit(object sender, EventArgs args) {
            LogHandler.CloseLog();
        }
    }
}
using System;
using System.Windows;
using UKSF_Launcher.UI.Dialog;
using UKSF_Launcher.UI.FTS;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    internal static class Core {
        /// <summary>
        ///     Application starting point
        /// </summary>
        /// <param name="updated">Determines if the launcher has been updated</param>
        public static void Start(bool updated) {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LogHandler.StartLogging();
            LogHandler.LogHashSpaceMessage(Severity.INFO, "Launcher Started");
            SettingsHandler.ReadSettings();
            UpdateHandler.UpdateCheck(updated);

            if (!FIRSTTIMESETUPDONE) {
                LogHandler.LogHashSpaceMessage(Severity.INFO, "Running first time setup");
                new FtsWindow().ShowDialog();
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.Focus();
        }

        /// <summary>
        ///     Shuts the application down.
        ///     If there is no instance of Applicaiton, exit forcefully.
        /// </summary>
        public static void ShutDown() {
            if (Application.Current == null) {
                Environment.Exit(0);
            } else {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        ///     Logs an error and displays a dialog with the error message.
        /// </summary>
        /// <param name="exception">Error exception to report</param>
        public static void Error(Exception exception) {
            string error = exception.Message + "\n" + exception.StackTrace;
            LogHandler.LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result =
                DialogWindow.Show("Error", "Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error,
                                  DialogWindow.DialogBoxType.OK);
            if (result == MessageBoxResult.OK) {
                ShutDown();
            }
        }
    }
}
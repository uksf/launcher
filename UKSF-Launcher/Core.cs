using System;
using System.Windows;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    internal static class Core {
        public static void Start(bool updated) {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LogHandler.StartLogging();
            LogHandler.LogHashSpaceMessage(Severity.INFO, "Launcher Started");
            SettingsHandler.ReadSettings();
            UpdateHandler.UpdateCheck(updated);

            if (!FIRSTTIMESETUPDONE) {
                SetupHandler.FirstTimeSetup();
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.Focus();
        }

        public static void ShutDown() {
            if (Application.Current == null) {
                Environment.Exit(0);
            } else {
                Application.Current.Shutdown();
            }
        }

        public static void Error(Exception exception) {
            string error = exception.Message + "\n" + exception.StackTrace;
            LogHandler.LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result =
                UI.Dialog.DialogWindow.Show("Error", "Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error,
                                  UI.Dialog.DialogWindow.DialogBoxType.OK);
            if (result == MessageBoxResult.OK) {
                ShutDown();
            }
        }
    }
}
using System;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    class Core {

        public Core(bool updated) {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LogHandler.StartLogging();
            LogHandler.LogHashSpaceMessage(Severity.INFO, "Launcher Started");
            SettingsHandler.ReadSettings();
            UpdateHandler.UpdateCheck(updated);

            GameHandler.CheckGameInstalled();

            if (!FIRSTTIMESETUPDONE) {
                SetupHandler.FirstTimeSetup();
            }

            Main_Window mainWindow = new Main_Window();
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
            return;
        }

        public static void Error(Exception exception) {
            var error = exception.Message + "\n" + exception.StackTrace;
            LogHandler.LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result = Dialog_Window.Show("Error", "Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error, Dialog_Window.DialogBoxType.OK);
            if (result == MessageBoxResult.OK) {
                ShutDown();
            }
        }
    }
}

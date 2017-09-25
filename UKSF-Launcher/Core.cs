using System;
using System.Windows;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher {
    class Core {

        public Core() {
            LogHandler.StartLogging();
            LogHandler.LogHashSpace(Severity.INFO, "Launcher Started");

            SettingsHandler.ReadSettings();
            UpdateHandler.UpdateCheck();
        }

        public static void Error(Exception exception) {
            var error = exception.Message + "\n" + exception.StackTrace;
            Console.WriteLine(error);
            LogHandler.LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result = MessageBox.Show("Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (result == MessageBoxResult.OK) {
                Application.Current.Shutdown();
            }
        }
    }
}

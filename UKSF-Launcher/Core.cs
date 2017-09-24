using System;
using System.Windows;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher {
    class Core {

        public Core() {
            LogHandler.LogHashSpace(LogHandler.Severity.INFO, "Launcher Started");
        }

        public static void Error(Exception exception) {
            var error = exception.Message + "\n" + exception.StackTrace;
            Console.WriteLine(error);
            LogHandler.LogSeverity(LogHandler.Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result = MessageBox.Show("Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (result == MessageBoxResult.OK) {
                Application.Current.Shutdown();
            }
        }
    }
}

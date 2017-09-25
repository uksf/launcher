using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher {
    class Core {

        public Core() {
            LogHandler.LogHashSpace(Severity.INFO, "Launcher Started");

            UpdateCheck();
        }

        private void UpdateCheck() {
            Version currentVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            Version latestVersion = Version.Parse(new WebClient().DownloadString("http://www.uk-sf.com/launcher/release/version"));
            LogHandler.LogSeverity(Severity.INFO, "Current version: " + currentVersion);
            LogHandler.LogSeverity(Severity.INFO, "Latest version: " + latestVersion);
#if FORCEUPDATE
            currentVersion = Version.Parse("0.0.0");
#endif
            if (currentVersion < latestVersion) {
                LogHandler.LogSeverity(Severity.INFO, "Updating to " + latestVersion);
                Update();
            }
            if (UPDATER) {
                File.Delete(Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            }
        }

        private void Update() {
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Updater.exe", Path.Combine(Environment.CurrentDirectory, "Updater.exe"));
            Process updater = new Process();
            try {
                updater.StartInfo.UseShellExecute = false;
                updater.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Updater.exe");
                updater.StartInfo.CreateNoWindow = true;
                updater.Start();
                Application.Current.Shutdown();
            } catch (Exception exception) {
                Error(exception);
            }
        }

        public static void Error(Exception exception) {
            var error = exception.Message + "\n" + exception.StackTrace;
            Console.WriteLine(error);
            LogHandler.LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result = MessageBox.Show("Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (result == MessageBoxResult.OK) {
                Application.Current.Shutdown();
            }
        }
    }
}

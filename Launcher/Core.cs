using System;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.Dialog;
using UKSF_Launcher.UI.FTS;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher {
    public class Core {
        public static SettingsHandler SettingsHandler;
        private static ServerHandler _serverHandler;

        /// <summary>
        ///     Application starting point.
        /// </summary>
        /// <param name="updated">Determines if the launcher has been updated</param>
        public Core(bool updated) {
            try {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                StartLogging();
                LogHashSpaceMessage(Severity.INFO, "Launcher Started");

                InitialiseSettings();

                UpdateHandler.UpdateCheck(updated);

                if (!FIRSTTIMESETUPDONE) {
                    LogHashSpaceMessage(Severity.INFO, "Running first time setup");
                    new FtsWindow().ShowDialog();
                }

                LogHashSpace();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Focus();

                _serverHandler = new ServerHandler();
            } catch (Exception exception) {
                Error(exception);
            }
        }

        /// <summary>
        ///     Reads all settings from the registry.
        /// </summary>
        private static void InitialiseSettings() {
            LogHashSpaceMessage(Severity.INFO, "Reading all settings");
            SettingsHandler = new SettingsHandler(REGSITRY);

            // Launcher
            FIRSTTIMESETUPDONE = SettingsHandler.ParseSetting("FIRSTTIMESETUPDONE", false);
            AUTOUPDATELAUNCHER = SettingsHandler.ParseSetting("AUTOUPDATELAUNCHER", true);

            // Game
            GAME_LOCATION = SettingsHandler.ParseSetting("GAME_LOCATION", "");
            MOD_LOCATION = SettingsHandler.ParseSetting("MOD_LOCATION", "");
            PROFILE = SettingsHandler.ParseSetting("PROFILE", "");

            // Startup
            STARTUP_NOSPLASH = SettingsHandler.ParseSetting("STARTUP_NOSPLASH", true);
            STARTUP_SCRIPTERRORS = SettingsHandler.ParseSetting("STARTUP_SCRIPTERRORS", false);
            STARTUP_HUGEPAGES = SettingsHandler.ParseSetting("STARTUP_HUGEPAGES", false);
            STARTUP_MALLOC = SettingsHandler.ParseSetting("STARTUP_MALLOC", MALLOC_SYSTEM_DEFAULT);
            STARTUP_FILEPATCHING = SettingsHandler.ParseSetting("STARTUP_FILEPATCHING", false);

            // Mods
            MODS_SHACKTAC = SettingsHandler.ParseSetting("MODS_SHACKTAC", false);
        }

        // ReSharper disable once UnusedMember.Local
        public static void ResetSettings() {
            SettingsHandler.ResetSettings();
        }

        // ReSharper disable once UnusedMember.Local
        public static void CleanSettings() { }

        /// <summary>
        ///     Shuts the application down.
        ///     If there is no instance of Applicaiton, exit forcefully.
        /// </summary>
        public static void ShutDown() {
            _serverHandler?.Stop();
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
            LogSeverity(Severity.ERROR, error);
            Clipboard.SetDataObject(error, true);
            MessageBoxResult result = DialogWindow.Show("Error", "Something went wrong.\nThe message below has been copied to your clipboard. Please send it to us.\n\n" + error,
                                                        DialogWindow.DialogBoxType.OK);
            if (result == MessageBoxResult.OK) {
                ShutDown();
            }
        }
    }
}
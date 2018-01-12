using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Patching;
using SendGrid;
using SendGrid.Helpers.Mail;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.Dialog;
using UKSF_Launcher.UI.FTS;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher {
    public class Core {
        public static SettingsHandler SettingsHandler;
        public static CancellationTokenSource CancellationTokenSource;

        /// <summary>
        ///     Application starting point.
        /// </summary>
        /// <param name="updated">Determines if the launcher has been updated</param>
        public Core(bool updated) {
            try {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                CancellationTokenSource = new CancellationTokenSource();

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
                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = false});

                BackgroundWorker repoBackgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
                repoBackgroundWorker.DoWork += (sender, args) => ServerHandler.StartServerHandler();
                repoBackgroundWorker.ProgressChanged += (sender, args) =>
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PROGRESS_EVENT) {
                        Value = args.ProgressPercentage,
                        Message = args.UserState.ToString()
                    });

                REPO = new RepoClient(MOD_LOCATION, APPDATA, "uksf", LogInfo, repoBackgroundWorker.ReportProgress);
                REPO.ErrorEvent += (sender, exception) => Error(exception);
                REPO.ErrorNoShutdownEvent += (sender, exception) => ErrorNoShutdown(exception);
                REPO.UploadEvent += (sender, requestTuple) => ServerHandler.SendDeltaRequest(requestTuple.Item1, requestTuple.Item2, requestTuple.Item3, requestTuple.Item4);
                REPO.DeleteEvent += (sender, path) => ServerHandler.SendDeltaDelete(path);
                repoBackgroundWorker.RunWorkerAsync();

                SendReport("test");
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
            CancellationTokenSource.Cancel();
            ServerHandler.Stop();
            if (Application.Current == null) {
                Environment.Exit(0);
            } else {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        ///     Logs an error and displays a dialog with the error message. Shuts program down.
        /// </summary>
        /// <param name="exception">Error exception to report</param>
        public static async void Error(Exception exception) {
            await Application.Current.Dispatcher.InvokeAsync(async () => {
                CancellationTokenSource.Cancel();
                await Task.Delay(250);
                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PROGRESS_EVENT) {
                    Value = 1,
                    Message = "stop"
                });

                string error = exception.Message + "\n" + exception.StackTrace;
                LogSeverity(Severity.ERROR, error);
                SendReport(error);
                MessageBoxResult result = DialogWindow.Show("Error", "Something went wrong.\nPlease create an issue with the below error here: ::\n\n" + error,
                                                            DialogWindow.DialogBoxType.OK, "https://github.com/uksf/launcher-issues/issues/new");
                if (result == MessageBoxResult.OK) {
                    ShutDown();
                }
            });
        }

        /// <summary>
        ///     Logs an error and displays a dialog with the error message.
        /// </summary>
        /// <param name="exception">Error exception to report</param>
        private static void ErrorNoShutdown(Exception exception) {
            Application.Current.Dispatcher.Invoke(() => {
                string error = exception.Message + "\n" + exception.StackTrace;
                LogSeverity(Severity.ERROR, error);
                SendReport(error);
                DialogWindow.Show("Error", "Something went wrong.\nPlease create an issue with the below error here: ::\n\n" + error, DialogWindow.DialogBoxType.OK,
                                  "https://github.com/uksf/launcher-issues/issues/new");
            });
        }

        private static async void SendReport(string message) {
            SendGridClient client = new SendGridClient("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");
            EmailAddress from = new EmailAddress("noreply@uk-sf.com", "NOREPLY");
            EmailAddress to = new EmailAddress("contact.tim.here@gmail.com");
            SendGridMessage email = MailHelper.CreateSingleEmail(from, to, $"Launcher Error - {DateTime.Now}", $"{PROFILE} - {DateTime.Now} - {VERSION}\n\n {message}",
                                                                 $"{PROFILE} - {DateTime.Now} - {VERSION}\n\n {message}");
            email.Attachments = new List<Attachment> {new Attachment {Filename = GetLogFilePath()}};
            await client.SendEmailAsync(email);
        }
    }
}
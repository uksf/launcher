using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF.Launcher.Game;
using UKSF.Launcher.Patching;
using UKSF.Launcher.UI.Dialog;
using UKSF.Launcher.UI.FTS;
using UKSF.Launcher.UI.General;
using UKSF.Launcher.UI.Main;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher {
    public class Core {
        public static CancellationTokenSource CancellationTokenSource;
        public static SettingsHandler SettingsHandler;

        public Core(bool updated) {
            try {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                CancellationTokenSource = new CancellationTokenSource();

                LogHandler.StartLogging();
                LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Launcher Started");

                InitialiseSettings();

                UpdateHandler.UpdateCheck(updated);

                if (!Global.Settings.Firsttimesetupdone) {
                    LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Running first time setup");
                    new FtsWindow().ShowDialog();
                }

                LogHandler.LogHashSpace();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Focus();
                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});

                BackgroundWorker repoBackgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
                repoBackgroundWorker.DoWork += (sender, args) => ServerHandler.StartServerHandler();
                repoBackgroundWorker.ProgressChanged += (sender, args) =>
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {
                        Value = args.ProgressPercentage, Message = args.UserState.ToString()
                    });

                Global.Repo = new RepoClient(Global.Settings.ModLocation, Global.Constants.APPDATA, "uksf", LogHandler.LogInfo, repoBackgroundWorker.ReportProgress);
                Global.Repo.ErrorEvent += (sender, exception) => Error(exception);
                Global.Repo.ErrorNoShutdownEvent += (sender, exception) => ErrorNoShutdown(exception);
                Global.Repo.UploadEvent += (sender, requestTuple) => ServerHandler.SendDeltaRequest(requestTuple.Item1, requestTuple.Item2, requestTuple.Item3, requestTuple.Item4);
                Global.Repo.DeleteEvent += (sender, path) => ServerHandler.SendDeltaDelete(path);
                repoBackgroundWorker.RunWorkerAsync();

                SendReport("test");
            } catch (Exception exception) {
                Error(exception);
            }
        }

        private static void InitialiseSettings() {
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Reading all settings");
            SettingsHandler = new SettingsHandler(Global.Constants.REGSITRY);

            // Launcher
            Global.Settings.Firsttimesetupdone = SettingsHandler.ParseSetting("FIRSTTIMESETUPDONE", false);
            Global.Settings.Autoupdatelauncher = SettingsHandler.ParseSetting("AUTOUPDATELAUNCHER", true);

            // Game
            Global.Settings.GameLocation = SettingsHandler.ParseSetting("GAME_LOCATION", "");
            Global.Settings.ModLocation = SettingsHandler.ParseSetting("MOD_LOCATION", "");
            Global.Settings.Profile = SettingsHandler.ParseSetting("PROFILE", "");

            // Startup
            Global.Settings.StartupNosplash = SettingsHandler.ParseSetting("STARTUP_NOSPLASH", true);
            Global.Settings.StartupScripterrors = SettingsHandler.ParseSetting("STARTUP_SCRIPTERRORS", false);
            Global.Settings.StartupHugepages = SettingsHandler.ParseSetting("STARTUP_HUGEPAGES", false);
            Global.Settings.StartupMalloc = SettingsHandler.ParseSetting("STARTUP_MALLOC", Global.Constants.MALLOC_SYSTEM_DEFAULT);
            Global.Settings.StartupFilepatching = SettingsHandler.ParseSetting("STARTUP_FILEPATCHING", false);
        }

        public static void ResetSettings() {
            SettingsHandler.ResetSettings();
        }

        public static void CleanSettings() { }

        public static void ShutDown() {
            CancellationTokenSource.Cancel();
            ServerHandler.Stop();
            if (Application.Current == null) {
                Environment.Exit(0);
            } else {
                Application.Current.Shutdown();
            }
        }

        public static async void Error(Exception exception) {
            await Application.Current.Dispatcher.InvokeAsync(async () => {
                CancellationTokenSource.Cancel();
                await Task.Delay(250);
                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {Value = 1, Message = "stop"});

                string error = exception.Message + "\n" + exception.StackTrace;
                LogHandler.LogSeverity(Global.Severity.ERROR, error);
                SendReport(error);
                MessageBoxResult result = DialogWindow.Show("Error",
                                                            "Something went wrong.\nPlease create an issue with the below error here: ::\n\n" + error,
                                                            DialogWindow.DialogBoxType.OK,
                                                            "https://github.com/uksf/launcher-issues/issues/new");
                if (result == MessageBoxResult.OK) {
                    ShutDown();
                }
            });
        }

        private static void ErrorNoShutdown(Exception exception) {
            Application.Current.Dispatcher.Invoke(() => {
                string error = exception.Message + "\n" + exception.StackTrace;
                LogHandler.LogSeverity(Global.Severity.ERROR, error);
                SendReport(error);
                DialogWindow.Show("Error",
                                  "Something went wrong.\nPlease create an issue with the below error here: ::\n\n" + error,
                                  DialogWindow.DialogBoxType.OK,
                                  "https://github.com/uksf/launcher-issues/issues/new");
            });
        }

        private static async void SendReport(string message) {
//            SendGridClient client = new SendGridClient("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");
//            EmailAddress from = new EmailAddress("noreply@uk-sf.com", "NOREPLY");
//            EmailAddress to = new EmailAddress("contact.tim.here@gmail.com");
//            SendGridMessage email = MailHelper.CreateSingleEmail(from, to, $"Launcher Error - {DateTime.Now}", $"{PROFILE} - {DateTime.Now} - {VERSION}\n\n {message}",
//                                                                 $"{PROFILE} - {DateTime.Now} - {VERSION}\n\n {message}");
//            email.Attachments = new List<Attachment> {new Attachment {Filename = GetLogFilePath()}};
//            await client.SendEmailAsync(email);
        }
    }
}

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UKSF.Launcher.Network;
using UKSF.Launcher.UI.Dialog;
using UKSF.Launcher.UI.FTS;
using UKSF.Launcher.UI.General;
using UKSF.Launcher.UI.Login;
using UKSF.Launcher.UI.Main;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher {
    public class Core {
        public static CancellationTokenSource CancellationTokenSource;
        public static SettingsHandler SettingsHandler;
        private static bool updated;

        public Core(bool updated) {
            Core.updated = updated;
            try {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                CancellationTokenSource = new CancellationTokenSource();

                LogHandler.StartLogging();
                LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Launcher Started");

                InitialiseSettings();

                Login();
            } catch (Exception exception) {
                Error(exception);
            }
        }

        private static async void Login() {
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Logging in");
            string message = "";
            string storedPassword = SettingsHandler.ParseSetting("LOGINPASSWORD", "");

            if (!string.IsNullOrEmpty(Global.Settings.LoginEmail)) {
                if (!string.IsNullOrEmpty(storedPassword)) {
                    string decryptedPassword = PasswordHandler.DecryptPassword(storedPassword);
                    try {
                        ApiWrapper.Login(Global.Settings.LoginEmail, decryptedPassword).Wait();
                        LogHandler.LogInfo("Logged in");
                        FinishInit();
                        return;
                    } catch (Exception exception) {
                        if (exception.InnerException is LoginFailedException loginFailedException) {
                            LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to login because: {loginFailedException.Reason}");
                            message = loginFailedException.Reason;
                        } else {
                            LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to login for an unknown reason");
                        }
                    }
                }
            }

            await Application.Current.Dispatcher.InvokeAsync(() => {
                LoginWindow.CreateLoginWindow(LoginEvent);
                LoginWindow.UpdateDetails(message, Global.Settings.LoginEmail);
            });
        }

        private static string LoginEvent(Tuple<MessageBoxResult, string, string> result) {
            (MessageBoxResult messageBoxResult, string email, string password) = result;
            if (messageBoxResult == MessageBoxResult.Cancel) {
                ShutDown();
                return null;
            }

            Global.Settings.LoginEmail = (string) SettingsHandler.WriteSetting("LOGINEMAIL", email);
            SettingsHandler.WriteSetting("LOGINPASSWORD", PasswordHandler.EncryptPassword(password));
            Login();

            return null;
        }

        private static void FinishInit() {
            try {
                UpdateHandler.UpdateCheck(updated);

                if (!Global.Settings.Firsttimesetupdone) {
                    LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "Running first time setup");
                    new FtsWindow().ShowDialog();
                }

                LogHandler.LogHashSpace();
                UKSF.Launcher.UI.Main.MainWindow.CreateMainWindow();
                UKSF.Launcher.UI.Main.MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});

//                BackgroundWorker repoBackgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
//                repoBackgroundWorker.DoWork += (sender, args) => ServerHandler.StartServerHandler();
//                repoBackgroundWorker.ProgressChanged += (sender, args) =>
//                    UKSF.Launcher.UI.Main.MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {
//                        Value = args.ProgressPercentage, Message = args.UserState.ToString()
//                    });
//
//                Global.Repo = new RepoClient(Global.Settings.ModLocation, Global.Constants.APPDATA, "uksf", LogHandler.LogInfo, repoBackgroundWorker.ReportProgress);
//                Global.Repo.ErrorEvent += (sender, exception) => Error(exception);
//                Global.Repo.ErrorNoShutdownEvent += (sender, exception) => ErrorNoShutdown(exception);
//                Global.Repo.UploadEvent += (sender, requestTuple) => ServerHandler.SendDeltaRequest(requestTuple.Item1, requestTuple.Item2, requestTuple.Item3, requestTuple.Item4);
//                Global.Repo.DeleteEvent += (sender, path) => ServerHandler.SendDeltaDelete(path);
//                repoBackgroundWorker.RunWorkerAsync();
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
            Global.Settings.LoginEmail = SettingsHandler.ParseSetting("LOGINEMAIL", "");

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

        public static void ShutDown() {
            CancellationTokenSource.Cancel();
            //ServerHandler.Stop();
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
                if (UKSF.Launcher.UI.Main.MainWindow.Instance != null) {
                    UKSF.Launcher.UI.Main.MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ProgressRoutedEventArgs(HomeControl.HOME_CONTROL_PROGRESS_EVENT) {Value = 1, Message = "stop"});
                }

                string error = exception.Message + "\n" + exception.StackTrace;
                LogHandler.LogSeverity(Global.Severity.ERROR, error);
                await SendReport(error);
                MessageBoxResult result = DialogWindow.Show("Error",
                                                            "Something went wrong.\nPlease report the error by creating an issue here: \n::\n\n" + error,
                                                            DialogWindow.DialogBoxType.OK,
                                                            "https://github.com/uksf/launcher-issues/issues/new");
                if (result == MessageBoxResult.OK) {
                    ShutDown();
                }
            });
        }

        private static async void ErrorNoShutdown(Exception exception) {
            await Application.Current.Dispatcher.InvokeAsync(async () => {
                string error = exception.Message + "\n" + exception.StackTrace;
                LogHandler.LogSeverity(Global.Severity.ERROR, error);
                await SendReport(error);
                DialogWindow.Show("Error",
                                  "Something went wrong.\nPlease report the error by creating an issue here: \n::\n\n" + error,
                                  DialogWindow.DialogBoxType.OK,
                                  "https://github.com/uksf/launcher-issues/issues/new");
            });
        }

        private static async Task SendReport(string message) {
            try {
                await ApiWrapper.Post("launcher/error", new {version = Global.Settings.VERSION.ToString(), message});
            } catch (Exception exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, exception.ToString());
            }
        }
    }
}

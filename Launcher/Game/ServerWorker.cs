using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Network;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    internal class ServerWorker {
        private Task _repoCheckTask;
        public ServerWorker(string command, string commandArguments) {
            switch (command) {
                case "servers":
                    Task serversUpdateTask = new Task(() => {
                        List<Server> servers = new List<Server>();
                        if (!string.IsNullOrEmpty(commandArguments)) {
                            string[] serverParts = commandArguments.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                            servers.AddRange(serverParts.Select(Server.DeSerialize));
                        }
                        MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.ServerRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_SERVER_EVENT) {Servers = servers});
                    });
                    serversUpdateTask.Start();
                    break;
                case "repodata":
                    if (_repoCheckTask != null) return;
                    try {
                        _repoCheckTask = new Task(() => {
                            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = false});
                            while (Global.REPO.CheckLocalRepo(commandArguments, ProgressUpdate) && !Core.CancellationTokenSource.IsCancellationRequested) {
                                //Global.REPO.SynchroniseLocalRepo();
                                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) { State = false });
                                break;
                            }
                            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                            _repoCheckTask = null;
                            Core.CancellationTokenSource.Cancel();
                        });
                        _repoCheckTask.Start();
                    } catch (Exception exception) {
                        ServerHandler.ParentWorker.ReportProgress(1, "");
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to process remote repo\n{exception}");
                    }
                    break;
                case "unlock":
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                    break;
            }
        }

        private static void ProgressUpdate(float value, string message) {
            LogHandler.LogInfo(message);
            if (Core.CancellationTokenSource.IsCancellationRequested) return;
            ServerHandler.ParentWorker.ReportProgress((int) (value * 100), message);
        }
    }
}
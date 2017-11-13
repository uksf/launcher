﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Network;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    public static class ServerHandler {
        public static readonly Server NO_SERVER = new Server("No Server", "", 0, "", false);

        private static ServerSocket _serverSocket;
        private static BackgroundWorker _parentWorker;

        private static Task _repoCheckTask;

        public static void StartServerHandler(object sender) {
            _parentWorker = (BackgroundWorker) sender;
            _serverSocket = new ServerSocket();
            _serverSocket.ServerLogEvent += ServerMessageLogCallback;
            _serverSocket.ServerCommandEvent += ServerMessageCallback;
            _serverSocket.ServerConnectedEvent += ServerSocketOnServerConnectedEvent;
            _serverSocket.AutoResetEvent.WaitOne();
        }

        private static void ServerSocketOnServerConnectedEvent(object sender, string unused) {
            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_STATE_EVENT) {Value = 1});
            new Task(async () => await SendDelayedServerMessage("reporequest uksf", 500)).Start();
        }

        private static async Task SendDelayedServerMessage(string message, int delay) {
            await Task.Delay(delay);
            SendServerMessage(message);
        }

        public static void SendServerMessage(string message) {
            _serverSocket.SendMessage(message);
        }

        public static void SendDeltaRequest(string name, string path, string remotePath) {
            _serverSocket.SendMessage(Encoding.ASCII.GetBytes($"deltarequest {name}::{path}::{remotePath}::end"));
        }

        internal static void SendDeltaDelete(string path) {
            _serverSocket.SendMessage(Encoding.ASCII.GetBytes($"deltadelete {path}::end"));
        }

        private static void ServerMessageCallback(object sender, string message) {
            if (message.Contains("message::")) {
                LogHandler.LogInfo(message.Replace("message::", ""));
            } else {
                HandleMessage(message);
            }
        }

        private static void HandleMessage(string message) {
            string[] parts = new Regex("::").Split(message.Replace("command::", ""), 2);
            string commandArguments = parts.Length > 1 ? parts[1] : "";
            if (!message.Contains("command::")) return;
            switch (parts[0]) {
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
                            Core.CancellationTokenSource = new CancellationTokenSource();
                            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = false});
                            while (Global.REPO.CheckLocalRepo(commandArguments, ProgressUpdate, Core.CancellationTokenSource) &&
                                   !Core.CancellationTokenSource.IsCancellationRequested) {
                                MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = false});
                            }
                            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                            MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_STATE_EVENT) { Value = 0 });
                            Core.CancellationTokenSource.Cancel();
                            _repoCheckTask = null;
                        });
                        _repoCheckTask.Start();
                    } catch (Exception exception) {
                        _parentWorker.ReportProgress(1, new Tuple<string, int>("", 1));
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to process remote repo\n{exception}");
                    }
                    break;
                case "deltaresponse":
                    try {
                        Task repoDeltaTask = new Task(() => Global.REPO.ProcessDelta(commandArguments));
                        repoDeltaTask.Start();
                    } catch (Exception exception) {
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to process delta\n{exception}");
                    }
                    break;
                case "unlock":
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) {State = true});
                    break;
                default: return;
            }
        }

        private static void ProgressUpdate(float value, Tuple<string, float> objectState) {
            if (Core.CancellationTokenSource.IsCancellationRequested) return;
            _parentWorker.ReportProgress((int) (value * 100), new Tuple<string, int>(objectState.Item1, (int)(objectState.Item2 * 100)));
        }

        private static void ServerMessageLogCallback(object sender, string message) {
            LogHandler.LogInfo(message);
        }

        public static void Stop() {
            _serverSocket?.StopCheck();
            _serverSocket = null;
        }
    }
}
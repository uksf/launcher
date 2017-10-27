using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Network;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.UI.Main;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    public class ServerHandler {
        public static readonly Server NO_SERVER = new Server("No Server", "", 0, "", false);

        private readonly List<Server> _servers = new List<Server>();

        private readonly ServerSocket _serverSocket;

        public ServerHandler() => _serverSocket = new ServerSocket(ServerMessageCallback, ServerMessageLogCallback);

        public void SendServerMessage(string message) {
            _serverSocket.SendMessage(message);
        }

        private void ServerMessageCallback(string message) {
            if (message.Contains("message::")) {
                LogHandler.LogInfo(message.Replace("message::", ""));
            } else {
                HandleMessage(message);
            }
        }

        private void HandleMessage(string message) {
            LogHandler.LogInfo(message);
            if (!message.Contains("command::")) return;
            string[] parts = new Regex("::").Split(message.Replace("command::", ""), 2);
            switch (parts[0]) {
                case "servers":
                    _servers.Clear();
                    string[] serverParts = parts[1].Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string serverPart in serverParts) {
                        _servers.Add(Server.DeSerialize(serverPart));
                    }
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.ServerRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_SERVER_EVENT) {Servers = _servers});
                    break;
            }
        }

        private static void ServerMessageLogCallback(string message) {
            LogHandler.LogInfo(message);
        }

        public void Stop() {
            _serverSocket.WatchThreadStop();
        }
    }
}
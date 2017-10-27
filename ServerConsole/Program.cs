using System;
using System.Runtime.InteropServices;
using System.Threading;
using Network;

namespace ServerConsole {
    internal static class Program {
        private static ServerSocket _serverSocket;
        private static string _callbackCommand = "";

        private static void Main(string[] args) {
            SetConsoleCtrlHandler(ConsoleControlCheck, true);
            _serverSocket = new ServerSocket(ServerMessageCallback, ServerMessageLogCallback);

            while (_callbackCommand != "connected") {
                Thread.Sleep(1000);
            }

            string argsString = string.Join(" ", args);
            _serverSocket.SendMessage(string.IsNullOrEmpty(argsString) ? "help" : argsString);

            while (_callbackCommand != "stop") {
                Thread.Sleep(1000);
            }
            _serverSocket.WatchThreadStop();
            _serverSocket = null;
        }

        private static void ServerMessageCallback(string message) {
            if (message.Contains("message::")) {
                Console.WriteLine(message.Replace("message::", ""));
            } else {
                _callbackCommand = message.Replace("command::", "");
            }
        }

        private static void ServerMessageLogCallback(string message) {
            Console.WriteLine(message);
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        private static bool ConsoleControlCheck(ControlTypes controlType) {
            switch (controlType) {
                case ControlTypes.CTRL_C_EVENT:
                case ControlTypes.CTRL_BREAK_EVENT:
                case ControlTypes.CTRL_LOGOFF_EVENT:
                case ControlTypes.CTRL_SHUTDOWN_EVENT:
                case ControlTypes.CTRL_CLOSE_EVENT:
                    _serverSocket.WatchThreadStop();
                    _serverSocket = null;
                    Environment.Exit(0);
                    break;
            }
            return true;
        }

        private delegate bool HandlerRoutine(ControlTypes controlType);

        private enum ControlTypes {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
    }
}
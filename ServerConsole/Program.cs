using System;
using System.Runtime.InteropServices;
using Network;

namespace ServerConsole {
    internal static class Program {
        private static string[] _args;
        private static ServerSocket _serverSocket;

        private static void Main(string[] args) {
            SetConsoleCtrlHandler(ConsoleControlCheck, true);
            _args = args;
            _serverSocket = new ServerSocket();
            _serverSocket.ServerLogEvent += ServerMessageLogCallback;
            _serverSocket.ServerCommandEvent += ServerMessageCallback;
            _serverSocket.ServerConnectedEvent += ServerSocketOnServerConnectedEvent;
            _serverSocket.AutoResetEvent.WaitOne();
        }

        private static void ServerSocketOnServerConnectedEvent(object sender, string unused) {
            string argsString = string.Join(" ", _args);
            _serverSocket.SendMessage(string.IsNullOrEmpty(argsString) ? "help" : argsString);
        }

        private static void ServerMessageCallback(object sender, string message) {
            if (message.Contains("command::")) {
                if (!message.Contains("stop")) return;
                _serverSocket.StopCheck();
                _serverSocket = null;
            } else {
                Console.WriteLine(message.Replace("message::", ""));
            }
        }

        private static void ServerMessageLogCallback(object sender, string message) {
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
                    _serverSocket.StopCheck();
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
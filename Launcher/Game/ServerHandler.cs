using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Network;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Game {
    public class ServerHandler {
        public static readonly Server NO_SERVER = new Server("No Server", "", 0, "", false);

        private static ServerSocket _serverSocket;
        public static BackgroundWorker ParentWorker;

        public static void StartServerHandler(object sender) {
            ParentWorker = (BackgroundWorker) sender;
            _serverSocket = new ServerSocket();
            _serverSocket.ServerLogEvent += ServerMessageLogCallback;
            _serverSocket.ServerCommandEvent += ServerMessageCallback;
            _serverSocket.ServerConnectedEvent += ServerSocketOnServerConnectedEvent;
            _serverSocket.AutoResetEvent.WaitOne();
        }

        private static void ServerSocketOnServerConnectedEvent(object sender, string unused) {
            new Task(async () => await SendDelayedServerMessage("reporequest uksf", 500)).Start();
        }

        private static async Task SendDelayedServerMessage(string message, int delay) {
            await Task.Delay(delay);
            SendServerMessage(message);
        }

        private static void SendServerMessage(string message) {
            _serverSocket.SendMessage(message);
        }

        private static void ServerMessageCallback(object sender, string message) {
            if (message.Contains("message::")) {
                LogHandler.LogInfo(message.Replace("message::", ""));
            } else {
                HandleMessage(message);
            }
        }

        private static void HandleMessage(string message) {
            if (!message.Contains("command::")) return;
            string[] parts = new Regex("::").Split(message.Replace("command::", ""), 2);
            ServerWorker unused = new ServerWorker(parts[0], parts.Length > 1 ? parts[1] : "");
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
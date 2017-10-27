using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ServerService {
    public class ServerService : ServiceBase {
        private const int PORT = 48900;

        private const int BUFFER_SIZE = 4096;

        private const int MAX_RECEIVE_ATTEMPT = 10;
        public new static EventLog EventLog;
        private static Socket _serverSocket;
        private static readonly byte[] BUFFER = new byte[BUFFER_SIZE];

        private static int _receiveAttempt;

        private static List<Client> _clients;
        private static Dictionary<Client, Thread> _clientThreads;

        private Container _components;
        private Thread _serverThread, _cleanupThread;
        private bool _stopServer, _stopCleanup;

        public ServerService() {
            InitializeComponent();

            EventLog = new EventLog();
            if (!EventLog.SourceExists("Server Service Source")) {
                EventLog.CreateEventSource("Server Service Source", "Server Service Log");
            }
            EventLog.Source = "Server Service Source";
            EventLog.Log = "Server Service Log";
        }

        private void InitializeComponent() {
            _components = new Container();
            ServiceName = "ServerService";
        }

        protected override void Dispose(bool disposing) {
            OnStop();
            if (disposing) {
                _components?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args) {
            EventLog.WriteEntry("Started");
            //Debugger.Launch();
            _serverThread = new Thread(ServerThreadStart);
            _cleanupThread = new Thread(CleanupThreadStart) {Priority = ThreadPriority.Lowest};
            _serverThread.Start();
            _cleanupThread.Start();
        }

        private void ServerThreadStart() {
            // ReSharper disable once InconsistentlySynchronizedField
            _clients = new List<Client>();
            _clientThreads = new Dictionary<Client, Thread>();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _serverSocket.Listen(100);
            _serverSocket.BeginAccept(AcceptCallback, null);

            while (!_stopServer) {
                Thread.Sleep(5000);
                ServerHandler.CheckServers();
                lock (_clients) {
                    foreach (Client client in _clients) {
                        client.SendCommand(ServerHandler.SERVERS.Where(server => server.Active)
                                                            .Aggregate("servers", (current, server) => string.Join("::", current, $"{server.Serialize()}")));
                    }
                }
            }
        }

        private static void AcceptCallback(IAsyncResult result) {
            try {
                Client client = new Client(_serverSocket.EndAccept(result));
                client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                _serverSocket.BeginAccept(AcceptCallback, null);
                EventLog.WriteEntry($"Client connected {client.Guid}");
            } catch (Exception exception) {
                EventLog.WriteEntry(exception.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult result) {
            try {
                Client client = (Client) result.AsyncState;
                if (!client.Socket.Connected) return;
                int received = client.Socket.EndReceive(result);
                if (received > 0) {
                    _receiveAttempt = 0;
                    byte[] data = new byte[received];
                    Buffer.BlockCopy(BUFFER, 0, data, 0, data.Length);
                    if (!HandleMessage(client, Encoding.UTF8.GetString(data))) return;
                    client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                } else if (_receiveAttempt < MAX_RECEIVE_ATTEMPT) {
                    _receiveAttempt++;
                    client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                } else {
                    EventLog.WriteEntry("Client receive failed");
                    _receiveAttempt = 0;
                }
            } catch (Exception) {
                EventLog.WriteEntry("Client receive failed");
            }
        }

        private static bool HandleMessage(Client client, string message) {
            EventLog.WriteEntry($"Message '{message}' received from {client.Guid}");
            switch (message) {
                case "connect":
                    lock (_clients) {
                        if (!_clients.Contains(client)) {
                            _clients.Add(client);
                        }
                    }
                    break;
                case "disconnect":
                    lock (_clients) {
                        _clients.Remove(client);
                    }
                    return false;
                case "break":
                    _clientThreads.TryGetValue(client, out Thread clientThreadBreak);
                    if (clientThreadBreak?.IsAlive == true) {
                        clientThreadBreak.Abort();
                        _clientThreads[client] = null;
                    }
                    break;
                default:
                    if (_clientThreads.TryGetValue(client, out Thread clientThread)) {
                        if (!clientThread.IsAlive) {
                            _clientThreads[client] = new Thread(() => MessageThreadStart(client, message));
                            _clientThreads[client].Start();
                        }
                    } else {
                        _clientThreads.Add(client, new Thread(() => MessageThreadStart(client, message)));
                        _clientThreads[client].Start();
                    }
                    break;
            }
            return true;
        }

        private static void MessageThreadStart(Client client, string message) {
            new MessageHandler(client, message);
        }

        protected override void OnStop() {
            EventLog.WriteEntry("Stopped");
            _serverSocket?.Dispose();
            _serverSocket?.Close();
            _serverSocket = null;

            _stopServer = true;
            _serverThread.Join(100);
            if (_serverThread.IsAlive) {
                _serverThread.Abort();
            }
            _stopCleanup = true;
            _cleanupThread.Join(100);
            if (_cleanupThread.IsAlive) {
                _cleanupThread.Abort();
            }

            lock (_clients) {
                foreach (Client client in _clients) {
                    client.Socket.Close();
                }
                _clients.Clear();
            }
            _clientThreads.Clear();
        }

        private void CleanupThreadStart() {
            while (!_stopCleanup) {
                List<Client> delete = new List<Client>();
                lock (_clients) {
                    foreach (Client client in _clients.Where(client => !client.Socket.Connected)) {
                        delete.Add(client);
                        client.Socket.Shutdown(SocketShutdown.Both);
                        client.Socket.Close();
                    }
                    foreach (Client client in delete) {
                        EventLog.WriteEntry($"Client disconnected {client.Guid}");
                        _clients.Remove(client);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public class Client {
            public Client(Socket socket) {
                Socket = socket;
                Guid = ((IPEndPoint) Socket.RemoteEndPoint).Address.ToString();
            }

            public Socket Socket { get; }
            public string Guid { get; }

            public void SendCommand(string message) {
                Socket.Send(Encoding.ASCII.GetBytes($"command::{message}::end"));
            }

            public void SendMessage(string message) {
                Socket.Send(Encoding.ASCII.GetBytes($"message::{message}::end"));
            }
        }
    }
}
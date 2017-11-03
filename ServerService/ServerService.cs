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
using System.Threading.Tasks;

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

        private Container _components;
        private Timer _gameServerTimer, _cleanupTimer;

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
            // ReSharper disable once InconsistentlySynchronizedField
            _clients = new List<Client>();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _serverSocket.Listen(100);
            _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
            _gameServerTimer = new Timer(CheckGameServers, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        private static void AcceptCallback(IAsyncResult asyncResult) {
            if (_serverSocket != (Socket) asyncResult.AsyncState) return;
            try {
                Client client = new Client(_serverSocket.EndAccept(asyncResult));
                client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
                EventLog.WriteEntry($"Client connected {client.Guid}");
            } catch (Exception exception) {
                EventLog.WriteEntry(exception.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult asyncResult) {
            try {
                Client client = (Client) asyncResult.AsyncState;
                if (!client.Socket.Connected) return;
                int received = client.Socket.EndReceive(asyncResult);
                if (received > 0) {
                    _receiveAttempt = 0;
                    byte[] data = new byte[received];
                    Buffer.BlockCopy(BUFFER, 0, data, 0, data.Length);
                    string message = Encoding.UTF8.GetString(data);
                    if (message.Contains("::end")) {
                        HandleMessage(client, message.Replace("::end", ""));
                    }
                    client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                } else if (_receiveAttempt < MAX_RECEIVE_ATTEMPT) {
                    _receiveAttempt++;
                    client.Socket.BeginReceive(BUFFER, 0, BUFFER.Length, SocketFlags.None, ReceiveCallback, client);
                } else {
                    _receiveAttempt = 0;
                }
            } catch (Exception) {
                EventLog.WriteEntry("Client receive failed");
            }
        }

        private static void HandleMessage(Client client, string message) {
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
                    EventLog.WriteEntry($"Client disconnected {client.Guid}");
                    client.Socket?.Close();
                    client.Thread?.Abort();
                    lock (_clients) {
                        _clients.Remove(client);
                    }
                    break;
                case "break":
                    client.Thread?.Abort();
                    break;
                default:
                    client.Thread?.Abort();
                    client.Thread = new Thread(() => MessageThreadStart(client, message));
                    client.Thread.Start();
                    break;
            }
        }

        // ReSharper disable once ObjectCreationAsStatement
        private static void MessageThreadStart(Client client, string message) => new MessageHandler(client, message);

        protected override void OnStop() {
            EventLog.WriteEntry("Stopped");
            _serverSocket?.Dispose();
            _serverSocket?.Close();
            _serverSocket = null;

            _gameServerTimer.Dispose();
            _cleanupTimer.Dispose();

            lock (_clients) {
                foreach (Client client in _clients) {
                    client.Socket?.Close();
                    client.Thread?.Abort();
                }
                _clients.Clear();
            }
        }

        private static void CheckGameServers(object unused) {
            // TODO: Add framework for launching servers. Switch to events instead of polling.
            lock (_clients) {
                if (_clients.Count <= 0) return;
                ServerHandler.CheckServers();
                foreach (Client client in _clients) {
                    client.SendCommand(ServerHandler.SERVERS.Where(server => server.Active)
                                                    .Aggregate("servers", (current, server) => string.Join("::", current, $"{server.Serialize()}")));
                }
            }
        }

        private void Cleanup(object unused) {
            try {
                List<Client> delete = new List<Client>();
                lock (_clients) {
                    foreach (Client client in _clients.Where(client => !client.Socket.Connected)) {
                        delete.Add(client);
                        client.Socket?.Shutdown(SocketShutdown.Both);
                        client.Socket?.Close();
                        client.Thread?.Abort();
                    }
                    foreach (Client client in delete) {
                        EventLog.WriteEntry($"Client disconnected {client.Guid}");
                        _clients.Remove(client);
                    }
                }
            } catch {
                // ignored
            }
        }

        public static void RepoUpdated(string message) {
            Task repoUpdatedTask = new Task(() => {
                lock (_clients) {
                    if (_clients.Count <= 0) return;
                    foreach (Client client in _clients) {
                        HandleMessage(client, message);
                    }
                }
            });
            repoUpdatedTask.Start();
        }

        public class Client {
            public Client(Socket socket) {
                Socket = socket;
                Guid = $"{((IPEndPoint) Socket.RemoteEndPoint).Address}_{System.Guid.NewGuid()}";
                Thread = null;
            }

            public Socket Socket { get; }
            public string Guid { get; }
            public Thread Thread { get; set; }

            public void SendCommand(string message) {
                try {
                    EventLog.WriteEntry($"Command '{message}' sent to {Guid}");
                    Socket.Send(Encoding.ASCII.GetBytes($"command::{message}::end"));
                } catch (Exception exception) {
                    EventLog.WriteEntry($"Failed to send '{message}' to {Guid}\n{exception}");
                }
            }

            public void SendMessage(string message) {
                try {
                    Socket.Send(Encoding.ASCII.GetBytes($"message::{message}::end"));
                } catch (Exception exception) {
                    EventLog.WriteEntry($"Failed to send '{message}' to {Guid}\n{exception}");
                }
            }
        }
    }
}
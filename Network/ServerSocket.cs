using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Network {
    public class ServerSocket {
        private const int BUFFER_SIZE = 4096;
        private const int MAX_RECEIVE_ATTEMPT = 10;
        private readonly byte[] _buffer = new byte[BUFFER_SIZE];
        private readonly Timer _serverWatchTimer;
        public readonly AutoResetEvent AutoResetEvent;
        private int _connectAttempts;
        private int _receiveAttempts;
        private readonly object _sendLock = new object();
        private Socket _serverSocket;

        public ServerSocket() {
            AutoResetEvent = new AutoResetEvent(false);
            _serverWatchTimer = new Timer(CheckServer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public event EventHandler<string> ServerConnectedEvent;
        public event EventHandler<string> ServerLogEvent;
        public event EventHandler<string> ServerCommandEvent;

        private void CheckServer(object state) {
            if (_serverSocket?.Connected == true) return;
            if (_connectAttempts == 3) {
                ServerCommandEvent?.Invoke(this, "command::unlock");
            }
            _connectAttempts++;
            if (_connectAttempts <= 5 || _connectAttempts % 30 == 0) {
                ServerLogEvent?.Invoke(this, "Connecting to server...");
            }
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (Connect()) return;
            _serverSocket = null;
            if (_connectAttempts <= 5 || _connectAttempts % 30 == 0) {
                ServerLogEvent?.Invoke(this, "Failed to connect to server");
            }
            if (_connectAttempts == 30) {
                _serverWatchTimer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            }
        }

        public void StopCheck() {
            _serverWatchTimer.Dispose();
            Disconnect();
            AutoResetEvent.Set();
        }

        private bool Connect() {
            try {
                IAsyncResult result = _serverSocket.BeginConnect(Dns.GetHostAddresses("uk-sf.com")[0], 48900, EndConnectCallback, _serverSocket);
                result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(250));
                return true;
            } catch (Exception) {
                //Ignore
            }
            return false;
        }

        private void Disconnect() {
            _connectAttempts = 0;
            try {
                SendMessage("disconnect");
            } catch (Exception) {
                ServerLogEvent?.Invoke(this, "Failed to send disconnect message (hard disconnect)");
            }
            _serverSocket?.Close();
            _serverSocket = null;
            ServerLogEvent?.Invoke(this, "Disconnected from server");
            ServerCommandEvent?.Invoke(this, "disconnected");
        }

        public void SendMessage(string message) {
            lock (_sendLock) {
                _serverSocket.Send(Encoding.ASCII.GetBytes($"{message}::end"));
                ServerLogEvent?.Invoke(this, $"Message sent to server '{message}'");
            }
        }

        public void SendMessage(byte[] bytes) {
            lock (_sendLock) {
                _serverSocket.Send(bytes);
                ServerLogEvent?.Invoke(this, "Upload message sent to server");
            }
        }

        private void EndConnectCallback(IAsyncResult asyncResult) {
            if (_serverSocket != (Socket) asyncResult.AsyncState) return;
            try {
                if (_serverSocket == null) return;
                _serverSocket.EndConnect(asyncResult);
                if (_serverSocket.Connected) {
                    _serverSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, _serverSocket);
                    SendMessage("connect");
                    ServerLogEvent?.Invoke(this, "Connected to server");
                    ServerConnectedEvent?.Invoke(this, "");
                } else {
                    ServerCommandEvent?.Invoke(this, "failconnect");
                }
            } catch (Exception exception) {
                ServerLogEvent?.Invoke(this, exception.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                Socket socket = (Socket) result.AsyncState;
                if (_serverSocket != socket) return;
                if (!socket.Connected) return;
                int received = socket.EndReceive(result);
                if (received > 0) {
                    _receiveAttempts = 0;
                    byte[] data = new byte[received];
                    Buffer.BlockCopy(_buffer, 0, data, 0, data.Length);
                    string fullmessage = Encoding.UTF8.GetString(data);
                    if (fullmessage.Contains("::end")) {
                        string[] messages = fullmessage.Split(new[] {"::end"}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string message in messages) {
                            ServerCommandEvent?.Invoke(this, message);
                        }
                    }
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                } else if (_receiveAttempts < MAX_RECEIVE_ATTEMPT) {
                    _receiveAttempts++;
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                } else {
                    ServerLogEvent?.Invoke(this, "Failed to receive from server");
                    _receiveAttempts = 0;
                    Disconnect();
                }
            } catch (Exception) {
                ServerLogEvent?.Invoke(this, "Failed to receive from server");
                Disconnect();
            }
        }
    }
}
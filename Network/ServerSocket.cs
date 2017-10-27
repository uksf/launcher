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
        private readonly Action<string> _callbackAction, _callbackLogAction;
        private readonly Thread _watchThread;
        private int _receiveAttempts;
        private Socket _serverSocket;
        private bool _stopWatch;

        public ServerSocket(Action<string> callbackAction, Action<string> callbackLogAction) {
            _callbackAction = callbackAction;
            _callbackLogAction = callbackLogAction;
            _watchThread = new Thread(WatchThreadStart);
            _watchThread.Start();
        }

        private void WatchThreadStart() {
            _callbackLogAction.Invoke("Connecting to server...");
            while (!_stopWatch) {
                if (_serverSocket?.Connected != true) {
                    _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    if (Connect()) {
                        try {
                            _serverSocket.Send(Encoding.ASCII.GetBytes("connect"));
                            _callbackLogAction.Invoke("Connected to server");
                            _callbackAction.Invoke("connected");
                        } catch (Exception) {
                            _callbackLogAction.Invoke("Failed to send connect message");
                        }
                    } else {
                        _callbackLogAction.Invoke("Failed to connect to server");
                    }
                }
                Thread.Sleep(3000);
            }
        }

        public void WatchThreadStop() {
            _stopWatch = true;
            _watchThread.Join(100);
            if (_watchThread.IsAlive) {
                _watchThread.Abort();
            }
            Disconnect();
        }

        private bool Connect() {
            int attempts = 0;
            while (!_serverSocket.Connected && attempts < 10) {
                try {
                    attempts++;
                    IAsyncResult result = _serverSocket.BeginConnect(Dns.GetHostAddresses("uk-sf.com")[0], 48900, EndConnectCallback, _serverSocket);
                    result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    break;
                } catch (Exception) {
                    //Ignore
                }
                Thread.Sleep(500);
            }
            return _serverSocket.Connected;
        }

        private void Disconnect() {
            try {
                _serverSocket.Send(Encoding.ASCII.GetBytes("disconnect"));
            } catch (Exception) {
                _callbackLogAction.Invoke("Failed to send disconnect message (hard exit)");
            }
            _serverSocket.Close();
            _serverSocket = null;
            _callbackLogAction.Invoke("Disconnected from server");
            _callbackAction.Invoke("disconnected");
        }

        public void SendMessage(string message) {
            _serverSocket.Send(Encoding.ASCII.GetBytes(message));
            _callbackLogAction.Invoke($"Message sent to server ({message})");
        }

        private void EndConnectCallback(IAsyncResult asyncResult) {
            if (_serverSocket != asyncResult.AsyncState as Socket) return;
            try {
                if (_serverSocket == null) return;
                _serverSocket.EndConnect(asyncResult);
                if (_serverSocket.Connected) {
                    _serverSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, _serverSocket);
                } else {
                    _callbackAction.Invoke("failconnect");
                }
            } catch (Exception exception) {
                _callbackLogAction.Invoke(exception.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                Socket socket = (Socket) result.AsyncState;
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
                            _callbackAction.Invoke(message);
                        }
                    }
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                } else if (_receiveAttempts < MAX_RECEIVE_ATTEMPT) {
                    _receiveAttempts++;
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                } else {
                    _callbackLogAction.Invoke("Failed to receive from server");
                    _callbackAction.Invoke("failreceive");
                    _receiveAttempts = 0;
                }
            } catch (Exception) {
                _callbackLogAction.Invoke("Failed to receive from server");
                _callbackAction.Invoke("failreceive");
            }
        }
    }
}
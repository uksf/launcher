using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UKSF_Launcher.Game {
    public class ServerHandler {
        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly Thread _thread;
        private bool _run;

        public ServerHandler() {
            _thread = new Thread(Start);
            _stopEvent.Reset();
            _thread.Start();
        }

        public List<Server> Servers { get; set; }

        private void Start() {
            Servers = new List<Server> {
                new Server {Active = false, Ip = "uk-sf.com", Name = "Primary Server", Password = "l85", Port = 2303},
                new Server {Active = false, Ip = "uk-sf.com", Name = "Secondary Server", Password = "l85", Port = 2333},
                new Server {Active = false, Ip = "uk-sf.com", Name = "Tertiary Server", Password = "l85", Port = 2343},
                new Server {Active = false, Ip = "uk-sf.com", Name = "Test Server", Password = "hi", Port = 2353}
            };
            _run = true;
            QueryServers();
        }

        public void Stop() {
            _run = false;
            _stopEvent.Set();
            if (!_thread.Join(1000)) {
                _thread.Abort();
            }
        }

        private void QueryServers() {
            while (_run) {
                if (_stopEvent.WaitOne(0)) break;
                foreach (Server server in Servers) {
                    try {
                        using (UdpClient udpClient = new UdpClient(56800)) {
                            IPEndPoint ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(server.Ip)[0], server.Port);
                            udpClient.Client.ReceiveTimeout = 100;
                            udpClient.Connect(ipEndPoint);
                            byte[] request = {
                                0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
                            };
                            udpClient.Send(request, request.Length);
                            udpClient.Receive(ref ipEndPoint);
                            server.Active = true;
                        }
                    } catch (Exception) {
                        server.Active = false;
                    }
                }
                _stopEvent.WaitOne(30000);
            }
        }

        public class Server {
            public string Name { get; set; }
            public string Ip { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
            public bool Active { get; set; }
        }
    }
}
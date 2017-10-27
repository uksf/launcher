using System;

namespace Network {
    [Serializable]
    public class Server {
        public Server(string name, string ip, int port, string password, bool active) {
            Name = name;
            Ip = ip;
            Port = port;
            Password = password;
            Active = active;
        }

        public string Name { get; }
        public string Ip { get; }
        public int Port { get; }
        public string Password { get; }
        public bool Active { get; set; }
        public string Serialize() => $"{Name}:{Ip}:{Port}:{Password}:{Active}";

        public static Server DeSerialize(string serialized) {
            string[] parts = serialized.Split(':');
            return new Server(parts[0], parts[0], Convert.ToInt32(parts[2]), parts[3], Convert.ToBoolean(parts[4]));
        }
    }
}
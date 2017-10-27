﻿using System;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Global

namespace ServerService {
    internal class MessageHandler {
        private const string HELP = "\thelp\n\tcreate [name]\n\tupdate [name]\n";
        private readonly ServerService.Client _client;

        public MessageHandler(ServerService.Client client, string message) {
            _client = client;
            string[] parts = message.Split(' ');
            string command = parts[0];
            string[] parameters = parts.Skip(1).ToArray();
            Tuple<int, string> result = (Tuple<int, string>) GetType().GetMethod(command)?.Invoke(this, new object[] {parameters});
            if (result == null) {
                _client.SendMessage($"\nCommand not found.\n{HELP}");
            } else {
                ServerService.EventLog.WriteEntry($"Handle message result: {result.Item2}");
                switch (result.Item1) {
                    case 0:
                    case 1: // Success/Fail
                        _client.SendMessage(result.Item2);
                        break;
                    case 2: // Param fail
                        _client.SendMessage($"\nIncorrect usage.\n{HELP}");
                        break;
                }
            }
            _client.SendCommand("stop");
        }

        private void Progress(string message) {
            _client.SendMessage(message);
        }

        // help
        public Tuple<int, string> help(string[] parameters) => new Tuple<int, string>(0, $"\nAvailable commands.\n{HELP}");

        // create [name]
        public Tuple<int, string> create(string[] parameters) {
            if (parameters.Length != 1) {
                return new Tuple<int, string>(2, "Failed");
            }
            bool result = RepoHandler.Create(parameters[0], Progress);
            return !result ? new Tuple<int, string>(1, "Failed") : new Tuple<int, string>(0, "Success");
        }

        // update [name]
        public Tuple<int, string> update(string[] parameters) {
            if (parameters.Length != 1) {
                return new Tuple<int, string>(2, "Failed");
            }
            bool result = RepoHandler.Update(parameters[0], Progress);
            return !result ? new Tuple<int, string>(1, "Failed") : new Tuple<int, string>(0, "Success");
        }
    }
}
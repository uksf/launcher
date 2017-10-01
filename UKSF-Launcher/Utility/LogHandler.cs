﻿using System;
using System.IO;
using System.Linq;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    internal static class LogHandler {
        // Logging data format
        private const string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";

        // Logging time format
        private const string FORMAT_TIME = "HH:mm:ss";

        // Logging spacer
        private const string HASHSPACE = "\n#############################################";

        // Logs directory path
        private static readonly string LOGS = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/UKSF-Launcher";

        // Log file
        private static string _logFile;

        /// <summary>
        ///     Starts logging. Creates a new log file, and deletes the oldest if there are more than 9 files.
        /// </summary>
        public static void StartLogging() {
            Console.WriteLine(LOGS);
            Directory.CreateDirectory(LOGS);
            string[] logFiles = new DirectoryInfo(LOGS).GetFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray();
            if (logFiles.Length > 9) {
                Console.WriteLine(LOGS + "/" + logFiles.Last());
                File.Delete(LOGS + "/" + logFiles.Last());
            }
            _logFile = LOGS + "/L__" + DateTime.Now.ToString(FORMAT_DATE) + ".log";
            try {
                File.Create(_logFile).Close();
            } catch (Exception e) {
                Console.WriteLine("Log file not created: " + _logFile + ". " + e.Message);
            }
            LogInfo("Log Created");
        }

        /// <summary>
        ///     Logs a message to the log file.
        /// </summary>
        /// <param name="message">Message to log</param>
        private static void LogToFile(string message) {
            using (StreamWriter writer = new StreamWriter(_logFile, true)) {
                writer.WriteLine(message);
            }
        }

        /// <summary>
        ///     Logs a message to the log file with a timestamp and the severity of the message.
        /// </summary>
        /// <param name="severity">Message severity</param>
        /// <param name="message">Message to log</param>
        public static void LogSeverity(Severity severity, string message) {
            message = DateTime.Now.ToString(FORMAT_TIME) + " " + severity + ": " + message;
            Console.WriteLine(message);
            LogToFile(message);
        }

        /// <summary>
        ///     Logs a message to the log file without a timestamp.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogNoTime(string message) {
            Console.WriteLine(message);
            LogToFile(message);
        }

        /// <summary>
        ///     Logs a spacer formed of hashes to the log file.
        /// </summary>
        public static void LogHashSpace() {
            LogNoTime(HASHSPACE);
        }

        /// <summary>
        ///     Logs a spacer formed of hashes followed by a message with severity to the log file.
        /// </summary>
        /// <param name="severity">Message severity</param>
        /// <param name="message">Message to log</param>
        public static void LogHashSpaceMessage(Severity severity, string message) {
            LogHashSpace();
            LogSeverity(severity, message);
        }

        /// <summary>
        ///     Logs a message to the file with INFO severity.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogInfo(string message) {
            LogSeverity(Severity.INFO, message);
        }

        /// <summary>
        ///     Closes the log file.
        /// </summary>
        public static void CloseLog() {
            LogNoTime(HASHSPACE);
            LogInfo("Closing Log");
        }
    }
}
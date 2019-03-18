using System;
using System.IO;
using System.Linq;

namespace UKSF.Launcher.Utility {
    internal static class LogHandler {
        private static readonly object LOCK_OBJECT = new object();
        private static string logFile;

        public static void StartLogging() {
            Console.WriteLine(Global.Constants.APPDATA);
            Directory.CreateDirectory(Global.Constants.APPDATA);
            string[] logFiles = new DirectoryInfo(Global.Constants.APPDATA)
                                .EnumerateFiles("*.log")
                                .OrderByDescending(file => file.LastWriteTime)
                                .Select(file => file.Name)
                                .ToArray();
            if (logFiles.Length > 9) {
                File.Delete(Path.Combine(Global.Constants.APPDATA, logFiles.Last()));
            }

            lock (LOCK_OBJECT) {
                logFile = Path.Combine(Global.Constants.APPDATA, $"L__{DateTime.Now.ToString(Global.Constants.FORMAT_DATE)}.log");
                try {
                    File.Create(logFile).Close();
                } catch (Exception e) {
                    Console.WriteLine($"Log file not created: {logFile}. {e.Message}");
                }
            }

            LogInfo("Log Created");
        }

        public static string GetLogFilePath() {
            lock (LOCK_OBJECT) {
                return logFile;
            }
        }

        private static void LogToFile(string message) {
            if (logFile == null) return;
            lock (LOCK_OBJECT) {
                using (StreamWriter writer = new StreamWriter(logFile, true)) {
                    writer.WriteLine(message);
                }
            }
        }

        public static void LogSeverity(Global.Severity severity, string message) {
            message = $"{DateTime.Now.ToString(Global.Constants.FORMAT_TIME)} {severity}: {message}";
            Console.WriteLine(message);
            LogToFile(message);
        }

        private static void LogNoTime(string message) {
            Console.WriteLine(message);
            LogToFile(message);
        }

        public static void LogHashSpace() {
            LogNoTime(Global.Constants.HASHSPACE);
        }

        public static void LogHashSpaceMessage(Global.Severity severity, string message) {
            LogHashSpace();
            LogSeverity(severity, message);
        }

        public static void LogInfo(string message) {
            LogSeverity(Global.Severity.INFO, message);
        }

        public static void CloseLog() {
            LogNoTime(Global.Constants.HASHSPACE);
            LogInfo("Closing Log");
        }
    }
}

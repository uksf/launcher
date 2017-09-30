using System;
using System.IO;
using System.Linq;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Utility {
    internal class LogHandler {
        private const string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";
        private const string FORMAT_TIME = "HH:mm:ss";
        private const string HASHSPACE = "\n#############################################";

        private static readonly string LOGS = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/UKSF-Launcher";

        private static string _logFile;

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

        private static void LogToFile(string message) {
            using (StreamWriter writer = new StreamWriter(_logFile, true)) {
                writer.WriteLine(message);
            }
        }

        public static void LogSeverity(Severity severity, string message) {
            message = DateTime.Now.ToString(FORMAT_TIME) + " " + severity + ": " + message;
            Console.WriteLine(message);
            LogToFile(message);
        }

        public static void LogNoTime(string message) {
            Console.WriteLine(message);
            LogToFile(message);
        }

        public static void LogHashSpace() {
            LogNoTime(HASHSPACE);
        }

        public static void LogHashSpaceMessage(Severity severity, string message) {
            LogHashSpace();
            LogSeverity(severity, message);
        }

        public static void LogInfo(string message) {
            LogSeverity(Severity.INFO, message);
        }

        public static void CloseLog() {
            LogNoTime(HASHSPACE);
            LogInfo("Closing Log");
        }
    }
}
using System;
using System.IO;
using System.Linq;

using static UKSF_Launcher.Utility.Info;

namespace UKSF_Launcher.Utility {
    class LogHandler {

        private static string logFile;

        public LogHandler() {
            CreateLogFile();
        }

        private void CreateLogFile() {
            Console.WriteLine(LOGS);
            Directory.CreateDirectory(LOGS);
            string[] logFiles = new DirectoryInfo(LOGS).GetFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray();
            if (logFiles.Length > 9) {
                Console.WriteLine(LOGS + "/" + logFiles.Last());
                File.Delete(LOGS + "/" + logFiles.Last());
            }
            logFile = LOGS + "/L__" + DateTime.Now.ToString(FORMAT_DATE) + ".log";
            try {
                File.Create(logFile).Close();
            } catch (Exception e) {
                Console.WriteLine("Log file not created: " + logFile + ". " + e.Message);
            }
            LogSeverity(Severity.INFO, "Log Created");
        }

        private static void LogToFile(string message) {
            using (StreamWriter writer = new StreamWriter(logFile, true)) {
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

        public static void LogHashSpace(Severity severity, string message) {
            Console.WriteLine(message);
            LogNoTime(HASHSPACE);
            LogSeverity(severity, message);
        }

        public static void CloseLog() {
            LogNoTime(HASHSPACE);
            LogSeverity(Severity.INFO, "Closing Log");
        }
    }
}

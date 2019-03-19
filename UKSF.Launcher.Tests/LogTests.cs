using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.Tests {
    internal class LogTests {
        [Test, Order(1)]
        public void LogTestsNoLogFile() {
            Directory.CreateDirectory(Global.Constants.APPDATA);
            FileInfo[] oldLogFiles = new DirectoryInfo(Global.Constants.APPDATA).EnumerateFiles("*.log").ToArray();
            LogHandler.LogInfo("PRESTARTTEST");
            FileInfo[] logFiles = new DirectoryInfo(Global.Constants.APPDATA).EnumerateFiles("*.log").ToArray();

            Assert.That(logFiles.Length == 0 || oldLogFiles[0] != logFiles[0]);
        }

        [Test, Order(2)]
        public void LogTestsStart() {
            string[] logFiles = new DirectoryInfo(Global.Constants.APPDATA)
                                .EnumerateFiles("*.log")
                                .OrderByDescending(file => file.LastWriteTime)
                                .Select(file => file.Name)
                                .ToArray();

            Assert.DoesNotThrow(LogHandler.StartLogging);
            string[] newLogFiles = new DirectoryInfo(Global.Constants.APPDATA)
                                   .EnumerateFiles("*.log")
                                   .OrderByDescending(file => file.LastWriteTime)
                                   .Select(file => file.Name)
                                   .ToArray();

            Assert.GreaterOrEqual(newLogFiles.Length, logFiles.Length);
        }

        [Test]
        public void LogTestsLogSeverity() {
            string logFile = Path.Combine(Global.Constants.APPDATA,
                                          new DirectoryInfo(Global.Constants.APPDATA)
                                              .EnumerateFiles("*.log")
                                              .OrderByDescending(file => file.LastWriteTime)
                                              .Select(file => file.Name)
                                              .ToArray()
                                              .First());
            LogHandler.LogSeverity(Global.Severity.INFO, "SEVERITYINFOTEST");
            LogHandler.LogSeverity(Global.Severity.WARNING, "SEVERITYWARNINGTEST");
            LogHandler.LogSeverity(Global.Severity.ERROR, "SEVERITYERRORTEST");

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYINFOTEST") && line.Contains((string) Global.Severity.INFO.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYWARNINGTEST") && line.Contains((string) Global.Severity.WARNING.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYERRORTEST") && line.Contains((string) Global.Severity.ERROR.ToString())));
        }

        [Test]
        public void LogTestsLogHashSpace() {
            string logFile = Path.Combine(Global.Constants.APPDATA,
                                          new DirectoryInfo(Global.Constants.APPDATA)
                                              .EnumerateFiles("*.log")
                                              .OrderByDescending(file => file.LastWriteTime)
                                              .Select(file => file.Name)
                                              .ToArray()
                                              .First());
            LogHandler.LogHashSpace();

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains((string) Global.Constants.HASHSPACE.Replace(Environment.NewLine, ""))));
        }

        [Test]
        public void LogTestsLogHashSpaceMessage() {
            string logFile = Path.Combine(Global.Constants.APPDATA,
                                          new DirectoryInfo(Global.Constants.APPDATA)
                                              .EnumerateFiles("*.log")
                                              .OrderByDescending(file => file.LastWriteTime)
                                              .Select(file => file.Name)
                                              .ToArray()
                                              .First());
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "HASHMESSAGETEST");

            string[] lines = File.ReadAllLines(logFile);
            Assert.That(lines.ElementAt(Array.IndexOf(lines, lines.First(line => line.Contains("HASHMESSAGETEST"))) - 1).Contains((string) Global.Constants.HASHSPACE.Replace(Environment.NewLine, "")));
        }

        [Test]
        public void LogTestsLogInfo() {
            string logFile = Path.Combine(Global.Constants.APPDATA,
                                          new DirectoryInfo(Global.Constants.APPDATA)
                                              .EnumerateFiles("*.log")
                                              .OrderByDescending(file => file.LastWriteTime)
                                              .Select(file => file.Name)
                                              .ToArray()
                                              .First());
            LogHandler.LogInfo("SINGLEINFOTEST");

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SINGLEINFOTEST") && line.Contains((string) Global.Severity.INFO.ToString())));
        }

        [Test]
        public void LogTestsClose() {
            string logFile = Path.Combine(Global.Constants.APPDATA,
                                          new DirectoryInfo(Global.Constants.APPDATA)
                                              .EnumerateFiles("*.log")
                                              .OrderByDescending(file => file.LastWriteTime)
                                              .Select(file => file.Name)
                                              .ToArray()
                                              .First());
            LogHandler.CloseLog();

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("Closing Log")));
        }
    }
}

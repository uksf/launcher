using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Tests {
    internal class LogTests {
        [Test, Order(1)]
        public void LogTestsStart() {
            Directory.CreateDirectory(Global.LOGS);
            string[] logFiles = new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray();

            Assert.DoesNotThrow(LogHandler.StartLogging);
            string[] newLogFiles = new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray();

            Assert.GreaterOrEqual(newLogFiles.Length, logFiles.Length);
        }

        [Test]
        public void LogTestsLogSeverity() {
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());
            LogHandler.LogSeverity(Global.Severity.INFO, "SEVERITYINFOTEST");
            LogHandler.LogSeverity(Global.Severity.WARNING, "SEVERITYWARNINGTEST");
            LogHandler.LogSeverity(Global.Severity.ERROR, "SEVERITYERRORTEST");

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYINFOTEST") && line.Contains(Global.Severity.INFO.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYWARNINGTEST") && line.Contains(Global.Severity.WARNING.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SEVERITYERRORTEST") && line.Contains(Global.Severity.ERROR.ToString())));
        }

        [Test]
        public void LogTestsLogHashSpace() {
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());
            LogHandler.LogHashSpace();

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains(Global.HASHSPACE.Replace(Environment.NewLine, ""))));
        }

        [Test]
        public void LogTestsLogHashSpaceMessage() {
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());
            LogHandler.LogHashSpaceMessage(Global.Severity.INFO, "HASHMESSAGETEST");

            string[] lines = File.ReadAllLines(logFile);
            Assert.That(lines.ElementAt(Array.IndexOf(lines, lines.First(line => line.Contains("HASHMESSAGETEST"))) - 1)
                             .Contains(Global.HASHSPACE.Replace(Environment.NewLine, "")));
        }

        [Test]
        public void LogTestsLogInfo() {
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());
            LogHandler.LogInfo("SINGLEINFOTEST");

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("SINGLEINFOTEST") && line.Contains(Global.Severity.INFO.ToString())));
        }

        [Test]
        public void LogTestsClose() {
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());
            LogHandler.CloseLog();

            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("Closing Log")));
        }
    }
}
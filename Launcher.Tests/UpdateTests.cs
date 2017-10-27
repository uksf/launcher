using System.IO;
using System.Linq;
using NUnit.Framework;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.Tests {
    internal class UpdateTests {
        [Test]
        public void UpdateTestsHandleAllFlags() {
            Core.SettingsHandler = new SettingsHandler(@"SOFTWARE\Launcher.Tests");
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());

            string[] newFlags = {"1.0.0.0", Global.UPDATE_FLAG_FORCE, Global.UPDATE_FLAG_RESET, Global.UPDATE_FLAG_CLEAN};
            string[] currentFlags = {"0.0.0.0", "", "", ""};
            bool force = UpdateHandler.HandleFlags(newFlags, currentFlags);

            Assert.True(force);
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("Resetting all settings") && line.Contains(Global.Severity.WARNING.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => line.Contains("Cleaning settings") && line.Contains(Global.Severity.WARNING.ToString())));
        }
        
        [Test]
        public void UpdateTestsHandleNoFlags() {
            Core.SettingsHandler = new SettingsHandler(@"SOFTWARE\Launcher.Tests");
            string logFile = Path.Combine(Global.LOGS,
                                          new DirectoryInfo(Global.LOGS).EnumerateFiles("*.log").OrderByDescending(file => file.LastWriteTime).Select(file => file.Name).ToArray()
                                                                        .First());

            string[] newFlags = { "1.0.0.0", Global.UPDATE_FLAG_FORCE, Global.UPDATE_FLAG_RESET, Global.UPDATE_FLAG_CLEAN };
            string[] currentFlags = { "1.0.0.0", Global.UPDATE_FLAG_FORCE, Global.UPDATE_FLAG_RESET, Global.UPDATE_FLAG_CLEAN };
            bool force = UpdateHandler.HandleFlags(newFlags, currentFlags);

            Assert.False(force);
            Assert.That(File.ReadAllLines(logFile).Any(line => !line.Contains("Resetting all settings") && !line.Contains(Global.Severity.WARNING.ToString())));
            Assert.That(File.ReadAllLines(logFile).Any(line => !line.Contains("Cleaning settings") && !line.Contains(Global.Severity.WARNING.ToString())));
        }
    }
}

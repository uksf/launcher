using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.UI.Main;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.Game {
    internal static class GameHandler {
        /// <summary>
        ///     Finds the game installation from registry and selects the best exe to use. Will return an empty string if no exe or
        ///     installation found.
        /// </summary>
        /// <returns>Path to game exe. Empty string if no installation</returns>
        public static string GetGameInstallation() {
            string location = "";
            RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(GAME_REGISTRY);
            if (gameKey == null) return location;
            string install = gameKey.GetValue("main", "").ToString();
            if (!Directory.Exists(install)) return location;
            string exe = Path.Combine(install, IS64BIT ? "arma3_x64.exe" : "arma3.exe");
            if (File.Exists(exe)) {
                location = exe;
            }
            return location;
        }

        /// <summary>
        ///     Checks the drive space in bytes against the required drive space for the given path.
        /// </summary>
        /// <param name="path">Path to check drive space</param>
        /// <returns>True if enough space on drive</returns>
        public static bool CheckDriveSpace(string path) {
            return DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot(path))).ElementAt(0).AvailableFreeSpace >= REQUIREDSPACE;
        }

        /// <summary>
        ///     Starts the game.
        /// </summary>
        public static void StartGame() {
            if (GAME_PROCESS != null) return;
            new Task(() => {
                LogHashSpaceMessage(Severity.INFO, "Starting game...");
                try {
                    GAME_PROCESS = new Process {StartInfo = {UseShellExecute = false, FileName = GAME_LOCATION, Arguments = GetStartupParameters()}};
                    LogInfo($"Startup Parameters {GAME_PROCESS.StartInfo.Arguments}");
                    ProfileHandler.UpdateProfileSquad(PROFILE);
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_STATE_EVENT) {Value = 2});
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) { State = false });
                    GAME_PROCESS.Start();
                    GAME_PROCESS.WaitForExit();
                } catch (Exception exception) {
                    Core.Error(exception);
                } finally {
                    GAME_PROCESS = null;
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_STATE_EVENT) { Value = 0 });
                    MainWindow.Instance.MainMainControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(MainMainControl.MAIN_MAIN_CONTROL_PLAY_EVENT) { State = true });
                }
            }).Start();
        }

        /// <summary>
        ///     Compiles startup parameter variables into a startup string.
        /// </summary>
        /// <returns>String of parameters</returns>
        public static string GetStartupParameters() {
            StringBuilder startupString = new StringBuilder();
            startupString.Append($"-name={PROFILE}");
            startupString.Append(STARTUP_NOSPLASH ? " -nosplash" : "");
            startupString.Append(STARTUP_SCRIPTERRORS ? " -showScriptErrors" : "");
            startupString.Append(STARTUP_FILEPATCHING ? " -filePatching" : "");
            startupString.Append(STARTUP_HUGEPAGES ? " -hugepages" : "");
            if (STARTUP_MALLOC != MALLOC_SYSTEM_DEFAULT) {
                startupString.Append(GAME_LOCATION.Contains("x64") ? $" -malloc={STARTUP_MALLOC}_x64" : $" -malloc={STARTUP_MALLOC}");
            } else {
                startupString.Append(" -malloc=system");
            }
            if (SERVER != null && SERVER != ServerHandler.NO_SERVER) {
                startupString.Append($" -connect={SERVER?.Ip} -port={SERVER?.Port - 1} -password={SERVER?.Password}");
            }
            StringBuilder modsString = new StringBuilder();
            foreach (string mod in REPO.GetRepoMods()) {
                modsString.Append($"{mod};");
            }
            startupString.Append($" \"-mod={modsString}\"");
            return startupString.ToString();
        }
    }
}
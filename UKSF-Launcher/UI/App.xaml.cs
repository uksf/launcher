using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.UI {
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        /// <summary>
        ///     Entry point for Application. Reads and extracts arguments, and runs program start routine.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Startup arguments</param>
        private void App_Startup(object sender, StartupEventArgs args) {
            using (new Mutex(true, "UKSF-Launcher", out bool newInstance)) {
                if (newInstance) {
                    bool updated = false;
                    for (int i = 0; i != args.Args.Length; ++i) {
                        if (args.Args[i] == "-u") {
                            updated = true;
                        }
                    }
                    // ReSharper disable once ObjectCreationAsStatement
                    new Core(updated);
                } else {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName)) {
                        if (process.Id == current.Id) continue;
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Triggered on Application exit to close the log file.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Shutdown arguments</param>
        private void Application_Exit(object sender, EventArgs args) => CloseLog();
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using UKSF.Launcher.Utility;

namespace UKSF.Launcher.UI {
    public partial class App {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        private void App_Startup(object sender, StartupEventArgs args) {
            using (new Mutex(true, "UKSF Launcher", out bool newInstance)) {
                if (newInstance) {
                    bool updated = false;
                    for (int i = 0; i != args.Args.Length; ++i) {
                        if (args.Args[i] == "-u") {
                            updated = true;
                        }
                    }

                    Core unused = new Core(updated);
                } else {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName).Where(process => process.Id != current.Id)) {
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
            }
        }

        private void Application_Exit(object sender, EventArgs args) => LogHandler.CloseLog();
    }
}

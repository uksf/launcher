using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Updater {
    internal class Program {
        private static void Main() {
            File.Delete(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"));
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/UKSF-Launcher.exe", Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"));
            Process launcher = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    FileName = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"),
                    Arguments = "-u"
                }
            };
            launcher.Start();
            Environment.Exit(0);
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Updater {
    class Program {
        static void Main(string[] args) {
            File.Delete(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"));
            new WebClient().DownloadFile("http://www.uk-sf.com/launcher/release/Updater.exe", Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe"));
            Process launcher = new Process();
            launcher.StartInfo.UseShellExecute = false;
            launcher.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe");
            launcher.StartInfo.Arguments = "-u";
            launcher.Start();
            Environment.Exit(0);
        }
    }
}

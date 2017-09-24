using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Updater {
    class Program {
        static void Main(string[] args) {
            string update = Path.Combine(Environment.CurrentDirectory);
            Console.WriteLine(update);

            string[] oldFiles = Directory.GetFiles(Directory.GetParent(update).FullName);
            foreach (string file in oldFiles) {
                if (!file.Contains("Updater.exe") && !file.Contains("update")) {
                    File.Delete(file);
                }
            }

            string[] newFiles = Directory.EnumerateFiles(update).Where(file => Path.GetFileName(file) != "Updater.exe").ToArray();
            foreach (string file in newFiles) {
                File.Move(file, Path.Combine(Directory.GetParent(update).FullName, Path.GetFileName(file)));
            }
            Process launcher = new Process();
            launcher.StartInfo.UseShellExecute = false;
            launcher.StartInfo.FileName = Path.Combine(Directory.GetParent(update).FullName, "UKSF-Launcher.exe");
            launcher.StartInfo.Arguments = "-u";
            launcher.Start();
            Environment.Exit(0);
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace VersionUpdater {
    class Program {
        static void Main(string[] args) {
            /*ZipFile.ExtractToDirectory(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.zip"), Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher"));
            string newVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher", "UKSF-Launcher.exe")).FileVersion;
            string versionFile = Path.Combine(Environment.CurrentDirectory, "version");
            if (!File.Exists(versionFile)) {
                File.Create(versionFile).Close();
            }
            using (StreamWriter writer = new StreamWriter(versionFile)) {
                writer.Write(newVersion);
            }
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher"), true);*/
            string newVersion = "0.0.0.25";
            Process.Start("git", "clone -q --branch=release git@github.com:uksf/launcher.git").WaitForExit();
            string repository = Path.Combine(Environment.CurrentDirectory, "launcher");
            foreach (string file in Directory.EnumerateFiles(repository, "AssemblyInfo.cs", SearchOption.AllDirectories)) {
                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"(\[assembly: AssemblyVersion\(""[0-9]*.[0-9]*.[0-9]*.[0-9]*""\)\])", "[assembly: AssemblyVersion(\"" + newVersion + "\")]"));
                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"(\[assembly: AssemblyFileVersion\(""[0-9]*.[0-9]*.[0-9]*.[0-9]*""\)\])", "[assembly: AssemblyFileVersion(\"" + newVersion + "\")]"));
            }            
            Process.Start("git", "add -A").WaitForExit();
            Process.Start("git", @"commit -m ""Updated version""").WaitForExit();
            Process.Start("git", "push git@github.com:uksf/launcher").WaitForExit();
        }
    }
}

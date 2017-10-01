using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace VersionUpdater {
    internal static class Program {
        private static void Main() {
            Version newVersion = Version.Parse(FileVersionInfo.GetVersionInfo(Path.Combine(Environment.CurrentDirectory, "UKSF-Launcher.exe")).FileVersion);
            string versionFile = Path.Combine(Environment.CurrentDirectory, "version");
            if (!File.Exists(versionFile)) {
                File.Create(versionFile).Close();
            }
            using (StreamWriter writer = new StreamWriter(versionFile)) {
                writer.Write(newVersion.ToString());
            }

            Process.Start("git", @"clone https://github.com/uksf/launcher.git")?.WaitForExit();
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "launcher"));
            Process.Start("git", @"fetch origin")?.WaitForExit();
            Process.Start("git", @"merge origin/release")?.WaitForExit();
            Process.Start("git", @"push")?.WaitForExit();
            Console.Read();

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories);
            foreach (string file in files) {
                string[] lines = File.ReadAllLines(file);
                for (int index = 0; index < lines.Length; index++) {
                    if (lines[index].Contains("AssemblyVersion")) {
                        lines[index] = "[assembly: AssemblyVersion(\"" + newVersion + "\")]";
                    }
                }
                File.WriteAllLines(file, lines);
            }
            string[] appveyorLines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "appveyor.yml"));
            appveyorLines[0] = "version: \"" + newVersion.Major + "." + newVersion.Minor + "." + newVersion.Build + ".{build}\"";
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, "appveyor.yml"), appveyorLines);
            Console.Read();
            Process.Start("git", @"git commit -am ""Release version: " + newVersion + "\"")?.WaitForExit();
            Console.Read();
            Process.Start("git", @"push")?.WaitForExit();

            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            SetAttributes(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "launcher")));
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "launcher"), true);
        }

        private static void SetAttributes(DirectoryInfo directory) {
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) {
                SetAttributes(subDirectory);
            }
            foreach (FileInfo file in directory.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
        }

        /*private static void ResetAppveyorBuildNumber() {
            HttpWebRequest post = (HttpWebRequest) WebRequest.Create("http://13.93.5.233:8080/load-rsf");
            post.Method = "POST";
            post.ContentType = "application/json";
            post.Headers.Add("Bearer", "Basic " + "<your-api-token>");
            StreamWriter streamWriter = new StreamWriter(post.GetRequestStream());
            streamWriter.Write("{\"nextBuildNumber\": " + newVersion + "}");
            streamWriter.Flush();
            streamWriter.Close();
            post.GetResponse();
        }*/
    }
}
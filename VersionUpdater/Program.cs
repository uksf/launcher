﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
            Thread.Sleep(1000);
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "launcher"));
            Process.Start("git", @"fetch origin")?.WaitForExit();
            Thread.Sleep(1000);
            Process.Start("git", @"merge origin/release")?.WaitForExit();

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

            Thread.Sleep(1000);
            Process.Start("git", @"git commit -am ""Version: " + newVersion + "\"")?.WaitForExit();
            Thread.Sleep(1000);
            Process.Start("git", @"push")?.WaitForExit();
            
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            Thread.Sleep(1000);
            //Directory.Delete(Path.Combine(Environment.CurrentDirectory, "launcher"), true);
        }
    }
}
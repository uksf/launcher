using System;
using System.Diagnostics;
using System.IO;

namespace Releaser {
    internal static class Program {
        private static void Main() {
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            Process.Start("git", @"checkout release")?.WaitForExit();
            Process.Start("git", @"merge master")?.WaitForExit();
            Process.Start("git", @"commit -am ""Release build""")?.WaitForExit();
            Process.Start("git", @"push")?.WaitForExit();
            Process.Start("git", @"checkout master")?.WaitForExit();
        }
    }
}
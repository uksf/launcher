using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Releaser {
    internal static class Program {
        private static void Main() {
            Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, ".."));
            Process.Start("git", @"checkout release")?.WaitForExit();
            Thread.Sleep(500);
            Process.Start("git", @"merge master")?.WaitForExit();
            Thread.Sleep(500);
            Process.Start("git", @"commit -am ""Release build""")?.WaitForExit();
            Thread.Sleep(500);
            Process.Start("git", @"push")?.WaitForExit();
            Thread.Sleep(500);
            Process.Start("git", @"checkout master")?.WaitForExit();
            Thread.Sleep(500);
        }
    }
}
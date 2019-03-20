using System;
using System.Diagnostics;
using UKSF.Launcher.Patching;

namespace Tester {
    class Program {
        static void Main(string[] args) {
            string version = FileVersionInfo.GetVersionInfo(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\release\launcher\netcoreapp3.0\Microsoft.Extensions.FileProviders.Abstractions.dll").FileVersion;
            Console.Out.WriteLine(version);
        }
    }
}

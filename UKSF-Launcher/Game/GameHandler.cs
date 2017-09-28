using Microsoft.Win32;
using System;
using System.Windows;
using UKSF_Launcher.Utility;

using static UKSF_Launcher.Global;

namespace UKSF_Launcher.Game {
    class GameHandler {

        private const string REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";

        public static void CheckGameInstalled() {
            LogHandler.LogHashSpace();
            RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(REGISTRY);
            if (gameKey == null) {
                MessageBoxResult result = Dialog_Window.Show("Arma 3 Not Installed", "We can't find your installation of Arma 3.\nEnsure it is marked as installed in steam.\n\nIf Arma 3 is installed, press 'Ok' and locate the installation folder in the explorer that appears.", Dialog_Window.DialogBoxType.OKCancel);
                if (result == MessageBoxResult.OK) {
                    using (System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog()) {
                        if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                            GAME_LOCATION = folderBrowser.SelectedPath;
                        } else {
                            Application.Current.Shutdown();
                        }
                    }
                } else {
                    Application.Current.Shutdown();
                }
            } else {
                GAME_LOCATION = gameKey.GetValue("main", "").ToString();
            }
            LogHandler.LogInfo("Arma 3 location: " + GAME_LOCATION);
        }
    }
}

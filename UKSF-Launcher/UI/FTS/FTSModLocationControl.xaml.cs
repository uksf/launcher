﻿using System.IO;
using System.Windows;
using System.Windows.Forms;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FTS_ModLocationControl.xaml
    /// </summary>
    public partial class FtsModLocationControl {
        private const string TITLE = "Mod Location";

        private static readonly string DESCRIPTION = "We have selected your Arma 3 install location as your mod download location." + Global.NL +
                                                     "If you wish to change this, select the folder below.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + Global.NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + Global.NL +
                                                               "You can continue by selecting the mod download location you wish to use manually. (Not recommended)";

        private static FtsModLocationControl _instance;
        private string _location = "";

        public FtsModLocationControl() {
            InitializeComponent();
            _instance = this;
        }

        public static void Show() {
            _instance.Visibility = Visibility.Visible;
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(_instance._location)) {
                _instance._location = Path.GetDirectoryName(GameHandler.GetGameInstallation());
                if (!string.IsNullOrEmpty(_instance._location)) {
                    LogHandler.LogInfo("Using Arma 3 location: " + _instance._location);
                    _instance.FtsModLocationControlTextBoxLocation.Text = _instance._location;
                }
            }
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = _instance._location != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            _instance.UpdateWarning();
        }

        public static void Hide() => _instance.Visibility = Visibility.Collapsed;

        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            Visibility visibility = Visibility.Hidden;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(_location)) {
                visibility = Visibility.Visible;
                warning = "Please select a mod download location";
                block = true;
            } else if (!GameHandler.CheckDriveSpace(_location)) {
                visibility = Visibility.Visible;
                warning = "Not enough drive space";
                block = true;
            }
            RaiseEvent(new FtsMainControl.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {
                Visibility = visibility,
                Warning = warning,
                Block = block
            });
        }

        private void FTSModLocationControlButtonBrowse_Click(object sender, RoutedEventArgs e) {
            _location = FtsModLocationControlTextBoxLocation.Text;
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog()) {
                if (_location != "") {
                    folderBrowser.SelectedPath = _location;
                }
                if (folderBrowser.ShowDialog() == DialogResult.OK) {
                    _location = folderBrowser.SelectedPath;
                    FtsModLocationControlTextBoxLocation.Text = _location;
                }
            }
            UpdateWarning();
        }
    }
}
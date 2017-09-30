using System.IO;
using System.Windows;
using System.Windows.Forms;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FTS_GameExeControl.xaml
    /// </summary>
    public partial class FtsGameExeControl {
        private const string TITLE = "Game Executable";

        private static readonly string DESCRIPTION = "We found your Arma 3 installation and chose the best exe for you." + Global.NL +
                                                     "If you are not happy with this, select the Arma 3 exe you wish to use.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + Global.NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + Global.NL +
                                                               "You can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)";

        private static FtsGameExeControl _instance;
        private string _location = "";

        public FtsGameExeControl() {
            InitializeComponent();
            _instance = this;
        }

        public static void Show() {
            _instance.Visibility = Visibility.Visible;
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(_instance._location)) {
                _instance._location = GameHandler.GetGameInstallation();
                if (!string.IsNullOrEmpty(_instance._location)) {
                    LogHandler.LogInfo("Using Arma 3 location: " + _instance._location);
                    _instance.FtsGameExeControlTextBoxLocation.Text = _instance._location;
                }
            }
            _instance.RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = _instance._location != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            _instance.UpdateWarning();
        }

        public static void Hide() {
            _instance.Visibility = Visibility.Collapsed;
        }

        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            Visibility visibility = Visibility.Hidden;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(_location)) {
                visibility = Visibility.Visible;
                warning = "Please select an Arma 3 exe";
                block = true;
            } else if (Path.GetExtension(_location) != ".exe") {
                visibility = Visibility.Visible;
                warning = "File is not an exe";
                block = true;
            } else if (!Path.GetFileNameWithoutExtension(_location).ToLower().Contains("arma3")) {
                visibility = Visibility.Visible;
                warning = "File is not an Arma 3 exe";
                block = true;
            } else if (Global.IS64BIT && !Path.GetFileNameWithoutExtension(_location).ToLower().Contains("x64")) {
                visibility = Visibility.Visible;
                warning = "We recommend using the 'arma3__x64' exe";
            }
            RaiseEvent(new FtsMainControl.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {
                Visibility = visibility,
                Warning = warning,
                Block = block
            });
        }

        private void FTSGameExeControlButtonBrowse_Click(object sender, RoutedEventArgs e) {
            _location = FtsGameExeControlTextBoxLocation.Text;
            using (OpenFileDialog fileBrowser = new OpenFileDialog()) {
                if (_location != "") {
                    fileBrowser.InitialDirectory = _location;
                }
                fileBrowser.Filter = "exe files|*.exe";
                if (fileBrowser.ShowDialog() == DialogResult.OK) {
                    _location = fileBrowser.FileName;
                    FtsGameExeControlTextBoxLocation.Text = _location;
                }
            }
            UpdateWarning();
        }
    }
}
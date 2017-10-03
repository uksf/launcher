using System.IO;
using System.Windows;
using System.Windows.Forms;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsGameExeControl.xaml
    /// </summary>
    public partial class FtsGameExeControl {
        private const string TITLE = "Game Executable";

        private static readonly string DESCRIPTION = "We found your Arma 3 installation and chose the best exe for you." + Global.NL +
                                                     "If you are not happy with this, select the Arma 3 exe you wish to use.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + Global.NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + Global.NL +
                                                               "You can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)";

        private string _location = "";

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsGameExeControl object.
        /// </summary>
        public FtsGameExeControl() {
            InitializeComponent();
        }

        /// <summary>
        ///     Shows the game exe location control. Adds profiles to the selection.
        /// </summary>
        public void Show() {
            Visibility = Visibility.Visible;
            RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(_location)) {
                _location = GameHandler.GetGameInstallation();
                if (!string.IsNullOrEmpty(_location)) {
                    LogHandler.LogInfo("Using Arma 3 location: " + _location);
                    FtsGameExeControlTextBoxLocation.Text = _location;
                }
            }
            RaiseEvent(new FtsMainControl.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = _location != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            UpdateWarning();
        }

        /// <summary>
        ///     Hides the game exe location control.
        /// </summary>
        public void Hide() {
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Checks if a warning needs to be displayed and raises a warning event.
        /// </summary>
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

        /// <summary>
        ///     Triggered when browse button is clicked. Opens file browser dialog and updates game exe location.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSGameExeControlButtonBrowse_Click(object sender, RoutedEventArgs args) {
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
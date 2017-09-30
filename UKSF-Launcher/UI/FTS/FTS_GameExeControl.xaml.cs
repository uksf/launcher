using System.IO;
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_GameExeControl.xaml
    /// </summary>
    public partial class FTS_GameExeControl : UserControl {

        private const string TITLE = "Game Executable";
        private static readonly string DESCRIPTION =
            "We found your Arma 3 installation and chose the best exe for you." + Global.NL +
            "If you are not happy with this, select the Arma 3 exe you wish to use.";
        private static readonly string DESCRIPTION_NOINSTALL =
            "We can't find your Arma 3 installation." + Global.NL +
            "This is unusual, so you should check the game is installed in Steam." + Global.NL +
            "You can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)";

        private static FTS_GameExeControl instance;
        private string location = "";

        public FTS_GameExeControl() {
            InitializeComponent();
            instance = this;
        }

        public static void Show() {
            instance.Visibility = Visibility.Visible;
            instance.RaiseEvent(new FTS_MainControl.StringRoutedEventArgs(FTS_MainControl.FTS_MainControl_Title_Event) { Text = TITLE });
            if (string.IsNullOrEmpty(instance.location)) {
                instance.location = GameHandler.GetGameInstallation();
                if (!string.IsNullOrEmpty(instance.location)) {
                    LogHandler.LogInfo("Using Arma 3 location: " + instance.location);
                    instance.FTS_GameExeControl_TextBoxLocation.Text = instance.location;
                }
            }
            instance.RaiseEvent(new FTS_MainControl.StringRoutedEventArgs(FTS_MainControl.FTS_MainControl_Description_Event) {
                Text = instance.location != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            instance.UpdateWarning();
        }

        public static void Hide() {
            instance.Visibility = Visibility.Collapsed;
        }

        private void UpdateWarning() {
            if (Visibility == Visibility.Visible) {
                Visibility visibility = Visibility.Hidden;
                string warning = "";
                bool block = false;
                if (string.IsNullOrEmpty(location)) {
                    visibility = Visibility.Visible;
                    warning = "Please select an Arma 3 exe";
                    block = true;
                }
                else if (Path.GetExtension(location) != ".exe") {
                    visibility = Visibility.Visible;
                    warning = "File is not an exe";
                    block = true;
                }
                else if (!Path.GetFileNameWithoutExtension(location).ToLower().Contains("arma3")) {
                    visibility = Visibility.Visible;
                    warning = "File is not an Arma 3 exe";
                    block = true;
                }
                else if (Global.IS64BIT && !Path.GetFileNameWithoutExtension(location).ToLower().Contains("x64")) {
                    visibility = Visibility.Visible;
                    warning = "We recommend using the 'arma3__x64' exe";
                }
                RaiseEvent(new FTS_MainControl.WarningRoutedEventArgs(FTS_MainControl.FTS_MainControl_Warning_Event) {
                    Visibility = visibility,
                    Warning = warning,
                    Block = block
                });
            }
        }

        private void FTS_GameExeControl_ButtonBrowse_Click(object sender, RoutedEventArgs e) {
            location = FTS_GameExeControl_TextBoxLocation.Text;
            using (System.Windows.Forms.OpenFileDialog fileBrowser = new System.Windows.Forms.OpenFileDialog()) {
                if (location != "") {
                    fileBrowser.InitialDirectory = location;
                }
                fileBrowser.Filter = "exe files|*.exe";
                if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    location = fileBrowser.FileName;
                    FTS_GameExeControl_TextBoxLocation.Text = location;
                }
            }
            UpdateWarning();
        }
    }
}

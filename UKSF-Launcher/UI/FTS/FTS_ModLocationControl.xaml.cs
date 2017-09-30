using System.IO;
using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_ModLocationControl.xaml
    /// </summary>
    public partial class FTS_ModLocationControl : UserControl {

        private const string TITLE = "Mod Location";
        private static readonly string DESCRIPTION =
            "We have selected your Arma 3 install location as your mod download location." + Global.NL +
            "If you wish to change this, select the folder below.";
        private static readonly string DESCRIPTION_NOINSTALL =
            "We can't find your Arma 3 installation." + Global.NL +
            "This is unusual, so you should check the game is installed in Steam." + Global.NL +
            "You can continue by selecting the mod download location you wish to use manually. (Not recommended)";

        private static FTS_ModLocationControl instance;
        private string location = "";

        public FTS_ModLocationControl() {
            InitializeComponent();
            instance = this;
        }

        public static void Show() {
            instance.Visibility = Visibility.Visible;
            instance.RaiseEvent(new FTS_MainControl.StringRoutedEventArgs(FTS_MainControl.FTS_MainControl_Title_Event) { Text = TITLE });
            if (string.IsNullOrEmpty(instance.location)) {
                instance.location = Path.GetDirectoryName(GameHandler.GetGameInstallation());
                if (!string.IsNullOrEmpty(instance.location)) {
                    LogHandler.LogInfo("Using Arma 3 location: " + instance.location);
                    instance.FTS_ModLocationControl_TextBoxLocation.Text = instance.location;
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
                    warning = "Please select a mod download location";
                    block = true;
                }
                else if (!GameHandler.CheckDriveSpace(location)) {
                    visibility = Visibility.Visible;
                    warning = "Not enough drive space";
                    block = true;
                }
                RaiseEvent(new FTS_MainControl.WarningRoutedEventArgs(FTS_MainControl.FTS_MainControl_Warning_Event) {
                    Visibility = visibility,
                    Warning = warning,
                    Block = block
                });
            }
        }

        private void FTS_ModLocationControl_ButtonBrowse_Click(object sender, RoutedEventArgs e) {
            location = FTS_ModLocationControl_TextBoxLocation.Text;
            using (System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog()) {
                if (location != "") {
                    folderBrowser.SelectedPath = location;
                }
                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    location = folderBrowser.SelectedPath;
                    FTS_ModLocationControl_TextBoxLocation.Text = location;
                }
            }
            UpdateWarning();
        }
    }
}

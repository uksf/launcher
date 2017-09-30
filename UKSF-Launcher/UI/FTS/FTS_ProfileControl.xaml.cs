using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_ProfileControl.xaml
    /// </summary>
    public partial class FTS_ProfileControl : UserControl {

        private const string TITLE = "Game Profile";
        private static readonly string DESCRIPTION =
            "We have selected the Arma 3 profile we think you use for UKSF." + Global.NL +
            "If this is incorrect, select the profile you wish to use from the list below.";
        private static readonly string DESCRIPTION_NOPROFILE =
            "We can't find an Arma 3 profile suitable for UKSF." + Global.NL +
            "If you would like to copy your default profile to preserve your settings,." + Global.NL +
            "Alternatively you can select a profile from the list below. (Not recommended)";

        private static FTS_ProfileControl instance;
        private string profile = "";

        public FTS_ProfileControl() {
            InitializeComponent();
            instance = this;
        }

        public static void Show() {
            instance.Visibility = Visibility.Visible;
            instance.RaiseEvent(new FTS_MainControl.StringRoutedEventArgs(FTS_MainControl.FTS_MainControl_Title_Event) { Text = TITLE });
            
            instance.RaiseEvent(new FTS_MainControl.StringRoutedEventArgs(FTS_MainControl.FTS_MainControl_Description_Event) {
                Text = instance.profile != "" ? DESCRIPTION : DESCRIPTION_NOPROFILE
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
                if (string.IsNullOrEmpty(profile)) {
                    visibility = Visibility.Visible;
                    warning = "Please select a profile";
                    block = true;
                }
                RaiseEvent(new FTS_MainControl.WarningRoutedEventArgs(FTS_MainControl.FTS_MainControl_Warning_Event) {
                    Visibility = visibility,
                    Warning = warning,
                    Block = block
                });
            }
        }

        /*private void FTS_ProfileControl_ButtonBrowse_Click(object sender, RoutedEventArgs e) {
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
        }*/
    }
}

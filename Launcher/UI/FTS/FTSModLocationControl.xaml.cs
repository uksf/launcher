using System.IO;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.General;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsModLocationControl.xaml
    /// </summary>
    public partial class FtsModLocationControl {
        private const string TITLE = "Mod Location";

        private static readonly string DESCRIPTION = "We have selected your Arma 3 install location as your mod download location." + NL +
                                                     "If you wish to change this, select the folder below.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + NL +
                                                               "You can continue by selecting the mod download location you wish to use manually. (Not recommended)";

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsModLocationControl object.
        /// </summary>
        public FtsModLocationControl() {
            InitializeComponent();

            FtsModLocationControlLocationTextboxControl.Directory = true;

            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsMainControlLocation_Update));
        }

        /// <summary>
        ///     Shows the mod location control. Adds profiles to the selection.
        /// </summary>
        public void Show() {
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                string path = Path.GetDirectoryName(GameHandler.GetGameInstallation());
                if (!string.IsNullOrEmpty(path)) {
                    FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text = path;
                    LogInfo("Using Arma 3 location: " + FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
                }
            }
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            UpdateWarning();
        }

        /// <summary>
        ///     Hides the mod location control.
        /// </summary>
        public void Hide() => Visibility = Visibility.Collapsed;

        /// <summary>
        ///     Checks if a warning needs to be displayed and raises a warning event.
        /// </summary>
        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select a mod download location";
                block = true;
            } else if (!GameHandler.CheckDriveSpace(FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Not enough drive space";
                block = true;
            }
            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        /// <summary>
        ///     Triggered by eventhanlder. Updates warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void FtsMainControlLocation_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
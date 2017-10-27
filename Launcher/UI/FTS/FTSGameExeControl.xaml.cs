using System.IO;
using System.Windows;
using UKSF_Launcher.Game;
using UKSF_Launcher.UI.General;
using static UKSF_Launcher.Global;
using static UKSF_Launcher.Utility.LogHandler;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsGameExeControl.xaml
    /// </summary>
    public partial class FtsGameExeControl {
        private const string TITLE = "Game Executable";

        private static readonly string DESCRIPTION = "We found your Arma 3 installation and chose the best exe for you." + NL +
                                                     "If you are not happy with this, select the Arma 3 exe you wish to use.";

        private static readonly string DESCRIPTION_NOINSTALL = "We can't find your Arma 3 installation." + NL +
                                                               "This is unusual, so you should check the game is installed in Steam." + NL +
                                                               "You can continue by selecting the Arma 3 exe you wish to use manually. (Not recommended)";

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsGameExeControl object.
        /// </summary>
        public FtsGameExeControl() {
            InitializeComponent();

            FtsGameExeControlLocationTextboxControl.Filter = "exe files|*.exe";

            AddHandler(LocationTextboxControl.LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT, new RoutedEventHandler(FtsGameExeControlLocation_Update));
        }

        /// <summary>
        ///     Shows the game exe location control. Adds profiles to the selection.
        /// </summary>
        public void Show() {
            Visibility = Visibility.Visible;
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_TITLE_EVENT) {Text = TITLE});
            if (string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text = GameHandler.GetGameInstallation();
                if (!string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                    LogInfo("Using Arma 3 location: " + FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
                }
            }
            RaiseEvent(new SafeWindow.StringRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_DESCRIPTION_EVENT) {
                Text = FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text != "" ? DESCRIPTION : DESCRIPTION_NOINSTALL
            });
            UpdateWarning();
        }

        /// <summary>
        ///     Hides the game exe location control.
        /// </summary>
        public void Hide() => Visibility = Visibility.Collapsed;

        /// <summary>
        ///     Checks if a warning needs to be displayed and raises a warning event.
        /// </summary>
        private void UpdateWarning() {
            if (Visibility != Visibility.Visible) return;
            string warning = "";
            bool block = false;
            if (string.IsNullOrEmpty(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text)) {
                warning = "Please select an Arma 3 exe";
                block = true;
            } else if (Path.GetExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text) != ".exe") {
                warning = "File is not an exe";
                block = true;
            } else if (!Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("arma3")) {
                warning = "File is not an Arma 3 exe";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("battleye")) {
                warning = "Exe cannot be battleye";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("launcher")) {
                warning = "Exe cannot be launcher";
                block = true;
            } else if (Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower().Contains("server")) {
                warning = "Exe cannot be server";
                block = true;
            } else if (IS64BIT && !Path.GetFileNameWithoutExtension(FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text).ToLower()
                                              .Contains("x64")) {
                warning = "We recommend using the 'arma3_x64' exe";
            }
            RaiseEvent(new SafeWindow.WarningRoutedEventArgs(FtsMainControl.FTS_MAIN_CONTROL_WARNING_EVENT) {Warning = warning, Block = block});
        }

        /// <summary>
        ///     Triggered by eventhanlder. Updates warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Selected arguments</param>
        private void FtsGameExeControlLocation_Update(object sender, RoutedEventArgs args) {
            UpdateWarning();
        }
    }
}
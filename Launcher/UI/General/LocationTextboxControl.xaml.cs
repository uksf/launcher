using System.Windows;
using System.Windows.Forms;

namespace UKSF_Launcher.UI.General {
    /// <summary>
    ///     Interaction logic for LocationTextboxControl.xaml
    /// </summary>
    public partial class LocationTextboxControl {
        public static readonly RoutedEvent LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT =
            EventManager.RegisterRoutedEvent("LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LocationTextboxControl));

        public LocationTextboxControl() {
            InitializeComponent();
        }

        public bool Directory { private get; set; }
        public string Filter { private get; set; }

        /// <summary>
        ///     Triggered when browse button is clicked. Opens file or folder browser dialog and updates location.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void LocationTextboxControlButtonBrowse_Click(object sender, RoutedEventArgs args) {
            if (Directory) {
                using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog()) {
                    if (LocationTextboxControlTextBoxLocation.Text != "") {
                        folderBrowser.SelectedPath = LocationTextboxControlTextBoxLocation.Text;
                    }
                    if (folderBrowser.ShowDialog() != DialogResult.OK) return;
                    LocationTextboxControlTextBoxLocation.Text = folderBrowser.SelectedPath;
                    RaiseEvent(new RoutedEventArgs(LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT));
                }
            } else {
                using (OpenFileDialog fileBrowser = new OpenFileDialog()) {
                    if (LocationTextboxControlTextBoxLocation.Text != "") {
                        fileBrowser.InitialDirectory = LocationTextboxControlTextBoxLocation.Text;
                    }
                    fileBrowser.Filter = Filter;
                    if (fileBrowser.ShowDialog() != DialogResult.OK) return;
                    LocationTextboxControlTextBoxLocation.Text = fileBrowser.FileName;
                    RaiseEvent(new RoutedEventArgs(LOCATION_TEXTBOX_CONTROL_UPDATE_EVENT));
                }
            }
        }
    }
}
using System.Windows;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainMainControl.xaml
    /// </summary>
    public partial class MainMainControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new MainMainControl object.
        /// </summary>
        public MainMainControl() {
            InitializeComponent();
            
            // TODO: Add event for blocking play button if blocking warning present
        }

        /// <summary>
        ///     Triggered when play button is clicked. Does nothing.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void MainMainControlButtonPlay_Click(object sender, RoutedEventArgs args) {
            GameHandler.StartGame();
        }
    }
}
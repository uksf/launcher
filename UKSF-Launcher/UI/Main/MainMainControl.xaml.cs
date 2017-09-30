using System;
using System.Windows;

namespace UKSF_Launcher.UI.Main {
    /// <summary>
    ///     Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainMainControl {
        public MainMainControl() {
            InitializeComponent();
        }

        private void MainMainControlButtonPlay_Click(object sender, RoutedEventArgs args) {
            Console.WriteLine("PRESSED");
        }
    }
}
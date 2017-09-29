using System;
using System.Windows;
using System.Windows.Controls;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class Main_MainControl : UserControl {
        public Main_MainControl() {
            InitializeComponent();
        }

        private void Main_MainControl_ButtonPlay_Click(object sender, RoutedEventArgs args) {
            Console.WriteLine("PRESSED");
        }
    }
}

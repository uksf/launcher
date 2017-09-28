using System;
using System.Windows;
using System.Windows.Input;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_Window.xaml
    /// </summary>
    public partial class FTS_Window : Window {

        public FTS_Window() {
            InitializeComponent();
            AddHandler(FTS_TitleBarControl.FTS_TitleBarControl_MouseDown_Event, new RoutedEventHandler(FTS_TitleBar_MouseDown));
            //AddHandler(DialogMainControl.DialogButtonOKClickEvent, new RoutedEventHandler(ButtonOK_Click));
            //AddHandler(DialogMainControl.DialogButtonCancelClickEvent, new RoutedEventHandler(ButtonCancel_Click));
        }

        private void FTS_TitleBar_MouseDown(object sender, RoutedEventArgs args) {
            DragMove();
        }

        private void FTS_ButtonNext_Click(object sender, RoutedEventArgs args) {

        }

        private void FTS_ButtonCancel_Click(object sender, RoutedEventArgs args) {
            Application.Current.Shutdown();
        }
    }
}

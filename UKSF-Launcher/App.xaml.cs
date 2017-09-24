using System;
using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application {

        private void Application_Exit(object sender, EventArgs args) {
            Utility.LogHandler.CloseLog();
        }
    }
}

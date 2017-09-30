using System.Windows;

namespace UKSF_Launcher.UI {
    public class SafeWindow : Window {
        public SafeWindow() {
            if (Application.Current == null) {
                Core.ShutDown();
            }
        }
    }
}
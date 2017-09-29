using System.Windows;

namespace UKSF_Launcher {
    public class SafeWindow : Window {

        public SafeWindow() {
            if (Application.Current == null) {
                Core.ShutDown();
            }
        }
    }
}

using System.Windows;

namespace UKSF_Launcher.UI {
    public class SafeWindow : Window {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new Window if current Application still exists, otherwise forces program exit.
        /// </summary>
        protected SafeWindow() {
            if (Application.Current == null) {
                Core.ShutDown();
            }
        }
    }
}
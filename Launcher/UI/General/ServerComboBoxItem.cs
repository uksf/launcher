using System.Windows;
using System.Windows.Controls;
using Network;

namespace UKSF_Launcher.UI.General {
    public class ServerComboBoxItem : ComboBoxItem {
        /// <inheritdoc />
        /// <summary>
        ///     Creates ComboBoxItem with given Malloc object and style.
        /// </summary>
        /// <param name="server">Server to assign ComboBoxItem to</param>
        /// <param name="style">Style to set ComboBoxItem to</param>
        public ServerComboBoxItem(Server server, Style style) {
            Server = server;
            Style = style;
            Content = Server.Name;
        }

        public Server Server { get; }
    }
}
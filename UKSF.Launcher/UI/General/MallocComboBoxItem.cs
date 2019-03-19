using System.Windows;
using System.Windows.Controls;
using UKSF.Launcher.Game;

namespace UKSF.Launcher.UI.General {
    public class MallocComboBoxItem : ComboBoxItem {
        public MallocComboBoxItem(MallocHandler.Malloc malloc, Style style) {
            Malloc = malloc;
            Content = Malloc.Name.Replace("_", "__");
            Style = style;
        }

        public MallocComboBoxItem(string name, Style style) {
            Malloc = new MallocHandler.Malloc(name);
            Content = Malloc.Name.Replace("_", "__");
            Style = style;
        }

        public MallocHandler.Malloc Malloc { get; }
    }
}

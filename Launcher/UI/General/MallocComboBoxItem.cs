using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI.General {
    public class MallocComboBoxItem : ComboBoxItem {
        /// <inheritdoc />
        /// <summary>
        ///     Creates ComboBoxItem with given Malloc object and style.
        /// </summary>
        /// <param name="malloc">Malloc to assign ComboBoxItem to</param>
        /// <param name="style">Style to set ComboBoxItem to</param>
        public MallocComboBoxItem(MallocHandler.Malloc malloc, Style style) {
            Malloc = malloc;
            Content = Malloc.Name.Replace("_", "__");
            Style = style;
        }

        /// <summary>
        ///     Creates ComboBoxItem with no Malloc object and style. System default malloc
        /// </summary>
        /// <param name="name">Malloc to assign ComboBoxItem to</param>
        /// <param name="style">Style to set ComboBoxItem to</param>
        public MallocComboBoxItem(string name, Style style) {
            Malloc = new MallocHandler.Malloc(name);
            Content = Malloc.Name.Replace("_", "__");
            Style = style;
        }

        public MallocHandler.Malloc Malloc { get; }
    }
}
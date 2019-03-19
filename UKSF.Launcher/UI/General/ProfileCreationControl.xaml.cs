using System.Windows.Controls;
using UKSF.Launcher.Game;

namespace UKSF.Launcher.UI.General {
    public partial class ProfileCreationControl {
        public ProfileCreationControl() {
            InitializeComponent();

            ProfileCreationControlDropdownPrefix.Items.Clear();
            foreach (string rank in ProfileHandler.PROFILE_PREFIXES) {
                ProfileCreationControlDropdownPrefix.Items.Add(rank);
            }
        }

        public string Initial { get; private set; }

        public string Rank { get; private set; }
        public string Surname { get; private set; }

        private void ProfileCreationControlDropdownPrefix_OnSelectionChanged(object sender, SelectionChangedEventArgs args) =>
            Rank = ProfileHandler.PROFILE_PREFIXES[ProfileCreationControlDropdownPrefix.SelectedIndex];

        private void ProfileCreationControlTextBoxSurname_OnTextChanged(object sender, TextChangedEventArgs args) => Surname = ProfileCreationControlTextBoxSurname.Text;

        private void ProfileCreationControlTextBoxInitial_OnTextChanged(object sender, TextChangedEventArgs args) => Initial = ProfileCreationControlTextBoxInitial.Text;
    }
}

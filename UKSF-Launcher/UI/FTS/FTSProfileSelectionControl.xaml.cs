using System.Windows.Controls;
using static UKSF_Launcher.Game.ProfileHandler;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsProfileSelectionControl.xaml
    /// </summary>
    public partial class FtsProfileSelectionControl {
        public FtsProfileSelectionControl() {
            InitializeComponent();

            FtsProfileSelectionControlDropdownPrefix.Items.Clear();
            foreach (string rank in PROFILE_PREFIXES) {
                FtsProfileSelectionControlDropdownPrefix.Items.Add(rank);
            }
        }

        public static string Rank { get; private set; }
        public static string Surname { get; private set; }
        public static string Initial { get; private set; }

        private void FtsProfileSelectionControlDropdownPrefix_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Rank = PROFILE_PREFIXES[FtsProfileSelectionControlDropdownPrefix.SelectedIndex];

        private void FtsProfileSelectionControlTextBoxSurname_OnTextChanged(object sender, TextChangedEventArgs e) =>
            Surname = FtsProfileSelectionControlTextBoxSurname.Text;

        private void FtsProfileSelectionControlTextBoxInitial_OnTextChanged(object sender, TextChangedEventArgs e) =>
            Initial = FtsProfileSelectionControlTextBoxInitial.Text;
    }
}
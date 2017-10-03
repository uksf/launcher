using System.Windows.Controls;
using UKSF_Launcher.Game;

namespace UKSF_Launcher.UI.General {
    /// <summary>
    ///     Interaction logic for FtsProfileSelectionControl.xaml
    /// </summary>
    public partial class ProfileCreationControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsProfileSelectionControl object.
        /// </summary>
        public ProfileCreationControl() {
            InitializeComponent();

            ProfileCreationControlDropdownPrefix.Items.Clear();
            foreach (string rank in ProfileHandler.PROFILE_PREFIXES) {
                ProfileCreationControlDropdownPrefix.Items.Add(rank);
            }
        }

        public string Rank { get; private set; }
        public string Surname { get; private set; }
        public string Initial { get; private set; }


        /// <summary>
        ///     Triggered when rank prefix selection is changed. Updates rank value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnSelectionChanged arguments</param>
        private void ProfileCreationControlDropdownPrefix_OnSelectionChanged(object sender, SelectionChangedEventArgs args) =>
            Rank = ProfileHandler.PROFILE_PREFIXES[ProfileCreationControlDropdownPrefix.SelectedIndex];

        /// <summary>
        ///     Triggered when surname textbox is changed. Updates surname value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnTextChanged arguments</param>
        private void ProfileCreationControlTextBoxSurname_OnTextChanged(object sender, TextChangedEventArgs args) =>
            Surname = ProfileCreationControlTextBoxSurname.Text;

        /// <summary>
        ///     Triggered when initial textbox is changed. Updates initial value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnTextChanged arguments</param>
        private void ProfileCreationControlTextBoxInitial_OnTextChanged(object sender, TextChangedEventArgs args) =>
            Initial = ProfileCreationControlTextBoxInitial.Text;
    }
}
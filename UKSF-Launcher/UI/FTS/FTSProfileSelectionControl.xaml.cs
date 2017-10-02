using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using static UKSF_Launcher.Game.ProfileHandler;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsProfileSelectionControl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class FtsProfileSelectionControl {
        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsProfileSelectionControl object.
        /// </summary>
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


        /// <summary>
        ///     Triggered when rank prefix selection is changed. Updates rank value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnSelectionChanged arguments</param>
        private void FtsProfileSelectionControlDropdownPrefix_OnSelectionChanged(object sender, SelectionChangedEventArgs args) =>
            Rank = PROFILE_PREFIXES[FtsProfileSelectionControlDropdownPrefix.SelectedIndex];

        /// <summary>
        ///     Triggered when surname textbox is changed. Updates surname value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnTextChanged arguments</param>
        private void FtsProfileSelectionControlTextBoxSurname_OnTextChanged(object sender, TextChangedEventArgs args) =>
            Surname = FtsProfileSelectionControlTextBoxSurname.Text;

        /// <summary>
        ///     Triggered when initial textbox is changed. Updates initial value.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">OnTextChanged arguments</param>
        private void FtsProfileSelectionControlTextBoxInitial_OnTextChanged(object sender, TextChangedEventArgs args) =>
            Initial = FtsProfileSelectionControlTextBoxInitial.Text;
    }
}
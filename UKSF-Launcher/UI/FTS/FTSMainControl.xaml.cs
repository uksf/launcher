using System.Windows;
using UKSF_Launcher.UI.General;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FtsMainControl.xaml
    /// </summary>
    public partial class FtsMainControl {
        public static readonly RoutedEvent FTS_MAIN_CONTROL_TITLE_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_TITLE_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        public static readonly RoutedEvent FTS_MAIN_CONTROL_DESCRIPTION_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_DESCRIPTION_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        public static readonly RoutedEvent FTS_MAIN_CONTROL_FINISH_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_FINISH_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        public static readonly RoutedEvent FTS_MAIN_CONTROL_WARNING_EVENT =
            EventManager.RegisterRoutedEvent("FTS_MAIN_CONTROL_WARNING_EVENT", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FtsMainControl));

        private int _windowIndex;

        /// <inheritdoc />
        /// <summary>
        ///     Creates new FtsMainControl object.
        /// </summary>
        public FtsMainControl() {
            InitializeComponent();

            AddHandler(FTS_MAIN_CONTROL_TITLE_EVENT, new RoutedEventHandler(FTSMainControlTitle_Update));
            AddHandler(FTS_MAIN_CONTROL_DESCRIPTION_EVENT, new RoutedEventHandler(FTSMainControlDescription_Update));
            AddHandler(FTS_MAIN_CONTROL_WARNING_EVENT, new RoutedEventHandler(FTSMainControlWarning_Update));

            UpdateControls();
        }

        /// <summary>
        ///     Triggered by eventhandler to update title text.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void FTSMainControlTitle_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs) args;
            FtsMainControlTitle.Content = stringArgs.Text;
        }

        /// <summary>
        ///     Triggered by eventhandler to update description text.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void FTSMainControlDescription_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs) args;
            FtsMainControlDescription.Text = stringArgs.Text;
        }

        /// <summary>
        ///     Triggered by eventhandler to update warning text and show/hide warning.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">MouseDown arguments</param>
        private void FTSMainControlWarning_Update(object sender, RoutedEventArgs args) {
            WarningRoutedEventArgs warningArgs = (WarningRoutedEventArgs) args;
            FtsMainControlWarningText.Visibility = warningArgs.Visibility;
            FtsMainControlWarningText.Content = warningArgs.Warning;
            if (warningArgs.Block) {
                FtsMainControlButtonNext.IsEnabled = false;
                FtsMainControlButtonFinish.IsEnabled = false;
            } else {
                FtsMainControlButtonNext.IsEnabled = true;
                FtsMainControlButtonFinish.IsEnabled = true;
            }
        }

        /// <summary>
        ///     Triggered when next/back button is clicked. Increments/decrements index and updates controls.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSMainControlButtonProgress_Click(object sender, RoutedEventArgs args) {
            if (Equals(sender, FtsMainControlButtonNext)) {
                _windowIndex++;
            } else {
                _windowIndex--;
            }
            UpdateControls();
        }

        /// <summary>
        ///     Updates controls and progress buttons based on current index.
        /// </summary>
        private void UpdateControls() {
            switch (_windowIndex) {
                case 0:
                    FtsGameExeControl.Show();
                    FtsModLocationControl.Hide();
                    FtsProfileControl.Hide();

                    FtsMainControlButtonNext.Visibility = Visibility.Visible;
                    FtsMainControlButtonBack.Visibility = Visibility.Collapsed;
                    FtsMainControlButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    FtsGameExeControl.Hide();
                    FtsModLocationControl.Show();
                    FtsProfileControl.Hide();

                    FtsMainControlButtonNext.Visibility = Visibility.Visible;
                    FtsMainControlButtonBack.Visibility = Visibility.Visible;
                    FtsMainControlButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    FtsGameExeControl.Hide();
                    FtsModLocationControl.Hide();
                    FtsProfileControl.Show();

                    FtsMainControlButtonNext.Visibility = Visibility.Collapsed;
                    FtsMainControlButtonBack.Visibility = Visibility.Visible;
                    FtsMainControlButtonFinish.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        ///     Triggered when finish button is clicked. Gets all setup values and writes them to the settings registry.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSMainControlButtonFinish_Click(object sender, RoutedEventArgs args) {
            LogHandler.LogInfo("Finishing first time setup");
            Global.GAME_LOCATION =
                (string) Core.SettingsHandler.WriteSetting("GAME_LOCATION", FtsGameExeControl.FtsGameExeControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            Global.MOD_LOCATION =
                (string) Core.SettingsHandler.WriteSetting("MOD_LOCATION",
                                                           FtsModLocationControl.FtsModLocationControlLocationTextboxControl.LocationTextboxControlTextBoxLocation.Text);
            Global.PROFILE = (string) Core.SettingsHandler.WriteSetting("PROFILE",
                                                                        ((ProfileComboBoxItem) FtsProfileControl
                                                                            .FtsProfileControlProfileSelectionControl.ProfileSelectionControlDropdownProfile.SelectedItem)
                                                                        .Profile.Name);
            RaiseEvent(new RoutedEventArgs(FTS_MAIN_CONTROL_FINISH_EVENT));
        }

        /// <summary>
        ///     Triggered when cancel button is clicked. Closes application.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">Click arguments</param>
        private void FTSMainControlButtonCancel_Click(object sender, RoutedEventArgs args) {
            LogHandler.LogSeverity(Global.Severity.WARNING, "First Time Setup Cancelled. Progress has not been saved.");
            Core.ShutDown();
        }

        public class StringRoutedEventArgs : RoutedEventArgs {
            public StringRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public string Text { get; set; }
        }

        public class WarningRoutedEventArgs : RoutedEventArgs {
            public WarningRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool Block { get; set; }
            public string Warning { get; set; }
            public Visibility Visibility { get; set; }
        }
    }
}
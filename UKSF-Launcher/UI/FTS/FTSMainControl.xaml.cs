using System.Windows;
using UKSF_Launcher.Utility;

namespace UKSF_Launcher.UI.FTS {
    /// <summary>
    ///     Interaction logic for FTS_MainControl.xaml
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

        public FtsMainControl() {
            InitializeComponent();

            AddHandler(FTS_MAIN_CONTROL_TITLE_EVENT, new RoutedEventHandler(FTSMainControlTitle_Update));
            AddHandler(FTS_MAIN_CONTROL_DESCRIPTION_EVENT, new RoutedEventHandler(FTSMainControlDescription_Update));
            AddHandler(FTS_MAIN_CONTROL_WARNING_EVENT, new RoutedEventHandler(FTSMainControlWarning_Update));

            UpdateControls();
        }

        private void FTSMainControlTitle_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs) args;
            FtsMainControlTitle.Content = stringArgs.Text;
        }

        private void FTSMainControlDescription_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs) args;
            FtsMainControlDescription.Text = stringArgs.Text;
        }

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

        private void FTSMainControlButtonProgress_Click(object sender, RoutedEventArgs e) {
            if (Equals(sender, FtsMainControlButtonNext)) {
                _windowIndex++;
            } else {
                _windowIndex--;
            }
            UpdateControls();
        }

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

        private void FTSMainControlButtonFinish_Click(object sender, RoutedEventArgs e) {
            LogHandler.LogInfo("Finishing first time setup");
            Global.GAME_LOCATION = (string) SettingsHandler.WriteSetting("GAME_LOCATION", FtsGameExeControl.FtsGameExeControlTextBoxLocation.Text);
            Global.MOD_LOCATION = (string)SettingsHandler.WriteSetting("MOD_LOCATION", FtsModLocationControl.FtsModLocationControlTextBoxLocation.Text);
            Global.PROFILE = (string)SettingsHandler.WriteSetting("PROFILE", FtsProfileControl.FtsProfileControlDropdownProfile.SelectionBoxItem);
            RaiseEvent(new RoutedEventArgs(FTS_MAIN_CONTROL_FINISH_EVENT));
        }

        private void FTSMainControlButtonCancel_Click(object sender, RoutedEventArgs e) {
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
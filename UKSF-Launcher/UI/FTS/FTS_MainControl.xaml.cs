using System.Windows;
using System.Windows.Controls;
using UKSF_Launcher.Utility;
using static UKSF_Launcher.Global;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for FTS_MainControl.xaml
    /// </summary>
    public partial class FTS_MainControl : UserControl {

        private int windowIndex = 0;

        public FTS_MainControl() {
            InitializeComponent();

            AddHandler(FTS_MainControl_Title_Event, new RoutedEventHandler(FTS_MainControl_Title_Update));
            AddHandler(FTS_MainControl_Description_Event, new RoutedEventHandler(FTS_MainControl_Description_Update));
            AddHandler(FTS_MainControl_Warning_Event, new RoutedEventHandler(FTS_MainControl_Warning_Update));

            UpdateControls();
        }

        public static readonly RoutedEvent FTS_MainControl_Title_Event = EventManager.RegisterRoutedEvent("FTS_MainControl_Title_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FTS_MainControl));
        public static readonly RoutedEvent FTS_MainControl_Description_Event = EventManager.RegisterRoutedEvent("FTS_MainControl_Description_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FTS_MainControl));
        public static readonly RoutedEvent FTS_MainControl_Finish_Event = EventManager.RegisterRoutedEvent("FTS_MainControl_Finish_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FTS_MainControl));
        public static readonly RoutedEvent FTS_MainControl_Warning_Event = EventManager.RegisterRoutedEvent("FTS_MainControl_Warning_Event", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FTS_MainControl));

        public event RoutedEventHandler FTS_MainControl_Title_EventHandler {
            add { AddHandler(FTS_MainControl_Title_Event, value); }
            remove { RemoveHandler(FTS_MainControl_Title_Event, value); }
        }
        public event RoutedEventHandler FTS_MainControl_Description_EventHandler {
            add { AddHandler(FTS_MainControl_Description_Event, value); }
            remove { RemoveHandler(FTS_MainControl_Description_Event, value); }
        }
        public event RoutedEventHandler FTS_MainControl_Finish_EventHandler {
            add { AddHandler(FTS_MainControl_Finish_Event, value); }
            remove { RemoveHandler(FTS_MainControl_Finish_Event, value); }
        }
        public event RoutedEventHandler FTS_MainControl_Warning_EventHandler {
            add { AddHandler(FTS_MainControl_Warning_Event, value); }
            remove { RemoveHandler(FTS_MainControl_Warning_Event, value); }
        }

        private void FTS_MainControl_Title_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs)args;
            FTS_MainControl_Title.Content = stringArgs.Text;
        }

        private void FTS_MainControl_Description_Update(object sender, RoutedEventArgs args) {
            StringRoutedEventArgs stringArgs = (StringRoutedEventArgs)args;
            FTS_MainControl_Description.Text = stringArgs.Text;
        }

        private void FTS_MainControl_Warning_Update(object sender, RoutedEventArgs args) {
            WarningRoutedEventArgs warningArgs = (WarningRoutedEventArgs)args;
            FTS_MainControl_WarningText.Visibility = warningArgs.Visibility;
            FTS_MainControl_WarningText.Content = warningArgs.Warning;
            if (warningArgs.Block) {
                FTS_MainControl_ButtonNext.IsEnabled = false;
                FTS_MainControl_ButtonFinish.IsEnabled = false;
            } else {
                FTS_MainControl_ButtonNext.IsEnabled = true;
                FTS_MainControl_ButtonFinish.IsEnabled = true;
            }
        }

        private void FTS_MainControl_ButtonProgress_Click(object sender, RoutedEventArgs e) {
            if (sender == FTS_MainControl_ButtonNext) {
                windowIndex++;
            }
            else {
                windowIndex--;
            }
            UpdateControls();
        }

        private void UpdateControls() {
            switch (windowIndex) {
                case 0: // Game exe
                    FTS_GameExeControl.Show();
                    FTS_ModLocationControl.Hide();
                    FTS_ProfileControl.Hide();

                    FTS_MainControl_ButtonNext.Visibility = Visibility.Visible;
                    FTS_MainControl_ButtonBack.Visibility = Visibility.Collapsed;
                    FTS_MainControl_ButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 1: // Mod download
                    FTS_GameExeControl.Hide();
                    FTS_ModLocationControl.Show();
                    FTS_ProfileControl.Hide();

                    FTS_MainControl_ButtonNext.Visibility = Visibility.Visible;
                    FTS_MainControl_ButtonBack.Visibility = Visibility.Visible;
                    FTS_MainControl_ButtonFinish.Visibility = Visibility.Collapsed;
                    break;
                case 2: // Profile
                    FTS_GameExeControl.Hide();
                    FTS_ModLocationControl.Hide();
                    FTS_ProfileControl.Show();

                    FTS_MainControl_ButtonNext.Visibility = Visibility.Collapsed;
                    FTS_MainControl_ButtonBack.Visibility = Visibility.Visible;
                    FTS_MainControl_ButtonFinish.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void FTS_MainControl_ButtonFinish_Click(object sender, RoutedEventArgs e) {
            LogHandler.LogInfo("Finishing first time setup");
            GAME_LOCATION = (string)SettingsHandler.WriteSetting("GAME_LOCATION", FTS_GameExeControl.FTS_GameExeControl_TextBoxLocation.Text);
            RaiseEvent(new RoutedEventArgs(FTS_MainControl_Finish_Event));
        }

        private void FTS_MainControl_ButtonCancel_Click(object sender, RoutedEventArgs e) {
            LogHandler.LogSeverity(Severity.WARNING, "First Time Setup Cancelled. Progress has not been saved.");
            Core.ShutDown();
        }

        public class StringRoutedEventArgs : RoutedEventArgs {
            private string text;
            public StringRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public string Text { get => text; set => text = value; }
        }

        public class WarningRoutedEventArgs : RoutedEventArgs {
            private Visibility visibility;
            private string warning;
            private bool block;
            public WarningRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
            public bool Block { get => block; set => block = value; }
            public string Warning { get => warning; set => warning = value; }
            public Visibility Visibility { get => visibility; set => visibility = value; }
        }
    }
}

using System.Windows;

namespace UKSF_Launcher {
    /// <summary>
    /// Interaction logic for Dialog_Window.xaml
    /// </summary>
    public partial class Dialog_Window : Window {

        public enum DialogBoxType {
            OK,
            OKCancel
        }

        private static Dialog_Window dialog;
        private static MessageBoxResult result = MessageBoxResult.None;

        public Dialog_Window() {
            InitializeComponent();
            AddHandler(Dialog_TitleBarControl.Dialog_TitleBarControl_MouseDown_Event, new RoutedEventHandler(TitleBar_MouseDown));
            AddHandler(Dialog_MainControl.Dialog_MainControl_ButtonOKClick_Event, new RoutedEventHandler(ButtonOK_Click));
            AddHandler(Dialog_MainControl.Dialog_MainControl_ButtonCancelClick_Event, new RoutedEventHandler(ButtonCancel_Click));
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type) {
            switch (type) {
                case DialogBoxType.OK:
                    return Show(title, message, MessageBoxButton.OK);
                default:
                    return Show(title, message, MessageBoxButton.OKCancel);
            }
        }

        private static MessageBoxResult Show(string title, string message, MessageBoxButton button) {
            dialog = new Dialog_Window();
            dialog.Dialog_TitleBarControl.Dialog_TitleBarControl_Label.Content = title;
            dialog.Dialog_MainControl.Dialog_MainControl_TextBlock.Text = message;
            SetButton(button);
            dialog.ShowDialog();
            return result;
        }

        private static void SetButton(MessageBoxButton button) {
            switch (button) {
                case MessageBoxButton.OK:
                    dialog.Dialog_MainControl.Dialog_MainControl_ButtonOK.Visibility = Visibility.Visible;
                    dialog.Dialog_MainControl.Dialog_MainControl_ButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    dialog.Dialog_MainControl.Dialog_MainControl_ButtonOK.Visibility = Visibility.Visible;
                    dialog.Dialog_MainControl.Dialog_MainControl_ButtonCancel.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void TitleBar_MouseDown(object sender, RoutedEventArgs args) {
            DragMove();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.OK;
            dialog.Close();
            dialog = null;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs args) {
            result = MessageBoxResult.Cancel;
            dialog.Close();
            dialog = null;
        }
    }
}

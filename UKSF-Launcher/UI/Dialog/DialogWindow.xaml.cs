using System.Windows;

namespace UKSF_Launcher.UI.Dialog {
    /// <summary>
    ///     Interaction logic for Dialog_Window.xaml
    /// </summary>
    public partial class DialogWindow {
        public enum DialogBoxType {
            OK,
            OK_CANCEL
        }

        private static DialogWindow _dialog;
        private static MessageBoxResult _result = MessageBoxResult.None;

        public DialogWindow() {
            InitializeComponent();

            AddHandler(DialogTitleBarControl.DIALOG_TITLE_BAR_CONTROL_MOUSE_DOWN_EVENT, new RoutedEventHandler(DialogTitleBar_MouseDown));
            AddHandler(DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_OK_CLICK_EVENT, new RoutedEventHandler(DialogButtonOK_Click));
            AddHandler(DialogMainControl.DIALOG_MAIN_CONTROL_BUTTON_CANCEL_CLICK_EVENT, new RoutedEventHandler(DialogButtonCancel_Click));
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type) {
            switch (type) {
                case DialogBoxType.OK: return Show(title, message, MessageBoxButton.OK);
                case DialogBoxType.OK_CANCEL: return Show(title, message, MessageBoxButton.OKCancel);
                default: return Show(title, message, MessageBoxButton.OKCancel);
            }
        }

        public static MessageBoxResult Show(string title, string message, DialogBoxType type, UIElement control) {
            switch (type) {
                case DialogBoxType.OK: return Show(title, message, MessageBoxButton.OK, control);
                case DialogBoxType.OK_CANCEL: return Show(title, message, MessageBoxButton.OKCancel, control);
                default: return Show(title, message, MessageBoxButton.OKCancel, control);
            }
        }

        private static MessageBoxResult Show(string title, string message, MessageBoxButton button, UIElement control = null) {
            _dialog = new DialogWindow();
            _dialog.DialogTitleBarControl.DialogTitleBarControlLabel.Content = title;
            if (control != null) {
                _dialog.DialogMainControl.DialogMainControlGrid.Children.Add(control);
            }
            _dialog.DialogMainControl.DialogMainControlTextBlock.Text = message;
            SetButton(button);
            _dialog.ShowDialog();
            return _result;
        }

        private static void SetButton(MessageBoxButton button) {
            switch (button) {
                case MessageBoxButton.OK:
                    _dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Visible;
                    _dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    _dialog.DialogMainControl.DialogMainControlButtonOk.Visibility = Visibility.Visible;
                    _dialog.DialogMainControl.DialogMainControlButtonCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    break;
                case MessageBoxButton.YesNo:
                    break;
            }
        }

        private void DialogTitleBar_MouseDown(object sender, RoutedEventArgs args) => DragMove();

        private static void DialogButtonOK_Click(object sender, RoutedEventArgs args) {
            _result = MessageBoxResult.OK;
            _dialog.Close();
            _dialog = null;
        }

        private static void DialogButtonCancel_Click(object sender, RoutedEventArgs args) {
            _result = MessageBoxResult.Cancel;
            _dialog.Close();
            _dialog = null;
        }
    }
}
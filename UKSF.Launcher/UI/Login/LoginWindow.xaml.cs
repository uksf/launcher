using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UKSF.Launcher.UI.Login {
    public partial class LoginWindow {
        public delegate string ResultEventDelegate(Tuple<MessageBoxResult, string, string> result);

        private static LoginWindow instance;

        public LoginWindow() {
            InitializeComponent();
        }

        public static event ResultEventDelegate ResultEvent;

        public static void CreateLoginWindow(ResultEventDelegate eventDelegate) {
            if (instance != null) return;
            instance = new LoginWindow();
            ResultEvent += eventDelegate;
        }

        public static void UpdateDetails(string message, string email) {
            instance.LoginMainControlWarningText.Text = message;
            instance.LoginMainControlTextBoxEmail.Text = email;
            instance.LoginMainControlTextBoxPassword.Password = "";
            instance.Show();
        }

        private async void LoginMainControlButtonLogin_Click(object sender, RoutedEventArgs args) {
            LoginMainControlLoginButton.IsEnabled = false;
            string email = LoginMainControlTextBoxEmail.Text;
            string password = LoginMainControlTextBoxPassword.Password;
            await Task.Run(() => ResultEvent?.Invoke(new Tuple<MessageBoxResult, string, string>(MessageBoxResult.OK, email, password)));
        }

        private void LoginMainControlButtonCancel_Click(object sender, RoutedEventArgs args) {
            ResultEvent?.Invoke(new Tuple<MessageBoxResult, string, string>(MessageBoxResult.Cancel, "", ""));
        }

        private void LoginTitleBarControl_MouseDown(object sender, MouseButtonEventArgs args) {
            if (args.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        private void LoginMainControlTextBoxEmail_TextChanged(object sender, TextChangedEventArgs args) {
            instance.LoginMainControlLoginButton.IsEnabled = instance.LoginMainControlTextBoxEmail.Text.Length > 0 && instance.LoginMainControlTextBoxPassword.Password.Length > 0;
        }

        private void LoginMainControlTextBoxPassword_PasswordChanged(object sender, RoutedEventArgs args) {
            instance.LoginMainControlLoginButton.IsEnabled = instance.LoginMainControlTextBoxEmail.Text.Length > 0 && instance.LoginMainControlTextBoxPassword.Password.Length > 0;
        }

        private void LoginTitleBarControlButtonClose_Click(object sender, RoutedEventArgs args) {
            Core.ShutDown();
        }
    }
}

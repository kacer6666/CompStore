using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин");
                txtLogin.Focus();
                return;
            }

            if (login.Length > 50)
            {
                MessageBox.Show("Логин слишком длинный");
                txtLogin.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль");
                txtPassword.Focus();
                return;
            }

            if (password.Length > 50)
            {
                MessageBox.Show("Пароль слишком длинный");
                txtPassword.Focus();
                return;
            }

            var userObj = AppConnect.model01.Users.FirstOrDefault(x =>
                x.Login == login && x.Password == password);

            if (userObj == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                txtPassword.Clear();
                txtLogin.Focus();
                return;
            }

            if (userObj.IsBlocked)
            {
                MessageBox.Show("Ваш аккаунт заблокирован");
                return;
            }

            AppConnect.CurrentUser = userObj;
            MessageBox.Show("Добро пожаловать, " + userObj.FullName);
            AppFrame.frmMain.Navigate(new CatalogPage());
        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new RegisterPage());
        }
    }
}
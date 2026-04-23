using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadOrders();
            LoadReceipts();
        }

        private void LoadUserData()
        {
            if (AppConnect.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

            tbFullName.Text = "Имя: " + AppConnect.CurrentUser.FullName;
            tbLogin.Text = "Логин: " + AppConnect.CurrentUser.Login;
            tbEmail.Text = "Email: " + AppConnect.CurrentUser.Email;
            tbPhone.Text = "Телефон: " + AppConnect.CurrentUser.Phone;
            tbCreatedAt.Text = "Дата регистрации: " + AppConnect.CurrentUser.CreatedAt;
        }

        private void LoadOrders()
        {
            lvOrders.ItemsSource = AppConnect.model01.Orders
                .Where(x => x.UserID == AppConnect.CurrentUser.UserID)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        private void LoadReceipts()
        {
            lvReceipts.ItemsSource = AppConnect.model01.Receipts
                .Where(x => x.Orders.UserID == AppConnect.CurrentUser.UserID)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        private void btnOpenReceipt_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Receipts selectedReceipt = button.Tag as Receipts;

            if (selectedReceipt == null)
                return;

            if (!File.Exists(selectedReceipt.PdfPath))
            {
                MessageBox.Show("Файл чека не найден");
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = selectedReceipt.PdfPath;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }
    }
}
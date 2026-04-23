using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class AdminUsersPage : Page
    {
        public AdminUsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            lvUsers.ItemsSource = AppConnect.model01.Users.ToList();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }

        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser = lvUsers.SelectedItem as Users;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            if (selectedUser.RoleID == 1)
            {
                int adminCount = AppConnect.model01.Users.Count(x => x.RoleID == 1);

                if (adminCount <= 1)
                {
                    MessageBox.Show("Нельзя снять роль у последнего администратора");
                    return;
                }

                selectedUser.RoleID = 2;
                MessageBox.Show("Пользователь переведен в роль: Пользователь");
            }
            else
            {
                selectedUser.RoleID = 1;
                MessageBox.Show("Пользователь переведен в роль: Администратор");
            }

            AppConnect.model01.SaveChanges();
            LoadUsers();
        }

        private void btnToggleBlock_Click(object sender, RoutedEventArgs e)
        {
            Users selectedUser = lvUsers.SelectedItem as Users;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            if (selectedUser.UserID == AppConnect.CurrentUser.UserID)
            {
                MessageBox.Show("Нельзя блокировать самого себя");
                return;
            }

            selectedUser.IsBlocked = !selectedUser.IsBlocked;

            AppConnect.model01.SaveChanges();
            LoadUsers();

            MessageBox.Show("Статус блокировки изменен");
        }
    }
}
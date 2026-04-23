using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class AdminProductsPage : Page
    {
        public AdminProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            lvAdminProducts.ItemsSource = AppConnect.model01.Products.ToList();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new AddEditProductPage(null));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Products selectedProduct = lvAdminProducts.SelectedItem as Products;

            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования");
                return;
            }

            AppFrame.frmMain.Navigate(new AddEditProductPage(selectedProduct));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Products selectedProduct = lvAdminProducts.SelectedItem as Products;

            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления");
                return;
            }

            bool hasOrders = AppConnect.model01.OrderItems.Any(x => x.ProductID == selectedProduct.ProductID);
            bool hasCart = AppConnect.model01.CartItems.Any(x => x.ProductID == selectedProduct.ProductID);

            if (hasOrders || hasCart)
            {
                MessageBox.Show("Нельзя удалить товар, который уже используется в заказах или корзинах");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить товар \"" + selectedProduct.ProductName + "\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppConnect.model01.Products.Remove(selectedProduct);
                AppConnect.model01.SaveChanges();

                MessageBox.Show("Товар удален");
                LoadProducts();
            }
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new AdminUsersPage());
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new AdminOrdersPage());
        }
    }
}
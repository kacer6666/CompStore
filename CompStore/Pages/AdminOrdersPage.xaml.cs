using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class AdminOrdersPage : Page
    {
        public AdminOrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            lvOrders.ItemsSource = AppConnect.model01.Orders
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            Orders selectedOrder = lvOrders.SelectedItem as Orders;

            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            if (selectedOrder.OrderStatusID == 4)
            {
                MessageBox.Show("Нельзя подтвердить отмененный заказ");
                return;
            }

            if (selectedOrder.OrderStatusID == 3)
            {
                MessageBox.Show("Заказ уже выдан");
                return;
            }

            selectedOrder.OrderStatusID = 2;
            AppConnect.model01.SaveChanges();
            LoadOrders();

            MessageBox.Show("Заказ подтвержден");
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            Orders selectedOrder = lvOrders.SelectedItem as Orders;

            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            if (selectedOrder.OrderStatusID == 4)
            {
                MessageBox.Show("Нельзя выдать отмененный заказ");
                return;
            }

            selectedOrder.OrderStatusID = 3;
            AppConnect.model01.SaveChanges();
            LoadOrders();

            MessageBox.Show("Заказ отмечен как выданный");
        }

        private void btnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            Orders selectedOrder = lvOrders.SelectedItem as Orders;

            if (selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            if (selectedOrder.OrderStatusID == 4)
            {
                MessageBox.Show("Заказ уже отменен");
                return;
            }

            var orderItems = AppConnect.model01.OrderItems
                .Where(x => x.OrderID == selectedOrder.OrderID)
                .ToList();

            foreach (var item in orderItems)
            {
                var product = AppConnect.model01.Products
                    .FirstOrDefault(x => x.ProductID == item.ProductID);

                if (product != null)
                {
                    product.QuantityInStock += item.Quantity;
                }
            }

            selectedOrder.OrderStatusID = 4;
            AppConnect.model01.SaveChanges();
            LoadOrders();

            MessageBox.Show("Заказ отменен, остатки возвращены");
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }
    }
}
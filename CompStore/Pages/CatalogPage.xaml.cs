using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();

            tbWelcome.Text = "Добро пожаловать, " + AppConnect.CurrentUser.FullName;

            LoadCategories();
            LoadSort();
            LoadProducts();

            if (AppConnect.CurrentUser.RoleID == 1)
            {
                
                btnAdmin.Visibility = Visibility.Visible;
            }
            else
            {
                
                btnAdmin.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSort()
        {
            cmbSort.Items.Clear();
            cmbSort.Items.Add("Без сортировки");
            cmbSort.Items.Add("Цена по возрастанию");
            cmbSort.Items.Add("Цена по убыванию");
            cmbSort.SelectedIndex = 0;
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("Все категории");

            var categories = AppConnect.model01.Categories.ToList();

            foreach (var category in categories)
            {
                cmbCategory.Items.Add(category.CategoryName);
            }

            cmbCategory.SelectedIndex = 0;
        }

        private string GetImagePath(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            string imagePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                imageName);

            if (File.Exists(imagePath))
                return imagePath;

            return null;
        }

        private void LoadProducts()
        {
            var products = AppConnect.model01.Products.ToList();

            if (cmbCategory.SelectedIndex > 0)
            {
                string selectedCategory = cmbCategory.SelectedItem.ToString();

                products = products.Where(x =>
                    x.Categories.CategoryName == selectedCategory
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();

                products = products.Where(x =>
                    x.ProductName.ToLower().Contains(searchText) ||
                    (x.Model != null && x.Model.ToLower().Contains(searchText)) ||
                    (x.CpuInfo != null && x.CpuInfo.ToLower().Contains(searchText)) ||
                    (x.GpuInfo != null && x.GpuInfo.ToLower().Contains(searchText)) ||
                    (x.Specifications != null && x.Specifications.ToLower().Contains(searchText))
                ).ToList();
            }

            if (cmbSort.SelectedIndex == 1)
            {
                products = products.OrderBy(x => x.Price).ToList();
            }
            else if (cmbSort.SelectedIndex == 2)
            {
                products = products.OrderByDescending(x => x.Price).ToList();
            }

            var displayItems = products.Select(x => new ProductDisplayItem
            {
                SourceProduct = x,
                ProductName = x.ProductName,
                Model = x.Model,
                Price = x.Price,
                WarrantyMonths = x.WarrantyMonths,
                ShortSpecifications = x.Specifications != null && x.Specifications.Length > 65
                    ? x.Specifications.Substring(0, 65) + "..."
                    : x.Specifications,
                ImagePath = GetImagePath(x.MainImage),
                StatusName = x.ProductStatuses != null ? x.ProductStatuses.StatusName : "",
                ComputerType = x.ComputerType,
                CpuInfo = x.CpuInfo,
                GpuInfo = x.GpuInfo
            }).ToList();

            lvProducts.ItemsSource = displayItems;
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Products selectedProduct = button.Tag as Products;

            if (selectedProduct == null)
            {
                MessageBox.Show("Товар не выбран");
                return;
            }

            var userCart = AppConnect.model01.Carts
                .FirstOrDefault(x => x.UserID == AppConnect.CurrentUser.UserID);

            if (userCart == null)
            {
                MessageBox.Show("Корзина пользователя не найдена");
                return;
            }

            var existingCartItem = AppConnect.model01.CartItems
                .FirstOrDefault(x =>
                    x.CartID == userCart.CartID &&
                    x.ProductID == selectedProduct.ProductID);

            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity + 1 > selectedProduct.QuantityInStock)
                {
                    MessageBox.Show("Нельзя добавить больше товара, чем есть на складе");
                    return;
                }

                existingCartItem.Quantity += 1;
            }
            else
            {
                if (selectedProduct.QuantityInStock < 1)
                {
                    MessageBox.Show("Товара нет в наличии");
                    return;
                }

                CartItems newCartItem = new CartItems();
                newCartItem.CartID = userCart.CartID;
                newCartItem.ProductID = selectedProduct.ProductID;
                newCartItem.Quantity = 1;
                newCartItem.PriceAtMoment = selectedProduct.Price;

                AppConnect.model01.CartItems.Add(newCartItem);
            }

            AppConnect.model01.SaveChanges();
            MessageBox.Show("Товар добавлен в корзину");
        }

        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Products selectedProduct = button.Tag as Products;

            if (selectedProduct == null)
                return;

            AppFrame.frmMain.Navigate(new ProductDetailsPage(selectedProduct));
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.CurrentUser = null;
            AppFrame.frmMain.Navigate(new AuthorizationPage());
        }

        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new CartPage());
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new ProfilePage());
        }

        private void btnQr_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new QrPage());
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.Navigate(new AdminProductsPage());
        }
    }

    public class ProductDisplayItem
    {
        public Products SourceProduct { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public int? WarrantyMonths { get; set; }
        public string ShortSpecifications { get; set; }
        public string ImagePath { get; set; }
        public string StatusName { get; set; }
        public string ComputerType { get; set; }
        public string CpuInfo { get; set; }
        public string GpuInfo { get; set; }
    }
}
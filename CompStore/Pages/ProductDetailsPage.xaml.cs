using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class ProductDetailsPage : Page
    {
        private Products currentProduct;

        public ProductDetailsPage(Products product)
        {
            InitializeComponent();
            currentProduct = product;
            LoadProductData();
        }

        private void LoadProductData()
        {
            if (currentProduct == null)
                return;

            tbName.Text = currentProduct.ProductName;
            tbModel.Text = "Модель: " + currentProduct.Model;
            tbPrice.Text = "Цена: " + currentProduct.Price + " ₽";
            tbCategory.Text = currentProduct.Categories.CategoryName;
            tbBrand.Text = currentProduct.Brands.BrandName;
            tbStatus.Text = currentProduct.ProductStatuses.StatusName;
            tbQuantity.Text = "Остаток: " + currentProduct.QuantityInStock;
            tbWarranty.Text = "Гарантия: " + currentProduct.WarrantyMonths + " мес.";
            tbComputerType.Text = "Тип: " + currentProduct.ComputerType;
            tbCpu.Text = "CPU: " + currentProduct.CpuInfo;
            tbGpu.Text = "GPU: " + currentProduct.GpuInfo;
            tbRam.Text = "RAM: " + currentProduct.RamInfo;
            tbStorage.Text = "Накопитель: " + currentProduct.StorageInfo;
            tbDescription.Text = currentProduct.Description;
            tbSpecifications.Text = currentProduct.Specifications;

            LoadMainImage();
            LoadAdditionalImages();
        }

        private void LoadMainImage()
        {
            if (string.IsNullOrWhiteSpace(currentProduct.MainImage))
                return;

            string imagePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                currentProduct.MainImage);

            if (File.Exists(imagePath))
            {
                imgMain.Source = new BitmapImage(new Uri(imagePath));
            }
        }

        private void LoadAdditionalImages()
        {
            List<string> imagePaths = new List<string>();

            if (!string.IsNullOrWhiteSpace(currentProduct.MainImage))
            {
                string mainImagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    currentProduct.MainImage);

                if (File.Exists(mainImagePath))
                {
                    imagePaths.Add(mainImagePath);
                }
            }

            var images = AppConnect.model01.ProductImages
                .Where(x => x.ProductID == currentProduct.ProductID)
                .ToList();

            foreach (var image in images)
            {
                string imagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    image.ImagePath);

                if (File.Exists(imagePath) && !imagePaths.Contains(imagePath))
                {
                    imagePaths.Add(imagePath);
                }
            }

            icImages.ItemsSource = imagePaths;
        }

        private void btnThumbnail_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string selectedImage = button.Tag as string;

            if (string.IsNullOrWhiteSpace(selectedImage))
                return;

            imgMain.Source = new BitmapImage(new Uri(selectedImage));
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
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
                    x.ProductID == currentProduct.ProductID);

            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity + 1 > currentProduct.QuantityInStock)
                {
                    MessageBox.Show("Нельзя добавить больше товара, чем есть на складе");
                    return;
                }

                existingCartItem.Quantity += 1;
            }
            else
            {
                if (currentProduct.QuantityInStock < 1)
                {
                    MessageBox.Show("Товара нет в наличии");
                    return;
                }

                CartItems newCartItem = new CartItems();
                newCartItem.CartID = userCart.CartID;
                newCartItem.ProductID = currentProduct.ProductID;
                newCartItem.Quantity = 1;
                newCartItem.PriceAtMoment = currentProduct.Price;

                AppConnect.model01.CartItems.Add(newCartItem);
            }

            AppConnect.model01.SaveChanges();
            MessageBox.Show("Товар добавлен в корзину");
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }
    }
}
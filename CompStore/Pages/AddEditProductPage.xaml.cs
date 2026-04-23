using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CompStore.ApplicationData;

namespace CompStore.Pages
{
    public partial class AddEditProductPage : Page
    {
        private Products currentProduct;
        private List<string> additionalImages = new List<string>();

        public AddEditProductPage(Products product)
        {
            InitializeComponent();
            LoadComboBoxes();

            if (product == null)
            {
                currentProduct = new Products();
                tbTitle.Text = "Добавление товара";
            }
            else
            {
                currentProduct = product;
                tbTitle.Text = "Редактирование товара";

                txtName.Text = currentProduct.ProductName;
                txtModel.Text = currentProduct.Model;
                txtDescription.Text = currentProduct.Description;
                txtSpecifications.Text = currentProduct.Specifications;
                txtComputerType.Text = currentProduct.ComputerType;
                txtCpuInfo.Text = currentProduct.CpuInfo;
                txtGpuInfo.Text = currentProduct.GpuInfo;
                txtRamInfo.Text = currentProduct.RamInfo;
                txtStorageInfo.Text = currentProduct.StorageInfo;
                txtPrice.Text = currentProduct.Price.ToString();
                txtQuantity.Text = currentProduct.QuantityInStock.ToString();
                txtWarranty.Text = currentProduct.WarrantyMonths.ToString();

                cmbCategory.SelectedValue = currentProduct.CategoryID;
                cmbBrand.SelectedValue = currentProduct.BrandID;
                cmbStatus.SelectedValue = currentProduct.StatusID;

                txtImage.Text = currentProduct.MainImage;
                LoadPreviewImage(currentProduct.MainImage);
                LoadAdditionalImages();
            }
        }

        private void LoadComboBoxes()
        {
            cmbCategory.ItemsSource = AppConnect.model01.Categories.ToList();
            cmbCategory.DisplayMemberPath = "CategoryName";
            cmbCategory.SelectedValuePath = "CategoryID";

            cmbBrand.ItemsSource = AppConnect.model01.Brands.ToList();
            cmbBrand.DisplayMemberPath = "BrandName";
            cmbBrand.SelectedValuePath = "BrandID";

            cmbStatus.ItemsSource = AppConnect.model01.ProductStatuses.ToList();
            cmbStatus.DisplayMemberPath = "StatusName";
            cmbStatus.SelectedValuePath = "StatusID";
        }

        private void LoadPreviewImage(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return;

            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imageName);

            if (File.Exists(imagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgPreview.Source = bitmap;
            }
        }

        private void LoadAdditionalImages()
        {
            additionalImages = AppConnect.model01.ProductImages
                .Where(x => x.ProductID == currentProduct.ProductID)
                .Select(x => x.ImagePath)
                .ToList();

            lbAdditionalImages.ItemsSource = null;
            lbAdditionalImages.ItemsSource = additionalImages;
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                string fileName = Path.GetFileName(dialog.FileName);
                string destinationPath = Path.Combine(imagesFolder, fileName);

                File.Copy(dialog.FileName, destinationPath, true);

                txtImage.Text = fileName;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(destinationPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgPreview.Source = bitmap;
            }
        }

        private void btnAddAdditionalImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                string fileName = Path.GetFileName(dialog.FileName);

                if (additionalImages.Contains(fileName))
                {
                    MessageBox.Show("Это изображение уже добавлено");
                    return;
                }

                string destinationPath = Path.Combine(imagesFolder, fileName);

                File.Copy(dialog.FileName, destinationPath, true);

                additionalImages.Add(fileName);

                lbAdditionalImages.ItemsSource = null;
                lbAdditionalImages.ItemsSource = additionalImages;
            }
        }

        private void btnRemoveAdditionalImage_Click(object sender, RoutedEventArgs e)
        {
            string selectedImage = lbAdditionalImages.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedImage))
            {
                MessageBox.Show("Выберите изображение");
                return;
            }

            additionalImages.Remove(selectedImage);

            lbAdditionalImages.ItemsSource = null;
            lbAdditionalImages.ItemsSource = additionalImages;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string productName = txtName.Text.Trim();
            string model = txtModel.Text.Trim();
            string description = txtDescription.Text.Trim();
            string specifications = txtSpecifications.Text.Trim();
            string computerType = txtComputerType.Text.Trim();
            string cpuInfo = txtCpuInfo.Text.Trim();
            string gpuInfo = txtGpuInfo.Text.Trim();
            string ramInfo = txtRamInfo.Text.Trim();
            string storageInfo = txtStorageInfo.Text.Trim();
            string priceText = txtPrice.Text.Trim().Replace(".", ",");
            string quantityText = txtQuantity.Text.Trim();
            string warrantyText = txtWarranty.Text.Trim();
            string imageName = txtImage.Text.Trim();

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Введите название товара");
                txtName.Focus();
                return;
            }

            if (productName.Length < 2 || productName.Length > 150)
            {
                MessageBox.Show("Название товара должно быть от 2 до 150 символов");
                txtName.Focus();
                return;
            }

            if (model.Length > 100)
            {
                MessageBox.Show("Модель слишком длинная");
                txtModel.Focus();
                return;
            }

            if (description.Length > 1000)
            {
                MessageBox.Show("Описание слишком длинное");
                txtDescription.Focus();
                return;
            }

            if (specifications.Length > 1000)
            {
                MessageBox.Show("Характеристики слишком длинные");
                txtSpecifications.Focus();
                return;
            }

            if (computerType.Length > 100)
            {
                MessageBox.Show("Тип товара слишком длинный");
                txtComputerType.Focus();
                return;
            }

            if (cpuInfo.Length > 200)
            {
                MessageBox.Show("Поле CPU слишком длинное");
                txtCpuInfo.Focus();
                return;
            }

            if (gpuInfo.Length > 200)
            {
                MessageBox.Show("Поле GPU слишком длинное");
                txtGpuInfo.Focus();
                return;
            }

            if (ramInfo.Length > 200)
            {
                MessageBox.Show("Поле RAM слишком длинное");
                txtRamInfo.Focus();
                return;
            }

            if (storageInfo.Length > 200)
            {
                MessageBox.Show("Поле накопителя слишком длинное");
                txtStorageInfo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(priceText))
            {
                MessageBox.Show("Введите цену");
                txtPrice.Focus();
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price))
            {
                MessageBox.Show("Цена введена неверно");
                txtPrice.Focus();
                return;
            }

            if (price <= 0 || price > 99999999)
            {
                MessageBox.Show("Цена должна быть больше 0 и не слишком большой");
                txtPrice.Focus();
                return;
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity < 0 || quantity > 999999)
            {
                MessageBox.Show("Количество введено неверно");
                txtQuantity.Focus();
                return;
            }

            int warranty = 0;
            if (!string.IsNullOrWhiteSpace(warrantyText))
            {
                if (!int.TryParse(warrantyText, out warranty) || warranty < 0 || warranty > 120)
                {
                    MessageBox.Show("Гарантия должна быть от 0 до 120 месяцев");
                    txtWarranty.Focus();
                    return;
                }
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию");
                return;
            }

            if (cmbBrand.SelectedValue == null)
            {
                MessageBox.Show("Выберите бренд");
                return;
            }

            if (cmbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус");
                return;
            }

            if (string.IsNullOrWhiteSpace(imageName))
            {
                MessageBox.Show("Выберите основное изображение");
                return;
            }

            if (imageName.Length > 255)
            {
                MessageBox.Show("Имя файла изображения слишком длинное");
                return;
            }

            currentProduct.ProductName = productName;
            currentProduct.Model = string.IsNullOrWhiteSpace(model) ? null : model;
            currentProduct.Description = string.IsNullOrWhiteSpace(description) ? null : description;
            currentProduct.Specifications = string.IsNullOrWhiteSpace(specifications) ? null : specifications;
            currentProduct.ComputerType = string.IsNullOrWhiteSpace(computerType) ? null : computerType;
            currentProduct.CpuInfo = string.IsNullOrWhiteSpace(cpuInfo) ? null : cpuInfo;
            currentProduct.GpuInfo = string.IsNullOrWhiteSpace(gpuInfo) ? null : gpuInfo;
            currentProduct.RamInfo = string.IsNullOrWhiteSpace(ramInfo) ? null : ramInfo;
            currentProduct.StorageInfo = string.IsNullOrWhiteSpace(storageInfo) ? null : storageInfo;
            currentProduct.Price = price;
            currentProduct.QuantityInStock = quantity;
            currentProduct.WarrantyMonths = warranty;
            currentProduct.CategoryID = (int)cmbCategory.SelectedValue;
            currentProduct.BrandID = (int)cmbBrand.SelectedValue;
            currentProduct.StatusID = (int)cmbStatus.SelectedValue;
            currentProduct.MainImage = imageName;

            try
            {
                if (currentProduct.ProductID == 0)
                {
                    AppConnect.model01.Products.Add(currentProduct);
                }

                AppConnect.model01.SaveChanges();

                var oldImages = AppConnect.model01.ProductImages
                    .Where(x => x.ProductID == currentProduct.ProductID)
                    .ToList();

                foreach (var oldImage in oldImages)
                {
                    AppConnect.model01.ProductImages.Remove(oldImage);
                }

                foreach (var additionalImageName in additionalImages)
                {
                    if (string.IsNullOrWhiteSpace(additionalImageName))
                        continue;

                    if (additionalImageName.Length > 255)
                        continue;

                    ProductImages newImage = new ProductImages();
                    newImage.ProductID = currentProduct.ProductID;
                    newImage.ImagePath = additionalImageName;

                    AppConnect.model01.ProductImages.Add(newImage);
                }

                AppConnect.model01.SaveChanges();

                MessageBox.Show("Товар сохранен");
                AppFrame.frmMain.GoBack();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var entityErrors in ex.EntityValidationErrors)
                {
                    sb.AppendLine("Сущность: " + entityErrors.Entry.Entity.GetType().Name);

                    foreach (var validationError in entityErrors.ValidationErrors)
                    {
                        sb.AppendLine("Поле: " + validationError.PropertyName);
                        sb.AppendLine("Ошибка: " + validationError.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                MessageBox.Show(sb.ToString(), "Ошибка валидации");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CompStore.ApplicationData;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;

namespace CompStore.Pages
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            LoadCartItems();
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

        private void LoadCartItems()
        {
            var userCart = AppConnect.model01.Carts
                .FirstOrDefault(x => x.UserID == AppConnect.CurrentUser.UserID);

            if (userCart == null)
            {
                MessageBox.Show("Корзина пользователя не найдена");
                return;
            }

            var cartItems = AppConnect.model01.CartItems
                .Where(x => x.CartID == userCart.CartID)
                .ToList();

            List<CartDisplayItem> displayItems = new List<CartDisplayItem>();

            foreach (var item in cartItems)
            {
                displayItems.Add(new CartDisplayItem
                {
                    CartItemID = item.CartItemID,
                    ProductID = item.ProductID,
                    ProductName = item.Products.ProductName,
                    Model = item.Products.Model,
                    Quantity = item.Quantity,
                    PriceAtMoment = item.PriceAtMoment,
                    TotalPrice = item.Quantity * item.PriceAtMoment,
                    ImagePath = GetImagePath(item.Products.MainImage),
                    SourceCartItem = item
                });
            }

            lvCartItems.ItemsSource = displayItems;

            decimal total = displayItems.Sum(x => x.TotalPrice);
            tbTotal.Text = "Итог: " + total + " ₽";
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            CartDisplayItem selectedItem = button.Tag as CartDisplayItem;

            if (selectedItem == null)
                return;

            AppConnect.model01.CartItems.Remove(selectedItem.SourceCartItem);
            AppConnect.model01.SaveChanges();

            MessageBox.Show("Товар удален из корзины");
            LoadCartItems();
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            CartDisplayItem selectedItem = button.Tag as CartDisplayItem;

            if (selectedItem == null)
                return;

            var cartItem = AppConnect.model01.CartItems
                .FirstOrDefault(x => x.CartItemID == selectedItem.CartItemID);

            if (cartItem == null)
                return;

            cartItem.Quantity -= 1;

            if (cartItem.Quantity <= 0)
            {
                AppConnect.model01.CartItems.Remove(cartItem);
            }

            AppConnect.model01.SaveChanges();
            LoadCartItems();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            CartDisplayItem selectedItem = button.Tag as CartDisplayItem;

            if (selectedItem == null)
                return;

            var cartItem = AppConnect.model01.CartItems
                .FirstOrDefault(x => x.CartItemID == selectedItem.CartItemID);

            if (cartItem == null)
                return;

            Products product = AppConnect.model01.Products
                .FirstOrDefault(x => x.ProductID == cartItem.ProductID);

            if (product == null)
                return;

            if (cartItem.Quantity + 1 > product.QuantityInStock)
            {
                MessageBox.Show("Нельзя добавить больше товара, чем есть на складе");
                return;
            }

            cartItem.Quantity += 1;

            AppConnect.model01.SaveChanges();
            LoadCartItems();
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            var userCart = AppConnect.model01.Carts
                .FirstOrDefault(x => x.UserID == AppConnect.CurrentUser.UserID);

            if (userCart == null)
            {
                MessageBox.Show("Корзина не найдена");
                return;
            }

            var cartItems = AppConnect.model01.CartItems
                .Where(x => x.CartID == userCart.CartID)
                .ToList();

            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Очистить корзину полностью?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            foreach (var item in cartItems)
            {
                AppConnect.model01.CartItems.Remove(item);
            }

            AppConnect.model01.SaveChanges();
            LoadCartItems();

            MessageBox.Show("Корзина очищена");
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            var userCart = AppConnect.model01.Carts
                .FirstOrDefault(x => x.UserID == AppConnect.CurrentUser.UserID);

            if (userCart == null)
            {
                MessageBox.Show("Корзина не найдена");
                return;
            }

            var cartItems = AppConnect.model01.CartItems
                .Where(x => x.CartID == userCart.CartID)
                .ToList();

            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            foreach (var item in cartItems)
            {
                Products product = AppConnect.model01.Products
                    .FirstOrDefault(x => x.ProductID == item.ProductID);

                if (product == null)
                {
                    MessageBox.Show("Один из товаров не найден");
                    return;
                }

                if (product.QuantityInStock < item.Quantity)
                {
                    MessageBox.Show("Недостаточно товара на складе: " + product.ProductName);
                    return;
                }
            }

            decimal totalAmount = cartItems.Sum(x => x.Quantity * x.PriceAtMoment);

            Orders newOrder = new Orders();
            newOrder.UserID = AppConnect.CurrentUser.UserID;
            newOrder.OrderDate = DateTime.Now;
            newOrder.TotalAmount = totalAmount;
            newOrder.OrderStatusID = 1;

            AppConnect.model01.Orders.Add(newOrder);
            AppConnect.model01.SaveChanges();

            foreach (var item in cartItems)
            {
                OrderItems newOrderItem = new OrderItems();
                newOrderItem.OrderID = newOrder.OrderID;
                newOrderItem.ProductID = item.ProductID;
                newOrderItem.Quantity = item.Quantity;
                newOrderItem.PriceAtMoment = item.PriceAtMoment;

                AppConnect.model01.OrderItems.Add(newOrderItem);

                Products product = AppConnect.model01.Products
                    .FirstOrDefault(x => x.ProductID == item.ProductID);

                if (product != null)
                {
                    product.QuantityInStock -= item.Quantity;

                    if (product.QuantityInStock < 0)
                    {
                        product.QuantityInStock = 0;
                    }
                }
            }

            string pdfPath = GenerateReceiptPdf(newOrder, cartItems);

            Receipts newReceipt = new Receipts();
            newReceipt.OrderID = newOrder.OrderID;
            newReceipt.PdfPath = pdfPath;
            newReceipt.CreatedAt = DateTime.Now;

            AppConnect.model01.Receipts.Add(newReceipt);

            foreach (var item in cartItems)
            {
                AppConnect.model01.CartItems.Remove(item);
            }

            AppConnect.model01.SaveChanges();

            MessageBox.Show("Заказ успешно оформлен");

            if (File.Exists(pdfPath))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = pdfPath;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }

            LoadCartItems();
        }

        private System.Drawing.Bitmap GenerateReceiptQrBitmap(Orders order)
        {
            string qrText =
                "Чек магазина компьютеров\n" +
                "Заказ № " + order.OrderID + "\n" +
                "Дата: " + order.OrderDate.ToString("dd.MM.yyyy HH:mm") + "\n" +
                "Покупатель: " + AppConnect.CurrentUser.FullName + "\n" +
                "Сумма: " + order.TotalAmount + " ₽";

            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(
                qrText,
                QRCoder.QRCodeGenerator.ECCLevel.Q);

            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);

            return qrCode.GetGraphic(10);
        }

        private string GenerateReceiptPdf(Orders order, List<CartItems> cartItems)
        {
            string receiptsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Receipts");

            if (!Directory.Exists(receiptsFolder))
            {
                Directory.CreateDirectory(receiptsFolder);
            }

            string fileName = "receipt_" + order.OrderID + ".pdf";
            string fullPath = Path.Combine(receiptsFolder, fileName);

            string fontPath = @"C:\Windows\Fonts\arial.ttf";

            iTextSharp.text.pdf.BaseFont baseFont =
                iTextSharp.text.pdf.BaseFont.CreateFont(
                    fontPath,
                    iTextSharp.text.pdf.BaseFont.IDENTITY_H,
                    iTextSharp.text.pdf.BaseFont.EMBEDDED);

            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 12);
            iTextSharp.text.Font boldFont = new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.BOLD);

            iTextSharp.text.Document document = new iTextSharp.text.Document();
            iTextSharp.text.pdf.PdfWriter.GetInstance(
                document,
                new FileStream(fullPath, FileMode.Create));

            document.Open();

            document.Add(new iTextSharp.text.Paragraph("Чек магазина компьютеров", boldFont));
            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));
            document.Add(new iTextSharp.text.Paragraph("Номер заказа: " + order.OrderID, normalFont));
            document.Add(new iTextSharp.text.Paragraph("Дата заказа: " + order.OrderDate.ToString("dd.MM.yyyy HH:mm"), normalFont));
            document.Add(new iTextSharp.text.Paragraph("Покупатель: " + AppConnect.CurrentUser.FullName, normalFont));
            document.Add(new iTextSharp.text.Paragraph("Логин: " + AppConnect.CurrentUser.Login, normalFont));
            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));

            document.Add(new iTextSharp.text.Paragraph("Состав заказа:", boldFont));
            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));

            foreach (var item in cartItems)
            {
                string line =
                    item.Products.ProductName +
                    " | Кол-во: " + item.Quantity +
                    " | Цена: " + item.PriceAtMoment +
                    " | Сумма: " + (item.Quantity * item.PriceAtMoment);

                document.Add(new iTextSharp.text.Paragraph(line, normalFont));
            }

            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));
            document.Add(new iTextSharp.text.Paragraph("Итоговая сумма: " + order.TotalAmount + " ₽", boldFont));
            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));
            document.Add(new iTextSharp.text.Paragraph("QR-код чека:", boldFont));
            document.Add(new iTextSharp.text.Paragraph(" ", normalFont));

            using (System.Drawing.Bitmap qrBitmap = GenerateReceiptQrBitmap(order))
            {
                using (MemoryStream qrStream = new MemoryStream())
                {
                    qrBitmap.Save(qrStream, ImageFormat.Png);
                    iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(qrStream.ToArray());
                    qrImage.ScaleToFit(150f, 150f);
                    qrImage.Alignment = iTextSharp.text.Image.ALIGN_LEFT;

                    document.Add(qrImage);
                }
            }

            document.Close();

            return fullPath;
        }
    }

    public class CartDisplayItem
    {
        public int CartItemID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtMoment { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImagePath { get; set; }
        public CartItems SourceCartItem { get; set; }
    }
}
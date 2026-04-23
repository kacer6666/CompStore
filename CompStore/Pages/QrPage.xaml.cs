using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CompStore.ApplicationData;
using QRCoder;

namespace CompStore.Pages
{
    public partial class QrPage : Page
    {
        private TelegramLinks activeTelegramLink;

        public QrPage()
        {
            InitializeComponent();
            LoadTelegramData();
        }

        private void LoadTelegramData()
        {
            activeTelegramLink = AppConnect.model01.TelegramLinks
                .FirstOrDefault(x => x.IsActive == true);

            if (activeTelegramLink == null)
            {
                MessageBox.Show("Активная Telegram-ссылка не найдена");
                return;
            }

            tbTelegramLink.Text = activeTelegramLink.Url;
            imgQr.Source = GenerateQrImage(activeTelegramLink.Url);
        }

        private BitmapImage GenerateQrImage(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            using (MemoryStream memory = new MemoryStream(qrCodeBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void btnOpenTelegram_Click(object sender, RoutedEventArgs e)
        {
            if (activeTelegramLink == null)
            {
                MessageBox.Show("Ссылка не найдена");
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = activeTelegramLink.Url;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frmMain.GoBack();
        }
    }
}
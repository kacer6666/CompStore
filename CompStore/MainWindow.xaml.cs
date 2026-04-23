using System.Windows;
using CompStore.ApplicationData;
using CompStore.Pages;

namespace CompStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppFrame.frmMain = frmMain;
            frmMain.Navigate(new AuthorizationPage());
        }
    }
}
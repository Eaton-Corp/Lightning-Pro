using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for PrintShopPackagePage.xaml
    /// </summary>
    public partial class PrintShopPackagePage : Window
    {

        BitmapImage CurrentPage;

        public PrintShopPackagePage(BitmapImage img)
        {
            InitializeComponent();

            CurrentPage = img;
            DetermineOrientation();
            Page.Source = CurrentPage;
        }

        private void DetermineOrientation() 
        {
            if (CurrentPage.Width > CurrentPage.Height) 
            {
                //rotate BitmapImage 90 deg 
                Bitmap page = Utility.BitmapImage2Bitmap(CurrentPage);
                page.RotateFlip(RotateFlipType.Rotate90FlipNone);
                CurrentPage = Utility.ToBitmapImage(page);
            }
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.PrintDialog myPrintDialog = new System.Windows.Controls.PrintDialog();
            int Margin = 12;   //adjust margin here
            if (myPrintDialog.ShowDialog() == true)
            {
                System.Windows.Size pageSize = new System.Windows.Size(myPrintDialog.PrintableAreaWidth - Margin, myPrintDialog.PrintableAreaHeight - Margin);
                Page.Measure(pageSize);
                Page.Arrange(new Rect(Margin, Margin, pageSize.Width, pageSize.Height));
                myPrintDialog.PrintVisual(Page, "Image Print");
                Close();
            }
        }

    }
}
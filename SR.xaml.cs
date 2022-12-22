using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for SR.xaml
    /// </summary>
    public partial class SR : Window
    {
        int Id;
        string CommitDate = "";
        string ProductSpecialist = "";
        Utility.ProductGroup CurrentProduct;


        public SR(int ID, Utility.ProductGroup Product)
        {
            InitializeComponent();
            
            Id = ID;
            CurrentProduct = Product;

            string reportSourceSR = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "SR.png";
            Uri image = new Uri(reportSourceSR, UriKind.RelativeOrAbsolute);
            BitmapImage img = new BitmapImage(image);

            string GO = Utility.IDtoGO(Id,CurrentProduct);
            getCommitAndSpecialist();

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(img, new Rect(0, 0, img.Width, img.Height));

            BitmapImage QRCode = null;
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                QRCode = Utility.GenerateQRCode(GO);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                QRCode = Utility.GenerateQRCode("4-" + GO);
            }

            drawingContext.DrawImage(QRCode, new Rect(1200, 1650, QRCode.Width/2, QRCode.Height/2));

            drawingContext.DrawText(StringToText("Product Specialist:"), new System.Windows.Point(200, 115));
            drawingContext.DrawText(StringToText(ProductSpecialist), new System.Windows.Point(550, 115));
            
            drawingContext.DrawText(StringToText("Commit Date:"), new System.Windows.Point(720, 115));
            drawingContext.DrawText(StringToText(CommitDate), new System.Windows.Point(1000, 115));
            

            if(CurrentProduct == Utility.ProductGroup.PRL123)
            {
                getGOs("select [GO_Item] from [PRL123] where [GO]='" + GO + "' and Tracking='Production'", drawingContext);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                getGOs("select [GO_Item] from [PRL4] where [GO]='" + GO + "' and Tracking='Production' AND [PageNumber]=0", drawingContext);
            }

            drawingContext.Close();

            RenderTargetBitmap renderBmap = new RenderTargetBitmap(img.PixelWidth, img.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            renderBmap.Render(drawingVisual);

            Report.Source = renderBmap;
        }


      
        private void getCommitAndSpecialist()
        {
            DataTable dt;
            string query = "";

            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "select [CommitDate], [ProductSpecialist] from [PRL123] where [ID]=" + Id.ToString();
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                query = "select [CommitDate], [ProductSpecialist] from [PRL4] where [ID]=" + Id.ToString();
            }
            dt = Utility.SearchLP(query);
            using (DataTableReader dtr = new DataTableReader(dt))
            {
                while (dtr.Read())
                {
                    CommitDate = dtr[0].ToString();
                    ProductSpecialist = dtr[1].ToString();
                }
            } //end using reader
        }


        private FormattedText StringToText(string input)
        {
            return new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 36, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        private void getGOs(string query, DrawingContext drawingContext)
        {
            int y = 280;

            DataTable dt;
            dt = Utility.SearchLP(query);

            using (DataTableReader dtr = new DataTableReader(dt))
            {
                while (dtr.Read())
                {
                    drawingContext.DrawText(StringToText(dtr[0].ToString()), new System.Windows.Point(220, y));
                    y += 68;
                }
            } //end using reader
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog myPrintDialog = new PrintDialog();
            int Margin = 12;   //adjust margin here
            if (myPrintDialog.ShowDialog() == true)
            {
                System.Windows.Size pageSize = new System.Windows.Size(myPrintDialog.PrintableAreaWidth - Margin, myPrintDialog.PrintableAreaHeight - Margin);
                Report.Measure(pageSize);
                Report.Arrange(new Rect(Margin, Margin, pageSize.Width, pageSize.Height));
                myPrintDialog.PrintVisual(Report, "Image Print");
                Close();
            }
        }

    }
}

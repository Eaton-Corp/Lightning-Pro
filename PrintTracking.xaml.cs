using System;
using System.Collections.Generic;
using System.Globalization;
using System.Configuration;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for PrintTracking.xaml
    /// </summary>
    public partial class PrintTracking : Window
    {
        readonly string[] Boxes;
        readonly string[] GOItems;
        public PrintTracking(string[] catalgoue, string[] GOs)
        {
            InitializeComponent();
            Boxes = catalgoue;
            GOItems = GOs;

            string reportSourceSR = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "BoxEarly.png";
            Uri image = new Uri(reportSourceSR, UriKind.RelativeOrAbsolute);
            BitmapImage img = new BitmapImage(image);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(img, new Rect(0, 0, img.Width, img.Height));
            
            for (int i = 0; i < Boxes.Length; i++)
            {
                drawingContext.DrawText(StringToText(GOItems[i].ToString()), new System.Windows.Point(200, 400 + 70 * i));
                drawingContext.DrawText(StringToText(Boxes[i].ToString()), new System.Windows.Point(600, 400 + 70 * i));
                drawingContext.DrawText(StringToText(Description(Boxes[i].ToString())), new System.Windows.Point(900, 400 + 70 * i));
            }

            drawingContext.Close();

            RenderTargetBitmap renderBmap = new RenderTargetBitmap(img.PixelWidth, img.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            renderBmap.Render(drawingVisual);

            Report.Source = renderBmap;
        }

        private string Description(string Enc)
        {
            if (Enc.Contains("EZB2030RC")) return "30”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2036RC")) return "36”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2042RC")) return "42”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2048RC")) return "48”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2054RC")) return "54”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2060RC")) return "60”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2072RC")) return "72”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2090RC")) return "90”H Galv Type 1 EZ Tub";
            else if (Enc.Contains("EZB2030RCSP")) return "30”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2036RCSP")) return "36”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2042RCSP")) return "42”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2048RCSP")) return "48”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2054RCSP")) return "54”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2060RCSP")) return "60”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2072RCSP")) return "72”H Galv Type 1 EZ Tub with Driphood";
            else if (Enc.Contains("EZB2090RCSP")) return "90”H Galv Type 1 EZ Tub with Driphood";
            else return "null";
        }

        private FormattedText StringToText(string input)
        {
            return new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 36, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog myPrintDialog = new PrintDialog();
            if (myPrintDialog.ShowDialog() == true)
            {
                myPrintDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                myPrintDialog.PrintVisual(Report, "Bidman Print");
            }
        }

    }
}

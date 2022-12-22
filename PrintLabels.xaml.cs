using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
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
using QRCoder;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for PrintLabels.xaml
    /// </summary>
    public partial class PrintLabels : Window
    {
        List<string> GO;
        Utility.ProductGroup CurrentProduct;

        System.Windows.Controls.Image im;

        public PrintLabels(List<string> GOs, Utility.ProductGroup Product)
        {
            InitializeComponent();

            GO = GOs;
            CurrentProduct = Product;

            PrintDialog myPrintDialog = new PrintDialog();
            if (myPrintDialog.ShowDialog() == true)
            {
                for (var i = 0; i < GO.Count; i++)
                {
                    CSALabel.Source = LabelRender(GO[i]);
                    myPrintDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                    myPrintDialog.PrintVisual(im, "Image Print");
                }
            }
        }

        private RenderTargetBitmap LabelRender(string GOItem)
        {
            string GONumber = "";
            string enc = "";
            string ItemNumber = "";
            string Designation = "";
            string MA = "";
            string Voltage = "";
            string P = "";
            string W = "";
            string Ground = "";
            string Hertz = "";
            string ProductID = "";
            string GO_Item = "";
            string XSpace = "";
            string N = "";

            string Type = "Type:";
            string Date = "Date:";
            string Enclosure = "Enclosure/Boitier Type:";
            string Main = "Main/Omnibus:";
            string Neut = "Neut:";
            string made = "Made At/Fait Au:";
            string system = "System(e):";
            string Order = "Order (Job)/Ordre (Ref) #:";
            string ph = "Ph:";
            string WiFil = "Wi/Fil:";
            string Hz = "Hz:";
            string Catalogue = "Catalogue/Le Catalogue #";
            string spaces = "Ccts or X Spaces/Ccts ou X Espaces";
            string trip1 = "BREAKER IS TRIPPED WHEN HANDLE IS MIDWAY BETWEEN 'ON' AND 'OFF'. TO RESET, MOVE HANDLE TO 'OFF', THEN TO 'ON'.";
            string trip2 = "LE DISJONCTEUR EST DECLENCHE QUAND LE MANCHON EST A MI-CHEMIN ENTRE LA POSITION OUVERT ET LA POSITION ";
            string trip3 = "FERME. POUR REFERMER, POUSSER LE MANCHON A LA POSITION 'OVERT' ET ENSUITE 'FERME'.";
            string trip4 = "239P058H01 REV #10";

            DataTableReader values = Utility.getCSAValues(GOItem, CurrentProduct);

            using (values)
            {
                while (values.Read())
                {
                    GONumber = values[1].ToString();
                    ItemNumber = values[2].ToString();
                    Designation = values[3].ToString();
                    enc = values[4].ToString();
                    N = values[5].ToString();
                    XSpace = values[6].ToString();
                    MA = values[7].ToString();
                    Voltage = values[8].ToString();
                    P = values[9].ToString();
                    W = values[10].ToString();
                    Ground = values[11].ToString();
                    Hertz = values[12].ToString();
                    GO_Item = values[13].ToString();
                    ProductID = values[14].ToString();
                }
            }


            string csaLabelSource = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "CSA LABEL.png";
            Uri image = new Uri(csaLabelSource, UriKind.RelativeOrAbsolute);
            BitmapImage img = new BitmapImage(image);


            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(img, new Rect(0, 0, img.PixelWidth, img.PixelHeight));

            BitmapImage QRCode = null;
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                QRCode = Utility.GenerateQRCode(GOItem);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                QRCode = Utility.GenerateQRCode("4-" + GOItem);
            }

            drawingContext.DrawImage(QRCode, new Rect(430, 0, 70, 70));

            drawingContext.DrawText(StringToText(Type), new System.Windows.Point(0, 40));
            drawingContext.DrawText(StringToText(Enclosure), new System.Windows.Point(95, 40));
            drawingContext.DrawText(StringToText(Main), new System.Windows.Point(270, 40));
            drawingContext.DrawText(StringToText(Neut), new System.Windows.Point(375, 40));
            drawingContext.DrawText(StringToText(Date), new System.Windows.Point(0, 55));
            drawingContext.DrawText(StringToText(made), new System.Windows.Point(120, 55));

            string location = ConfigurationManager.ConnectionStrings["Location"].ToString();
            drawingContext.DrawText(StringToText(location), new System.Windows.Point(210, 55));

            drawingContext.DrawText(StringToText(system), new System.Windows.Point(280, 55));
            drawingContext.DrawText(StringToText(Order), new System.Windows.Point(0, 70));
            drawingContext.DrawText(StringToText(ph), new System.Windows.Point(290, 70));
            drawingContext.DrawText(StringToText(WiFil), new System.Windows.Point(370, 70));
            drawingContext.DrawText(StringToText(Hz), new System.Windows.Point(470, 70));
            drawingContext.DrawText(StringToText(Catalogue), new System.Windows.Point(0, 85));
            drawingContext.DrawText(StringToText(spaces), new System.Windows.Point(285, 85));
            drawingContext.DrawText(StringToText(trip1), new System.Windows.Point(0, 100));
            drawingContext.DrawText(StringToText(trip2), new System.Windows.Point(0, 115));
            drawingContext.DrawText(StringToText(trip3), new System.Windows.Point(0, 130));
            drawingContext.DrawText(StringToText(trip4), new System.Windows.Point(0, 145));

            drawingContext.DrawText(StringToText(DateTime.Now.ToString().Substring(0, 10)), new System.Windows.Point(25, 55));

            drawingContext.DrawText(StringToText(ProductID), new System.Windows.Point(25, 40));
            drawingContext.DrawText(StringToText(enc), new System.Windows.Point(190, 40));
            drawingContext.DrawText(StringToText(MA), new System.Windows.Point(340, 40));
            drawingContext.DrawText(StringToText(Voltage), new System.Windows.Point(340, 55));
            drawingContext.DrawText(StringToText(P), new System.Windows.Point(320, 70));
            drawingContext.DrawText(StringToText(W), new System.Windows.Point(430, 70));
            drawingContext.DrawText(StringToText(Hertz), new System.Windows.Point(490, 70));
            drawingContext.DrawText(StringToText(GO_Item), new System.Windows.Point(120, 70));
            drawingContext.DrawText(StringToText(Designation), new System.Windows.Point(120, 85));
            drawingContext.DrawText(StringToText(XSpace), new System.Windows.Point(460, 85));
            drawingContext.DrawText(StringToText(N), new System.Windows.Point(410, 40));

            drawingContext.DrawText(TitleToText("PANELBOARD / PANNEAU DE DISTRIBUTION"), new System.Windows.Point(100, 10));
            drawingContext.Close();

            RenderTargetBitmap renderBmap = new RenderTargetBitmap(img.PixelWidth * 8, img.PixelHeight * 8, 768, 768, PixelFormats.Pbgra32);
            renderBmap.Render(drawingVisual);

            im = new System.Windows.Controls.Image();
            im.Source = renderBmap;
            im.Width = img.Width - 20;
            im.Height = img.Height;
            im.HorizontalAlignment = HorizontalAlignment.Left;
            im.VerticalAlignment = VerticalAlignment.Top;

            return renderBmap;
        }

        private FormattedText StringToText(string input)
        {
            return new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        private FormattedText TitleToText(string input)
        {
            FormattedText formattedText = new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 14, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return formattedText;
        }

    }
}

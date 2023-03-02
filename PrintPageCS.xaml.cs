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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QRCoder;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for PrintPage.xaml
    /// </summary>
    public partial class PrintPageCS : Window
    {
        string GO;
        System.Windows.Controls.Image im;
        Utility.ProductGroup CurrentProduct;

        public PrintPageCS(string GO_Item, Utility.ProductGroup Product)
        {
            InitializeComponent();

            GO = GO_Item;
            CurrentProduct = Product;

            CSALabel.Source = LabelRender(GO);
        }

        private void Click_Print(object sender, RoutedEventArgs e)
        {
            PrintDialog myPrintDialog = new PrintDialog();
            if (myPrintDialog.ShowDialog() == true)
            {
                myPrintDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                myPrintDialog.PrintVisual(im, "Image Print");
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


            string csaLabelSource = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "CS_CSALABEL.png";
            Uri image = new Uri(csaLabelSource, UriKind.RelativeOrAbsolute);
            BitmapImage img = new BitmapImage(image);


            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(img, new Rect(0, 0, img.PixelWidth, img.PixelHeight));

            BitmapImage QRCode = null;
            
            QRCode = Utility.GenerateQRCode(GOItem);
            drawingContext.DrawImage(QRCode, new Rect(430, 0, 70, 70));

            string A = "800 Amps";
            string V = "208Y/120V";
            string T = "60 Hz";
            string Sym = "65 Ka";
            string GO = "CSBI139504-002-22";
            string type = "PRL-CS";
            string enclosure_type = "TYPE 1 SPRINKLERED";


            //if switchboard == unit
            /*drawingContext.DrawText(ToText("SWITCHBOARD UNIT", 20), new System.Windows.Point(100, 120));
            drawingContext.DrawText(ToText("SOUS STATION", 20), new System.Windows.Point(100, 150));*/
            
            //else
            drawingContext.DrawText(ToText("SWITCHBOARD METER CENTER", 18), new System.Windows.Point(30, 120));
            drawingContext.DrawText(ToText("TABLEAUX DE COMMUTATION ET DE", 18), new System.Windows.Point(25, 150));
            drawingContext.DrawText(ToText("MESURAGE", 18), new System.Windows.Point(25, 180));




            //if CSAStandard == 244
            drawingContext.DrawText(ToText("CSA Standard C22.2 No.244/SERIE CSA C22.2 No.244", 12), new System.Windows.Point(20, 210));
            //if CSAStandard == 229
            /*drawingContext.DrawText(ToText("CSA Standard C22.2 No.229/SERIE CSA C22.2 No.229", 12), new System.Windows.Point(20, 210));
             * //if CSAStandard == 31
            drawingContext.DrawText(ToText("CSA Standard C22.2 No.31/SERIE CSA C22.2 No.31", 12), new System.Windows.Point(20, 210));*/



            //Pull from Config
            string location = ConfigurationManager.ConnectionStrings["Location"].ToString();
            drawingContext.DrawText(ToText(location, 20), new System.Windows.Point(20, 240));

            //section
            drawingContext.DrawText(ToText("SECTION 1 OF 3", 20), new System.Windows.Point(200, 240));


            drawingContext.DrawText(ToText("MAIN BUS BAR", 14), new System.Windows.Point(15, 280));
            drawingContext.DrawText(ToText("CAPACITY/CAPACITE", 14), new System.Windows.Point(15, 295));

            drawingContext.DrawText(ToText("DE BARRE PRINCIPALE", 14), new System.Windows.Point(15, 310));
            drawingContext.DrawText(ToText("VOLTAGE", 14), new System.Windows.Point(15, 325));

            drawingContext.DrawText(ToText("TENSION", 14), new System.Windows.Point(15, 340));
            drawingContext.DrawText(ToText("SYSTEM", 14), new System.Windows.Point(15, 355));
            drawingContext.DrawText(ToText("SYSTEME", 14), new System.Windows.Point(15, 370));

            drawingContext.DrawText(ToText("SHORT CIRCUIT RATING", 14), new System.Windows.Point(100, 385));
            drawingContext.DrawText(ToText("LA VALEUR DU COURANT DE COURT CIRCUIT", 14), new System.Windows.Point(15, 400));
            drawingContext.DrawText(ToText("SYMMETRICAL", 14), new System.Windows.Point(15, 415));

            drawingContext.DrawText(ToText("SYMMETRICAL", 14), new System.Windows.Point(15, 430));
            drawingContext.DrawText(ToText("AMPS/L'AMPERAGE", 14), new System.Windows.Point(15, 445));
            drawingContext.DrawText(ToText("VOLTS MAX.", 14), new System.Windows.Point(200, 445));
            drawingContext.DrawText(ToText("SYMMETRIQUE", 14), new System.Windows.Point(15, 460));
            drawingContext.DrawText(ToText("TENSION MAX.", 14), new System.Windows.Point(200, 460));




            drawingContext.DrawText(ToText("JOB NO.", 14), new System.Windows.Point(15, 475));
            drawingContext.DrawText(ToText("NO. REF.", 14), new System.Windows.Point(15, 490));
            drawingContext.DrawText(ToText("TYPE", 14), new System.Windows.Point(15, 505));
            drawingContext.DrawText(ToText("DATE", 14), new System.Windows.Point(200, 505));
            drawingContext.DrawText(ToText("ENCLOSURE TYPE", 14), new System.Windows.Point(15, 520));
            drawingContext.DrawText(ToText("ENVELOPPE TYPE", 14), new System.Windows.Point(15, 535));


            //if switchboardUnit
            drawingContext.DrawText(ToText("SWITCHBOARD UNIT", 14), new System.Windows.Point(80, 620));
            drawingContext.DrawText(ToText("TABLEAUX DE CONTROLE", 14), new System.Windows.Point(80, 640));
            drawingContext.DrawText(ToText("LL47168", 14), new System.Windows.Point(80, 660));
            //metereing center
            /*drawingContext.DrawText(ToText("SWITCHING AND METERING CENTER", 14), new System.Windows.Point(80, 620));
            drawingContext.DrawText(ToText("TABLEAUX DE COMMUTATION ET DE", 14), new System.Windows.Point(80, 640));
            drawingContext.DrawText(ToText("MESURAGE", 14), new System.Windows.Point(80, 660));
            drawingContext.DrawText(ToText("LL47167 ", 14), new System.Windows.Point(80, 680));*/









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
            FormattedText formattedText = new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 8, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return formattedText;
        }
        private FormattedText ToText(string input, int font)
        {
            FormattedText formattedText = new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), font, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return formattedText;
        }

        private FormattedText TitleToText(string input)
        {
            FormattedText formattedText = new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 14, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return formattedText;
        }
    }
}
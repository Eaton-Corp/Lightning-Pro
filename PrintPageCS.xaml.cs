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
    /// Interaction logic for PrintPageCS.xaml
    /// </summary>
    public partial class PrintPageCS : Window
    {
        System.Windows.Controls.Image im;
        
        PrintDialog accessPrintDialog;

        string customer;


        //constructor for a single line item

        string GOI;

        public PrintPageCS(string GO_Item, string client)
        {
            InitializeComponent();

            GOI = GO_Item;
            customer = client;

            accessPrintDialog = new PrintDialog();

            if (accessPrintDialog.ShowDialog() == true)
            {
                LabelRender(GOI);
            }
        }



        //constructor to print multiple line items

        List<string> All_GOs;

        public PrintPageCS(List<string> GOs, string client)
        {
            InitializeComponent();

            All_GOs = GOs;
            customer = client;

            accessPrintDialog = new PrintDialog();

            if (accessPrintDialog.ShowDialog() == true)
            {
                for (var i = 0; i < All_GOs.Count; i++)
                {
                    LabelRender(All_GOs[i]);
                }
            }
        }





        private void LabelRender(string GOItem)
        {
            DataTableReader valuesCounter = Utility.getCSAValues(GOItem, Utility.ProductGroup.PRLCS);
            DataTableReader values = Utility.getCSAValues(GOItem, Utility.ProductGroup.PRLCS);

            if (valuesCounter.HasRows == false)
            {
                MessageBox.Show("Unable To Retrieve CS CSA Label Data");
                return;
            }

            var counter = 0;
            int numLabels = 0;

            //using the first DataTableReader
            using (valuesCounter)
            {
                while (valuesCounter.Read())
                {
                    numLabels++;          //get the number or rows/labels that data has
                }
            }


            //initialize fields with the correct size

            string[] GONumber = new string[numLabels];
            string[] ItemNumber = new string[numLabels];
            string[] Switchboard = new string[numLabels];
            string[] CSAStandard = new string[numLabels];
            string[] SMCenter = new string[numLabels];
            string[] Section = new string[numLabels];
            string[] MainBusBar = new string[numLabels];
            string[] Voltage = new string[numLabels];
            string[] Hz = new string[numLabels];
            string[] P = new string[numLabels];
            string[] W = new string[numLabels];
            string[] ShortCircuitRating = new string[numLabels];
            string[] Amps = new string[numLabels];
            string[] Enclosure = new string[numLabels];



            //using the second DataTableReader
            using (values)
            {
                while (values.Read())
                {
                    GONumber[counter] = values[1].ToString();
                    ItemNumber[counter] = values[2].ToString();
                    Switchboard[counter] = values[3].ToString();
                    CSAStandard[counter] = values[4].ToString();
                    SMCenter[counter] = values[5].ToString();
                    Section[counter] = values[6].ToString();
                    MainBusBar[counter] = values[7].ToString();
                    Voltage[counter] = values[8].ToString();
                    Hz[counter] = values[9].ToString();
                    P[counter] = values[10].ToString();
                    W[counter] = values[11].ToString();
                    ShortCircuitRating[counter] = values[12].ToString();
                    Amps[counter] = values[13].ToString();
                    Enclosure[counter] = values[14].ToString();

                    counter++;
                }
            }


            for (var i = 0; i < numLabels; i++)
            {
                string csaLabelSource = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "CS_CSALabel.png";
                Uri image = new Uri(csaLabelSource, UriKind.RelativeOrAbsolute);
                BitmapImage img = new BitmapImage(image);


                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(img, new Rect(0, 0, img.PixelWidth, img.PixelHeight));


                BitmapImage QRCode = Utility.GenerateQRCode("CS-" + GOItem);
                drawingContext.DrawImage(QRCode, new Rect(290, 25, 85, 85));

                string[] TitleLines = Switchboard[i].Split('/');

                if (TitleLines[0].Contains("SWITCHBOARD UNIT"))
                {
                    drawingContext.DrawText(ToText("SWITCHBOARD UNIT", 20), new System.Windows.Point(85, 120));
                    drawingContext.DrawText(ToText("SOUS STATION", 20), new System.Windows.Point(110, 150));
                }
                else if (TitleLines[0].Contains("SWITCHBOARD METER CENTER"))
                {
                    drawingContext.DrawText(ToText("SWITCHBOARD METER CENTER", 18), new System.Windows.Point(50, 120));
                    drawingContext.DrawText(ToText("TABLEAUX DE COMMUTATION ET DE", 18), new System.Windows.Point(25, 150));
                    drawingContext.DrawText(ToText("MESURAGE", 18), new System.Windows.Point(140, 180));
                }
                else if (TitleLines[0].Contains("SWITCHGEAR UNIT"))
                {
                    drawingContext.DrawText(ToText("SWITCHGEAR UNIT", 20), new System.Windows.Point(100, 120));
                    drawingContext.DrawText(ToText("APPAREIL DE COMMUTATION", 20), new System.Windows.Point(40, 150));
                }

                drawingContext.DrawText(ToText(CSAStandard[i], 12), new System.Windows.Point(35, 210));

                //pull location from config
                string location = ConfigurationManager.ConnectionStrings["Location"].ToString();
                drawingContext.DrawText(ToText(location, 20), new System.Windows.Point(20, 240));

                drawingContext.DrawText(ToText(Section[i], 20), new System.Windows.Point(200, 240));

                drawingContext.DrawText(ToText("MAIN BUS BAR", 14), new System.Windows.Point(15, 280));
                drawingContext.DrawText(ToText("CAPACITY/CAPACITÉ", 14), new System.Windows.Point(15, 295));
                drawingContext.DrawText(ToText("DE BARRE PRINCIPALE", 14), new System.Windows.Point(15, 310));
                drawingContext.DrawText(ToText(MainBusBar[i] + " A", 14), new System.Windows.Point(220, 295));


                drawingContext.DrawText(ToText("VOLTAGE", 14), new System.Windows.Point(15, 340));
                drawingContext.DrawText(ToText("TENSION", 14), new System.Windows.Point(15, 355));
                drawingContext.DrawText(ToText(Voltage[i] + "            " + Hz[i] + " Hz", 14), new System.Windows.Point(110, 347));


                drawingContext.DrawText(ToText("SYSTEM", 14), new System.Windows.Point(15, 385));
                drawingContext.DrawText(ToText("SYSTÈME", 14), new System.Windows.Point(15, 400));
                drawingContext.DrawText(ToText(P[i] + "PH  " + W[i] + "W", 14), new System.Windows.Point(110, 392));


                drawingContext.DrawText(ToText("SHORT CIRCUIT CURRENT RATING", 14), new System.Windows.Point(60, 420));
                drawingContext.DrawText(ToText("LA VALEUR DU COURANT DE COURT CIRCUIT", 14), new System.Windows.Point(15, 435));

                drawingContext.DrawText(ToText("SYMMETRICAL" + "        " + ShortCircuitRating[i] + "KA", 14), new System.Windows.Point(15, 460));
                drawingContext.DrawText(ToText("AMPS/L'AMPERAGE", 14), new System.Windows.Point(15, 475));
                drawingContext.DrawText(ToText("SYMMETTRIQUE", 14), new System.Windows.Point(15, 490));

                drawingContext.DrawText(ToText(Amps[i], 14), new System.Windows.Point(180, 482));

                drawingContext.DrawText(ToText("VOLTS MAX.", 14), new System.Windows.Point(230, 475));
                drawingContext.DrawText(ToText("TENSION MAX.", 14), new System.Windows.Point(230, 490));


                drawingContext.DrawText(ToText("JOB NO.", 14), new System.Windows.Point(15, 510));
                drawingContext.DrawText(ToText("NO. REF.", 14), new System.Windows.Point(15, 525));
                drawingContext.DrawText(ToText(GONumber[i] + "-" + ItemNumber[i], 14), new System.Windows.Point(150, 517));


                DateTime dt = DateTime.Now;
                string sDate = dt.ToShortDateString();

                drawingContext.DrawText(ToText("TYPE        PRLCS", 14), new System.Windows.Point(15, 550));
                drawingContext.DrawText(ToText("DATE     " + sDate, 14), new System.Windows.Point(200, 550));
                drawingContext.DrawText(ToText("ENCLOSURE TYPE", 14), new System.Windows.Point(15, 565));
                drawingContext.DrawText(ToText("ENVELOPPE TYPE", 14), new System.Windows.Point(15, 580));

                drawingContext.DrawText(ToText(Enclosure[i], 14), new System.Windows.Point(155, 572));


                string[] SMCenterLines = SMCenter[i].Split('/');
                double Ypoint = 615;
                for (var j = 0; j < SMCenterLines.Count(); j++)
                {
                    drawingContext.DrawText(ToText(SMCenterLines[j], 14), new System.Windows.Point(80, Ypoint));
                    Ypoint += 20;
                }


                drawingContext.Close();

                RenderTargetBitmap renderBmap = new RenderTargetBitmap(img.PixelWidth * 8, img.PixelHeight * 8, 768, 768, PixelFormats.Pbgra32);
                renderBmap.Render(drawingVisual);

                im = new System.Windows.Controls.Image();
                im.Source = renderBmap;
                im.Width = img.Width - 20;
                im.Height = img.Height;
                im.HorizontalAlignment = HorizontalAlignment.Left;
                im.VerticalAlignment = VerticalAlignment.Top;


                accessPrintDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                accessPrintDialog.PrintVisual(im, "Image Print");

                updateExcelDoc(GONumber[i], ItemNumber[i], Enclosure[i], sDate);
            }
        }


        private void updateExcelDoc(string GO, string item, string enclosure, string date) 
        {
            string command = "insert into [CSANamePlateRecord259P075H01] ([Job Number], [GO Number], [SWBD Type], [Enclosure Type], [Customer], [Print Date]) values (?, ?, ?, ?, ?, ?)";
            using (OleDbCommand InsertCSA = new OleDbCommand(command, MainWindow.LPcon))
            {
                InsertCSA.Parameters.AddWithValue("[Job Number]", GO);
                InsertCSA.Parameters.AddWithValue("[GO Number]", GO + "-" + item);
                InsertCSA.Parameters.AddWithValue("[SWBD Type]", "PRLCS");
                InsertCSA.Parameters.AddWithValue("[Enclosure Type]", enclosure);
                InsertCSA.Parameters.AddWithValue("[Customer]", customer);
                InsertCSA.Parameters.AddWithValue("[Print Date]", date);

                InsertCSA.ExecuteNonQuery();
            }//end using command 
        }


        private FormattedText ToText(string input, int font)
        {
            FormattedText formattedText = new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), font, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return formattedText;
        }

    }
}




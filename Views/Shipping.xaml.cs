using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PRL123_Final.Views
{
    /// <summary>
    /// Interaction logic for Shipping.xaml
    /// </summary>
    public partial class Shipping : UserControl
    {
        string[] GOItemsArr;
        string[] UrgencyArr;
        string[] ShopOrderInterior;
        string[] ShopOrderBox;
        string[] ShopOrderTrim;
        string[] Quantity;
        Boolean[] BoxEarly;

        Boolean SuccessPull = false;
        Boolean InfoLoaded = false;
        Boolean BoxSelected = false;

        int page;

        string SelectedGO;
        string ProductTable;
        public string Current_Tab;

        Utility.ProductGroup CurrentProduct;


        public Shipping()
        {
            InitializeComponent();

            Current_Tab = "Shipping";
            PRL123_Set();
            ButtonColorChanges();
        }


        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;
                SelectedGO = row["GO_Item"];
                BoxSelected = true;
            }
        }


        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Scan.Text.Length >= 10)
                {
                    page = 0;
                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        getGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency], [BoxEarly] from [PRL123] where [GO]='" + Scan.Text.Substring(0, 10) + "' order by [GO_Item]");
                    }
                    else
                    {
                        getGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency] from [PRL4] where [GO]='" + Scan.Text.Substring(0, 10) + "' and [PageNumber] = 0 order by [GO_Item]");
                    }
                    if (SuccessPull == false)
                    {
                        updateStatus(Scan.Text + " ENTRY NOT FOUND");
                    }
                    else
                    {
                        InfoLoaded = true;
                        InsertData();
                        updateStatus(Scan.Text + " SCANNED SUCCESSFULLY");
                    }
                }
                else
                {
                    updateStatus("INVALID SCAN");
                }
            }
            catch
            {
                MessageBox.Show("Unable To Scan, Please Try Again");
            }
            finally
            {
                Scan.Text = "";
            }
        }


        private void getGOs(string query)
        {
            DataTableReader rd = Utility.loadData(query);
            DataTableReader rb = Utility.loadData(query);

            if (rd.HasRows == false)
            {
                SuccessPull = false;
                return;
            }
            else 
            {
                SuccessPull = true;
            }

            var counter = 0;
            int pages = 0;

            using (rd)
            {
                while (rd.Read())
                {

                    pages++;
                }
            }

            GOItemsArr = new string[pages];
            UrgencyArr = new string[pages];
            ShopOrderInterior = new string[pages];
            ShopOrderBox = new string[pages];
            ShopOrderTrim = new string[pages];
            Quantity = new string[pages];
            BoxEarly = new Boolean[pages];

            using (rb)
            {
                while (rb.Read())
                {
                    GOItemsArr[counter] = rb[0].ToString();
                    ShopOrderInterior[counter] = rb[1].ToString();
                    ShopOrderBox[counter] = rb[2].ToString();
                    ShopOrderTrim[counter] = rb[3].ToString();                    
                    Quantity[counter] = rb[4].ToString();
                    UrgencyArr[counter] = rb[5].ToString();

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        //PRL123 Specific
                        BoxEarly[counter] = (Boolean)rb[6];
                    }
                    counter++;
                }
            }
        }


        private void InsertData() 
        {
            GOI.Text = GOItemsArr[page];
            setQRs();
            setUrgency();

            pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemsArr.Length.ToString();
        }


        public void LoadGrid(string inputQuery)
        {
            DataTable dt;
            string query = "";
            if (!string.IsNullOrEmpty(inputQuery))
            {
                query = inputQuery;
            }
            else 
            {
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short] from [PRL123] where [Tracking]='Shipping'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short] from [PRL4] where [Tracking]='Shipping' and [PageNumber] = 0";
                }
            }
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
        }

        private void PRL123_Click(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
        }

        private void PRL4_Click(object sender, RoutedEventArgs e)
        {
            PRL4_Set();
        }

        private void PRL123_Set()
        {
            ProductTable = "PRL123";
            CurrentProduct = Utility.ProductGroup.PRL123;
            PWL123.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            LoadGrid(null);
        }

        private void PRL4_Set()
        {
            ProductTable = "PRL4";
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            LoadGrid(null);
        }

        public void ButtonColorChanges()
        {
            if (Current_Tab.Equals("Ship Early"))
            {
                ShipEarlyBtn.Background = System.Windows.Media.Brushes.DarkBlue;
                ShippingBtn.Background = System.Windows.Media.Brushes.Blue;
            }
            else if (Current_Tab.Equals("Shipping"))
            {
                ShipEarlyBtn.Background = System.Windows.Media.Brushes.Blue;
                ShippingBtn.Background = System.Windows.Media.Brushes.DarkBlue;
            }
        }

        private void updateStatus(string command)
        {
            Status.Content = command;
            Status.Foreground = System.Windows.Media.Brushes.Green;
        }



        private void View_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BoxSelected == true)
                {
                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        ShippingView SF = new ShippingView(SelectedGO, this);
                        SF.Show();
                    }
                }
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To View The Order\nPlease Make Sure You Are On The Ship Early Tab");
            }
        }




        private void Specialty_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    string query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Catalogue], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short] from [PRL123] where [BoxEarly]=True and [Box Sent]=False";
                    LoadGrid(query);
                    Current_Tab = "Ship Early";
                    ButtonColorChanges();
                }
                else
                {
                    MessageBox.Show("Ship Early View Is Only Available For PRL123");
                }
            }
            catch
            {
                MessageBox.Show("Unable To Switch To Ship Early Tab\nPlease Try Again");
            }
        }


        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadGrid(null);
                Current_Tab = "Shipping";
                ButtonColorChanges();
            }
            catch
            {
                MessageBox.Show("Unable To Switch To Shipping Tab\nPlease Try Again");
            }
        }



        private void setUrgency()
        {
            if (UrgencyArr[page] == "UP")
            {
                Urgency.Content = "Urgent Paid";
                Urgency.Background = System.Windows.Media.Brushes.Red;
            }
            else if (UrgencyArr[page] == "UU")
            {
                Urgency.Content = "Urgent Unpaid";
                Urgency.Background = System.Windows.Media.Brushes.Green;
            }
            else if (UrgencyArr[page] == "HOLD")
            {
                Urgency.Content = "HOLD";
                Urgency.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                Urgency.Content = "Normal";
                Urgency.Background = System.Windows.Media.Brushes.LightGray;
            }
        }


        private void setQRs()
        {
            if ((BoxEarly[page] == false) && (string.IsNullOrEmpty(ShopOrderBox[page]) == false))
            {
                Box.Source = Utility.GenerateQRCode(CC_code(ShopOrderBox[page], Quantity[page]));
            }
            else
            {
                Box.Source = null;
            }

            if (string.IsNullOrEmpty(ShopOrderTrim[page]))
            {
                Trim.Source = null;
            }
            else
            {
                Trim.Source = Utility.GenerateQRCode(CC_code(ShopOrderTrim[page], Quantity[page]));
            }

            if (string.IsNullOrEmpty(ShopOrderInterior[page]))
            {
                Interior.Source = null;
            }
            else
            {
                Interior.Source = Utility.GenerateQRCode(CC_code(ShopOrderInterior[page], Quantity[page]));
            }
        }



        private string CC_code(string Order, string Qty)
        {
            string output;
            string Tab = "\t";
            output = Order + Tab + Views.Configuration.addressLocation[3] + "-FG" + Tab + Tab + "XX.XX.FG.FG" + Tab + Qty;
            return output;
        }



        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InfoLoaded == true)
                {
                    if (page < GOItemsArr.Length - 1)
                    {
                        page += 1;

                        InsertData();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable To Load Next Line Item");
            }
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InfoLoaded == true)
                {
                    if (page != 0)
                    {
                        page -= 1;

                        InsertData();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable To Load Previous Line Item");
            }
        }












        //Fields Used When Storing All Quality Information

        //from SetupDirectoriesDate()
        static string pdfDirectory;
        static string shippedDate;      


        //from getDataFromLPDB()
        static string GO_Item;
        static string interior;
        static string box;
        static string trim;
        static string customer;
        static string quantity;
        static string enteredDate;
        static string releaseDate;
        static string commitDate;
        static string urgency;

        static string SpecialCustomer;
        static string AMO;
        static string ServiceEntrance;
        static string RatedNeutral200;
        static string PaintedBox;
        static string DNSB;
        static string Complete;
        static string Short;
        static string Notes;

        //PRL123 Specific
        static string BoxEarlyInfo;
        static string BoxSent;
        static string DoubleSection;

        //PRL4 Specific 
        static string DoorOverDist;
        static string DoorInDoor;

        static string type;
        static string volts;
        static string amps;
        static string torque;
        static string appearance;
        static string bus;
        static string catalogue;
        static string productSpecialist;


        //from getCSAinfo()
        static string Designation;
        static string Enclosure;
        static string N;
        static string XSpaceUsed;
        static string MA;
        static string Voltage;
        static string P;
        static string W;
        static string Ground;
        static string Hz;
        static string ProductID;



        private void Shipped_Click(object sender, RoutedEventArgs e)
        {
            if ((InfoLoaded == true) && (string.IsNullOrEmpty(GOI.Text) == false))
            {
                if (MessageBox.Show("You Are About To Ship " + GOI.Text + "\nWould You Like To Proceed?", "Confirm",
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        SetupDirectoriesDate();
                        getDataFromLPDB();
                        getCSAinfo();

                        cleanUpDirectories();
                        Ship();

                        ReLoadPage();
                    }
                    catch
                    {
                        MessageBox.Show("Unable To Ship The Line Item\nPlease Try Again");
                        updateStatus(GOI.Text + " SHIPPING ERROR");
                    }
                }
            }
        }


        private void SetupDirectoriesDate()
        {
            pdfDirectory = Utility.getDirectoryForOrderFiles(GOI.Text, CurrentProduct);
            Directory.CreateDirectory(pdfDirectory + @"\Z_FinalShippedDocuments");

            DateTime dt = DateTime.Now;
            shippedDate = dt.ToShortDateString();
        }

        private void getDataFromLPDB() 
        {
            string query = "";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [Notes], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO_Item]='" + GOI.Text + "'";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [Notes], [DoorOverDist], [DoorInDoor] from [PRL4] where [GO_Item]='" + GOI.Text + "' and [PageNumber]=0";
            }
            
            DataTableReader dtr = Utility.loadData(query);
            using (dtr) 
            {
                while (dtr.Read()) 
                {
                    GO_Item = dtr[0].ToString();
                    interior = dtr[1].ToString();
                    box = dtr[2].ToString();
                    trim = dtr[3].ToString();
                    customer = dtr[4].ToString();
                    quantity = dtr[5].ToString();
                    enteredDate = dtr[6].ToString();
                    releaseDate = dtr[7].ToString();
                    commitDate = dtr[8].ToString();
                    urgency = dtr[9].ToString();

                    type = dtr[10].ToString();
                    volts = dtr[11].ToString();
                    amps = dtr[12].ToString();
                    torque = dtr[13].ToString();
                    appearance = dtr[14].ToString();
                    bus = dtr[15].ToString();
                    catalogue = dtr[16].ToString();
                    productSpecialist = dtr[17].ToString();

                    SpecialCustomer = dtr[18].ToString();
                    AMO = dtr[19].ToString();
                    ServiceEntrance = dtr[20].ToString();
                    RatedNeutral200 = dtr[21].ToString();
                    PaintedBox = dtr[22].ToString();
                    DNSB = dtr[23].ToString();
                    Complete = dtr[24].ToString();
                    Short = dtr[25].ToString();
                    Notes = dtr[26].ToString();

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        //PRL123 Specific
                        BoxEarlyInfo = dtr[27].ToString();
                        BoxSent = dtr[28].ToString();
                        DoubleSection = dtr[29].ToString();
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        //PRL4 Specific 
                        DoorOverDist = dtr[27].ToString();
                        DoorInDoor = dtr[28].ToString();
                    }
                }
            }       
        }


        private void getCSAinfo()
        {
            DataTableReader values = Utility.getCSAValues(GOI.Text, CurrentProduct);
            using (values)
            {
                while (values.Read())
                {
                    Designation = values[3].ToString();
                    Enclosure = values[4].ToString();
                    N = values[5].ToString();
                    XSpaceUsed = values[6].ToString();
                    MA = values[7].ToString();
                    Voltage = values[8].ToString();
                    P = values[9].ToString();
                    W = values[10].ToString();
                    Ground = values[11].ToString();
                    Hz = values[12].ToString();
                    ProductID = values[14].ToString();
                }
            }
        }

    

        private void cleanUpDirectories()
        {
            var pdfDir = new DirectoryInfo(pdfDirectory);

            //write TXT file with all Quality Info
            string txtPath = pdfDirectory + @"\Z_FinalShippedDocuments\QualityInfo_" + GOI.Text + ".txt";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                writeQualityInfoTXTprl123(txtPath);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                writeQualityInfoTXTprl4(txtPath);
            }

            //convert the TXT file to PDF
            Utility.ConvertTXTtoPDF(txtPath, pdfDirectory + @"\BLT_" + GOI.Text + "_QualityInfo.pdf");


            MessageBox.Show("Storing All Quality Information...");


            //merge all CONSTR files into final CONSTR PDF
            FileInfo[] pdfFileArr = pdfDir.GetFiles(GOI.Text + "*");
            List<string> pdfFileNames = new List<string>(pdfFileArr.Length);
            foreach (var file in pdfFileArr)
            {
                if (file.Exists)
                {
                    pdfFileNames.Add(file.FullName);
                }
            }
            if (pdfFileArr.Length > 0) 
            {
                string outputFile = pdfDirectory + @"\Z_FinalShippedDocuments\" + GOI.Text + "_CONSTR_FINAL.pdf";
                Utility.Merge(pdfFileNames, outputFile);
            }
            

            //merge all BLT files into final BLT PDF
            FileInfo[] BLTpdfFileArr = pdfDir.GetFiles("BLT_" + GOI.Text + "*");
            List<string> BLTpdfFileNames = new List<string>(BLTpdfFileArr.Length);
            foreach (var file in BLTpdfFileArr)
            {
                if (file.Exists)
                {
                    BLTpdfFileNames.Add(file.FullName);
                }
            }
            if (BLTpdfFileArr.Length > 0)
            {
                string outputFile2 = pdfDirectory + @"\Z_FinalShippedDocuments\BLT_" + GOI.Text + "_FINAL.pdf";
                Utility.Merge(BLTpdfFileNames, outputFile2);
            }

        }


        private void writeQualityInfoTXTprl123(string documentPath)
        {
            //PRL123
            string[] lines = {"QUALITY INFORMATION REPORT"," ", "Go Item: " + GO_Item, "Product ID: " + ProductID, "Shop Order Interior: " + interior,
                "Shop Order Box: " + box, "Shop Order Trim: " + trim, "Customer: " + customer, "Quantity: " + quantity, "Date Entered: " + enteredDate,
                "Date Released: " + releaseDate, "Commit Date: " + commitDate, "Date Shipped: " + shippedDate, "Urgency: " + urgency," ", "Type: " + type,
                "Volts: " + volts, "Amps: " + amps, "Torque: " + torque, "Appearance: " + appearance, "Bus: " + bus, "Catalogue: " + catalogue,
                "Product Specialist: " + productSpecialist, "FilePath: " + pdfDirectory," ", "BoxEarly: " + BoxEarlyInfo, "BoxSent: " + BoxSent,
                "AMO: " + AMO, "Special Customer: " + SpecialCustomer, "Service Entrance: " + ServiceEntrance, "200% Rated Neutral: " + RatedNeutral200,
                "Painted Box: " + PaintedBox, "Double Section: " + DoubleSection, "DNSB: " + DNSB, "Complete: " + Complete, "Short: " + Short," ", "Designation: " + Designation,
                "Enclosure: " + Enclosure, "N: " + N, "X Space Used: " + XSpaceUsed, "MA: " + MA, "Voltage: " + Voltage, "P: " + P, "W: " + W,
                "Ground: " + Ground, "Hz: " + Hz," ", "Notes: " + Notes };

            Utility.WriteLinesToTXT(lines, documentPath);
        }

        private void writeQualityInfoTXTprl4(string documentPath)
        {
            //PRL4
            string[] lines = {"QUALITY INFORMATION REPORT"," ", "Go Item: " + GO_Item, "Product ID: " + ProductID, "Shop Order Interior: " + interior,
                "Shop Order Box: " + box, "Shop Order Trim: " + trim, "Customer: " + customer, "Quantity: " + quantity, "Date Entered: " + enteredDate,
                "Date Released: " + releaseDate, "Commit Date: " + commitDate, "Date Shipped: " + shippedDate, "Urgency: " + urgency," ", "Type: " + type,
                "Volts: " + volts, "Amps: " + amps, "Torque: " + torque, "Appearance: " + appearance, "Bus: " + bus, "Catalogue: " + catalogue,
                "Product Specialist: " + productSpecialist, "FilePath: " + pdfDirectory," ", "AMO: " + AMO, "Special Customer: " + SpecialCustomer,
                "Service Entrance: " + ServiceEntrance, "200% Rated Neutral: " + RatedNeutral200, "DoorOverDist: " + DoorOverDist, "DoorInDoor: " + DoorInDoor,
                "Painted Box: " + PaintedBox,  "DNSB: " + DNSB, "Complete: " + Complete, "Short: " + Short," ", "Designation: " + Designation,
                "Enclosure: " + Enclosure, "N: " + N, "X Space Used: " + XSpaceUsed, "MA: " + MA, "Voltage: " + Voltage, "P: " + P, "W: " + W,
                "Ground: " + Ground, "Hz: " + Hz," ", "Notes: " + Notes };

            Utility.WriteLinesToTXT(lines, documentPath);
        }

        
        private void Ship()
        {
            string command = "delete * from [" + ProductTable + "] where [GO_Item]='" + GOI.Text + "'";
            Utility.executeNonQueryLP(command);
            Utility.DeleteCSAValues(GOI.Text, CurrentProduct);
            LoadGrid(null);
            updateStatus(GOI.Text + " SUCCESSFULLY SHIPPED");
        }



        private void ReLoadPage()
        {
            page = 0;
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                getGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency], [BoxEarly] from [PRL123] where [GO]='" + GOI.Text.Substring(0, 10) + "' order by [GO_Item]");
            }
            else
            {
                getGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency] from [PRL4] where [GO]='" + GOI.Text.Substring(0, 10) + "' and [PageNumber] = 0 order by [GO_Item]");
            }

            if (SuccessPull == true)
            {
                InfoLoaded = true;
                InsertData();
            }
            else 
            {
                InfoLoaded = false;
                GOI.Text = "";
                Urgency.Content = "";
                pg.Content = "";
                Interior.Source = null;
                Box.Source = null;
                Trim.Source = null;
            }
        }




    }
}

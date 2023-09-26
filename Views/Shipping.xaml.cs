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
using System.Collections.ObjectModel;

namespace LightningPRO.Views
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

            PRL123_Set();
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
                        GetGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency], [BoxEarly] from [PRL123] where [GO]='" + Scan.Text.Substring(0, 10) + "' order by [GO_Item]");
                    }
                    else
                    {
                        GetGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency] from [" + ProductTable + "] where [GO]='" + Scan.Text.Substring(0, 10) + "' and [PageNumber] = 0 order by [GO_Item]");
                    }
                    if (SuccessPull == false)
                    {
                        UpdateStatus(Scan.Text + " ENTRY NOT FOUND");
                    }
                    else
                    {
                        InfoLoaded = true;
                        InsertData();
                        UpdateStatus(Scan.Text + " SCANNED SUCCESSFULLY");
                    }
                }
                else
                {
                    UpdateStatus("INVALID SCAN");
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


        private void GetGOs(string query)
        {
            DataTableReader rd = Utility.LoadData(query);
            DataTableReader rb = Utility.LoadData(query);

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
            SetQRs();
            SetUrgency();

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
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [NameplateRequired], [NameplateOrdered], [LabelsPrinted], [Notes] from [PRL123] where [Tracking]='Shipping'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [NameplateRequired], [NameplateOrdered], [LabelsPrinted], [Notes] from [PRL4] where [Tracking]='Shipping' and [PageNumber] = 0";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [NameplateRequired], [NameplateOrdered], [LabelsPrinted], [Notes] from [PRLCS] where [Tracking]='Shipping' and [PageNumber] = 0";
                }
            }
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            HideFullNotesColoumn();
        }

        private void PRL123_Click(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
            Refresh();
        }

        private void PRL4_Click(object sender, RoutedEventArgs e)
        {
            PRL4_Set();
            Refresh();
        }
        private void PRLCS_Click(object sender, RoutedEventArgs e)
        {
            PRLCS_Set();
            Refresh();
        }

        public void PRL123_Set()
        {
            ProductTable = "PRL123";
            CurrentProduct = Utility.ProductGroup.PRL123;
            PWL123.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.Blue;
            Current_Tab = "Shipping";
            ButtonColorChanges();
            LoadGrid(null);
        }

        private void PRL4_Set()
        {
            ProductTable = "PRL4";
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.Blue;
            Current_Tab = "Shipping";
            ButtonColorChanges();
            LoadGrid(null);
        }

        private void PRLCS_Set()
        {
            ProductTable = "PRLCS";
            CurrentProduct = Utility.ProductGroup.PRLCS;
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.DarkBlue;
            Current_Tab = "Shipping";
            ButtonColorChanges();
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

        private void UpdateStatus(string command)
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
                    string query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Catalogue], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [NameplateRequired], [NameplateOrdered], [LabelsPrinted], [Notes] from [PRL123] where [BoxEarly]=True and [Box Sent]=False";
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



        private void SetUrgency()
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


        private void SetQRs()
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



        private void Open_Records(object sender, RoutedEventArgs e)
        {
            CSArecords sf = new CSArecords();
            sf.Show();
        }


        private void HideFullNotesColoumn()
        {
            //Get the columns collection
            ObservableCollection<DataGridColumn> columns = dg.Columns;

            //set the visibility for end Notes coloumn to hidden
            foreach (DataGridColumn col in columns)
            {
                switch (col.Header.ToString())
                {
                    case "Notes":
                        col.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        private void DG_Loaded(object sender, RoutedEventArgs e)
        {
            HideFullNotesColoumn();
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
        static string PaintedBox;
        static string DNSB;
        static string Complete;
        static string Short;
        static string Notes;

        //PRL123/PRL4 only
        static string ServiceEntrance;
        static string RatedNeutral200;


        //PRL123 Specific
        static string BoxEarlyInfo;
        static string BoxSent;
        static string DoubleSection;

        //PRL4 Specific 
        static string DoorOverDist;
        static string DoorInDoor;

        //PRLCS Specific
        static string IncLocLeft;
        static string IncLocRight;
        static string CrossBus;
        static string OpenBottom;
        static string ExtendedTop;
        static string ThirtyDeepEnclosure;

        static string type;
        static string volts;
        static string amps;
        static string torque;
        static string appearance;
        static string bus;
        static string catalogue;
        static string productSpecialist;


        //from getCSAinfo()

        static string Voltage;
        static string P;
        static string W;
        static string Hz;
        static string Enclosure;
        static string ProductID;

        //PRL4/123 only
        static string Designation;
        static string N;
        static string XSpaceUsed;
        static string MA;
        static string Ground;

        //PRLCS
        static string SwitchBoard;
        static string CSAStandard;
        static string SMCenter;
        static string Section;
        static string MainBusBarCapacity;
        static string ShortCircuitRating;
        static string Amps;




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
                        GetDataFromLPDB();
                        GetCSAinfo();

                        CleanUpDirectories();
                        Ship();

                        ReLoadPage();
                    }
                    catch
                    {
                        MessageBox.Show("Unable To Ship The Line Item\nPlease Try Again");
                        UpdateStatus(GOI.Text + " SHIPPING ERROR");
                    }
                }
            }
        }

        private void SetupDirectoriesDate()
        {
            pdfDirectory = Utility.GetDirectoryForOrderFiles(GOI.Text, CurrentProduct);
            Directory.CreateDirectory(pdfDirectory + @"\Z_FinalShippedDocuments");

            DateTime dt = DateTime.Now;
            shippedDate = dt.ToShortDateString();
        }

        

        private void GetDataFromLPDB()
        {
            //base query for all similar fields 
            string query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [PaintedBox], [DNSB], [Complete], [Short], [Notes], ";

            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query += "[ServiceEntrance], [RatedNeutral200], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO_Item]='" + GOI.Text + "'";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                query += "[ServiceEntrance], [RatedNeutral200], [DoorOverDist], [DoorInDoor] from [PRL4] where [GO_Item]='" + GOI.Text + "' and [PageNumber]=0";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                query += "[IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [ThirtyDeepEnclosure] from [PRLCS] where [GO_Item]='" + GOI.Text + "' and [PageNumber]=0";
            }

            DataTableReader dtr = Utility.LoadData(query);
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
                    PaintedBox = dtr[20].ToString();
                    DNSB = dtr[21].ToString();
                    Complete = dtr[22].ToString();
                    Short = dtr[23].ToString();
                    Notes = dtr[24].ToString();

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        ServiceEntrance = dtr[25].ToString();
                        RatedNeutral200 = dtr[26].ToString();

                        //PRL123 Specific
                        BoxEarlyInfo = dtr[27].ToString();
                        BoxSent = dtr[28].ToString();
                        DoubleSection = dtr[29].ToString();
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        ServiceEntrance = dtr[25].ToString();
                        RatedNeutral200 = dtr[26].ToString();

                        //PRL4 Specific 
                        DoorOverDist = dtr[27].ToString();
                        DoorInDoor = dtr[28].ToString();
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        //PRLCS Specific
                        IncLocLeft = dtr[25].ToString();
                        IncLocRight = dtr[26].ToString();
                        CrossBus = dtr[27].ToString();
                        OpenBottom = dtr[28].ToString();
                        ExtendedTop = dtr[29].ToString();
                        ThirtyDeepEnclosure = dtr[30].ToString();
                    }
                }
            }
        }



        private void GetCSAinfo()
        {
            DataTableReader values = Utility.GetCSAValues(GOI.Text, CurrentProduct);
            using (values)
            {
                while (values.Read())
                {
                    if (CurrentProduct == Utility.ProductGroup.PRL4 || CurrentProduct == Utility.ProductGroup.PRL123)
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
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        SwitchBoard = values[3].ToString();
                        CSAStandard = values[4].ToString();
                        SMCenter = values[5].ToString();
                        Section = values[6].ToString();
                        MainBusBarCapacity = values[7].ToString();
                        Voltage = values[8].ToString();
                        Hz = values[9].ToString();
                        P = values[10].ToString();
                        W = values[11].ToString();
                        ShortCircuitRating = values[12].ToString();
                        Amps = values[13].ToString();
                        Enclosure = values[14].ToString();
                        ProductID = values[16].ToString();
                    }
                }
            }
        }




        private void CleanUpDirectories()
        {
            var pdfDir = new DirectoryInfo(pdfDirectory);

            //write TXT file with all Quality Info
            string txtPath = pdfDirectory + @"\Z_FinalShippedDocuments\QualityInfo_" + GOI.Text + ".txt";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                WriteQualityInfoTXTprl123(txtPath);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                WriteQualityInfoTXTprl4(txtPath);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                WriteQualityInfoTXTprlCS(txtPath);
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


        private void WriteQualityInfoTXTprl123(string documentPath)
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

        private void WriteQualityInfoTXTprl4(string documentPath)
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

        private void WriteQualityInfoTXTprlCS(string documentPath)
        {
            //PRLCS
            string[] lines = {"QUALITY INFORMATION REPORT"," ", "Go Item: " + GO_Item, "Product ID: " + ProductID, "Shop Order Interior: " + interior,
                "Shop Order Box: " + box, "Shop Order Trim: " + trim, "Customer: " + customer, "Quantity: " + quantity, "Date Entered: " + enteredDate,
                "Date Released: " + releaseDate, "Commit Date: " + commitDate, "Date Shipped: " + shippedDate, "Urgency: " + urgency," ", "Type: " + type,
                "Volts: " + volts, "Amps: " + amps, "Torque: " + torque, "Appearance: " + appearance, "Bus: " + bus, "Catalogue: " + catalogue,
                "Product Specialist: " + productSpecialist, "FilePath: " + pdfDirectory," ", "AMO: " + AMO, "Special Customer: " + SpecialCustomer,
                "IncLocLeft: " + IncLocLeft,"IncLocRight: " + IncLocRight,"CrossBus: " + CrossBus,"OpenBottom: " + OpenBottom,"ExtendedTop: " + ExtendedTop,
                "PaintedBox: " + PaintedBox, "ThirtyDeepEnclosure: " + ThirtyDeepEnclosure,"DNSB: " + DNSB, "Complete: " + Complete, "Short: " + Short," ",
                "SwitchBoard: " + SwitchBoard, "CSAStandard: " + CSAStandard, "SMCenter: " + SMCenter, "Section: " + Section, "MainBusBarCapacity: " + MainBusBarCapacity,
                "Voltage: " + Voltage, "Hz: " + Hz, "P: " + P, "W: " + W, "ShortCircuitRating: " + ShortCircuitRating, "Max Volts: " + Amps, "Enclosure: " + Enclosure,
                " ", "Notes: " + Notes };
            Utility.WriteLinesToTXT(lines, documentPath);
        }



        private void Ship()
        {
            string command = "delete * from [" + ProductTable + "] where [GO_Item]='" + GOI.Text + "'";
            Utility.ExecuteNonQueryLP(command);
            Utility.DeleteCSAValues(GOI.Text, CurrentProduct);
            LoadGrid(null);
            UpdateStatus(GOI.Text + " SUCCESSFULLY SHIPPED");
        }



        private void ReLoadPage()
        {
            page = 0;
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                GetGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency], [BoxEarly] from [PRL123] where [GO]='" + GOI.Text.Substring(0, 10) + "' order by [GO_Item]");
            }
            else
            {
                GetGOs("select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], [Urgency] from [" + ProductTable + "] where [GO]='" + GOI.Text.Substring(0, 10) + "' and [PageNumber] = 0 order by [GO_Item]");
            }

            if (SuccessPull == true)
            {
                InfoLoaded = true;
                InsertData();
            }
            else 
            {
                Refresh();
            }
        }

        private void Refresh() 
        {
            InfoLoaded = false;
            SuccessPull = false;
            BoxSelected = false;
            GOI.Text = "";
            Urgency.Content = null;
            pg.Content = "";
            Interior.Source = null;
            Box.Source = null;
            Trim.Source = null;
        }


    }
}

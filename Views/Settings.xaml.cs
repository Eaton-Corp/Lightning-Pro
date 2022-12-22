using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PRL123_Final.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        int Selected;
        
        string SelectedDelete = "";
        List<string> MultipleSelectedDelete;

        string ProductTable;
        Utility.ProductGroup CurrentProduct;

        public Settings()
        {
            InitializeComponent();
            PRL123_Set();
            loadGrid("select * from [Employee]");
        }

        private void loadGrid(string query)
        {
            DataTable dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;    
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;
                Selected = row["ID"];
            }
        }

        private void Click_Delete(object sender, RoutedEventArgs e)
        {
            string query = "delete * from [Employee] where [ID]=" + Selected.ToString();
            Utility.executeNonQueryLP(query);
            loadGrid("select * from [Employee]");
        }

        private void Click_Insert(object sender, RoutedEventArgs e)
        {
            string commandStr = "insert into [Employee] (EmployeeName, EmployeePosition) values (?, ?)";
            using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
            {
                cmd.Parameters.AddWithValue("EmployeeName", EmployeeName.Text);
                cmd.Parameters.AddWithValue("EmployeePosition", Role.Text);
                cmd.ExecuteNonQuery();
            } //end using command
            loadGrid("select * from [Employee]");
        }




        private void loadDeleteGrid()
        {
            string query = "";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "SELECT A.[GO_Item], A.[GO], A.[ShopOrderInterior], A.[ShopOrderBox], A.[ShopOrderTrim], A.[Quantity], A.[EnteredDate], A.[ReleaseDate], A.[CommitDate], A.[Tracking], A.[Urgency], A.[Customer], A.[SpecialCustomer], A.[Complete], A.[Short], B.[Closed Date] FROM [" + ProductTable + "] as A LEFT JOIN [tblOrderStatus] as B ON A.[GO_Item] = B.[GO Item] WHERE B.[Closed Date] IS NOT NULL";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4) 
            {
                query = "SELECT A.[GO_Item], A.[GO], A.[ShopOrderInterior], A.[ShopOrderBox], A.[ShopOrderTrim], A.[Quantity], A.[EnteredDate], A.[ReleaseDate], A.[CommitDate], A.[Tracking], A.[Urgency], A.[Customer], A.[SpecialCustomer], A.[Complete], A.[Short], B.[Closed Date] FROM [" + ProductTable + "] as A LEFT JOIN [tblOrderStatus] as B ON A.[GO_Item] = B.[GO Item] WHERE B.[Closed Date] IS NOT NULL AND A.[PageNumber] = 0";
            }
            DataTable dt = Utility.SearchLP(query);
            CompleteGrid.ItemsSource = dt.DefaultView;
        }

        private void Click_CleanUp(object sender, RoutedEventArgs e)
        {
            loadDeleteGrid();
        }

        private void Selection_ChangedDelete(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            if (gd.SelectedItems != null)
            {
                MultipleSelectedDelete = new List<string>();
                for (int i = 0; i < gd.SelectedItems.Count; i++)
                {
                    dynamic row = gd.SelectedItems[i];
                    MultipleSelectedDelete.Add(row["GO_Item"]); ;
                }
            }

            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;
                SelectedDelete = row["GO_Item"];
            }
        }

        private async void Click_DeleteOld(object sender, RoutedEventArgs e)
        {
            string currentGOI = "";
            try
            {
                if (SelectedDelete != "" )
                {
                    if (MessageBox.Show("You Are About To Archive & Delete Old Jobs\nWould You Like To Proceed?", "Confirm",
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Task<int> ShowProgressBar = TurnOnStatus();
                        int result = await ShowProgressBar;

                        if (MultipleSelectedDelete.Count > 1)
                        {
                            foreach (string GoItem in MultipleSelectedDelete)
                            {
                                currentGOI = GoItem;
                                RegularShippingProcess(GoItem);
                                string query = "delete * from [" + ProductTable + "] where [GO_Item]='" + GoItem + "'";
                                Utility.executeNonQueryLP(query);
                                Utility.DeleteCSAValues(GoItem, CurrentProduct);
                            }
                            loadDeleteGrid();
                        }
                        else
                        {
                            currentGOI = SelectedDelete;
                            RegularShippingProcess(SelectedDelete);
                            string query = "delete * from [" + ProductTable + "] where [GO_Item]='" + SelectedDelete + "'";
                            Utility.executeNonQueryLP(query);
                            Utility.DeleteCSAValues(SelectedDelete, CurrentProduct);
                            loadDeleteGrid();
                        }
                        Status.Content = "OLD JOBS SUCCESSFULLY ARCHIVED ";
                    }
                }
            }
            catch
            {
                Status.Content = currentGOI + " ERROR ";
                MessageBox.Show("Deletion Error");
            }
        }


        private async Task<int> TurnOnStatus()
        {
            Status.Content = "PROCESSING ... ";
            await Task.Delay(500);
            return 1;
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



        private void RegularShippingProcess(string GoItem)
        {
            SetupDirectoriesDate(GoItem);
            SaveBLTfiles(GoItem);           //only for PRL123
            getDataFromLPDB(GoItem);
            getCSAinfo(GoItem);
            cleanUpDirectories(GoItem);
        }

        

        //only done for PRL123
        private void SaveBLTfiles(string GOI) 
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123) 
            {
                BitmapImage GOIpage = getBidman(GOI);
                var path = pdfDirectory + @"\BLT_" + GOI + ".pdf";
                Utility.SaveImageToPdf(path, GOIpage);
            }
        }

        private BitmapImage getBidman(string GOItem)
        {
            BitmapImage output;
            string imageFilePath = Utility.getImageFilePath(GOItem, CurrentProduct, 0);
            if (string.IsNullOrEmpty(imageFilePath))  //proceed with bidman byte array to bitmap image
            {
                output = Utility.retrieveBinaryBidman(GOItem);
            }
            else
            {
                output = Utility.PNGtoBitmap(imageFilePath);
            }
            return output;
        }
        



        private void SetupDirectoriesDate(string GOI)
        {
            pdfDirectory = Utility.getDirectoryForOrderFiles(GOI, CurrentProduct);
            Directory.CreateDirectory(pdfDirectory + @"\Z_FinalShippedDocuments");

            DateTime dt = DateTime.Now;
            shippedDate = dt.ToShortDateString();
        }

        private void getDataFromLPDB(string GOI)
        {
            string query = "";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [Notes], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO_Item]='" + GOI + "'";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [Notes], [DoorOverDist], [DoorInDoor] from [PRL4] where [GO_Item]='" + GOI + "' and [PageNumber]=0";
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


        private void getCSAinfo(string GOI)
        {
            DataTableReader values = Utility.getCSAValues(GOI, CurrentProduct);
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



        private void cleanUpDirectories(string GOI)
        {
            var pdfDir = new DirectoryInfo(pdfDirectory);

            //write TXT file with all Quality Info
            string txtPath = pdfDirectory + @"\Z_FinalShippedDocuments\QualityInfo_" + GOI + ".txt";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                writeQualityInfoTXTprl123(txtPath);
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                writeQualityInfoTXTprl4(txtPath);
            }

            //convert the TXT file to PDF
            Utility.ConvertTXTtoPDF(txtPath, pdfDirectory + @"\BLT_" + GOI + "_QualityInfo.pdf");



            //merge all CONSTR files into final CONSTR PDF
            FileInfo[] pdfFileArr = pdfDir.GetFiles(GOI + "*");
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
                string outputFile = pdfDirectory + @"\Z_FinalShippedDocuments\" + GOI + "_CONSTR_FINAL.pdf";
                Utility.Merge(pdfFileNames, outputFile);
            }


            //merge all BLT files into final BLT PDF
            FileInfo[] BLTpdfFileArr = pdfDir.GetFiles("BLT_" + GOI + "*");
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
                string outputFile2 = pdfDirectory + @"\Z_FinalShippedDocuments\BLT_" + GOI + "_FINAL.pdf";
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
            PWL123.Background = Brushes.DarkBlue;
            PWL4.Background = Brushes.Blue;
            loadDeleteGrid();
        }

        private void PRL4_Set()
        {
            ProductTable = "PRL4";
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            loadDeleteGrid();
        }


    }
}



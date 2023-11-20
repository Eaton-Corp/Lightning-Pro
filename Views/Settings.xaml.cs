using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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

namespace LightningPRO.Views
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
        }

        private void Settings_OnLoaded(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
            LoadGrid("select * from [Employee]");
        }


        private void LoadGrid(string query)
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
            Utility.ExecuteNonQueryLP(query);
            LoadGrid("select * from [Employee]");
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
            LoadGrid("select * from [Employee]");
        }




        private void LoadDeleteGrid()
        {
            string query = "";
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "SELECT A.[GO_Item], A.[GO], A.[ShopOrderInterior], A.[ShopOrderBox], A.[ShopOrderTrim], A.[Quantity], A.[EnteredDate], A.[ReleaseDate], A.[CommitDate], A.[Tracking], A.[Urgency], A.[Customer], A.[SpecialCustomer], A.[Complete], A.[Short], B.[Closed Date] FROM [" + ProductTable + "] as A LEFT JOIN [tblOrderStatus] as B ON A.[GO_Item] = B.[GO Item] WHERE B.[Closed Date] IS NOT NULL AND A.[PageNumber] = 0";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4 || CurrentProduct == Utility.ProductGroup.PRLCS) 
            {
                query = "SELECT A.[GO_Item], A.[GO], A.[ShopOrderInterior], A.[ShopOrderBox], A.[ShopOrderTrim], A.[Quantity], A.[EnteredDate], A.[ReleaseDate], A.[CommitDate], A.[Tracking], A.[Urgency], A.[Customer], A.[SpecialCustomer], A.[Complete], A.[Short], B.[Closed Date] FROM [" + ProductTable + "] as A LEFT JOIN [tblOrderStatus] as B ON A.[GO_Item] = B.[GO Item] WHERE B.[Closed Date] IS NOT NULL AND A.[PageNumber] = 0";
            }
            DataTable dt = Utility.SearchLP(query);
            CompleteGrid.ItemsSource = dt.DefaultView;
        }

        private void Click_CleanUp(object sender, RoutedEventArgs e)
        {
            LoadDeleteGrid();
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
                                Utility.ExecuteNonQueryLP(query);
                                Utility.DeleteCSAValues(GoItem, CurrentProduct);
                            }
                            LoadDeleteGrid();
                        }
                        else
                        {
                            currentGOI = SelectedDelete;
                            RegularShippingProcess(SelectedDelete);
                            string query = "delete * from [" + ProductTable + "] where [GO_Item]='" + SelectedDelete + "'";
                            Utility.ExecuteNonQueryLP(query);
                            Utility.DeleteCSAValues(SelectedDelete, CurrentProduct);
                            LoadDeleteGrid();
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




        public class InfoGOIPairs
        {
            public string GOI { get; private set; }
            public string ShopOrderInterior { get; set; }
            public string ShopOrderBox { get; set; }
            public string ShopOrderTrim { get; set; }
            public string SchedulingGroup { get; set; }
            public DateTime? EnteredDate { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public DateTime? CommitDate { get; set; }
            public string ProductSpecialist { get; set; }
            public bool RequiresUpdate { get; set; } = false;

            public InfoGOIPairs(string GoItem)
            {
                GOI = GoItem;
            }
        }


        List<InfoGOIPairs> InfoGOIPairsList;

        private async void Update_Jobs(object sender, RoutedEventArgs e)
        {
            /*try
            {*/
                if (MessageBox.Show("You Are About To Update All Information of Jobs in LightningPro.\nNote: this may take a couple minutes.\nWould You Like To Proceed?", "Confirm",
                      MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Task<int> ShowProgressBar = TurnOnStatus();
                    int result = await ShowProgressBar;

                    GetInfo("PRL123");
                    GetInfo("PRL4");
                    GetInfo("PRLCS");

                    Status.Content = "INFORMATION SUCCESSFULLY UPDATED ";
                }
            /*}
            catch
            {
                Status.Content = "UPDATE ERROR ";
                MessageBox.Show("Error Updating All Information");
            }*/
        }


        private async Task<int> TurnOnStatus()
        {
            Status.Content = "PROCESSING ... ";
            await Task.Delay(500);
            return 1;
        }


        private void GetInfo(string currTable)
        {
            InfoGOIPairsList = new List<InfoGOIPairs>();

            // Retrieve current GO items data from LightningPro database for comparison
            string loadCurrentDataStr = $"SELECT [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [ProductSpecialist], [CommitDate], [EnteredDate], [ReleaseDate] FROM [{currTable}] WHERE [PageNumber] = 0";
            DataTable currentData = Utility.SearchLP(loadCurrentDataStr);
            Dictionary<string, InfoGOIPairs> currentDataMap = currentData.AsEnumerable().ToDictionary(
                row => row.Field<string>("GO_Item"),
                row => new InfoGOIPairs(row.Field<string>("GO_Item"))
                {
                    ShopOrderInterior = row.Field<string>("ShopOrderInterior"),
                    ShopOrderBox = row.Field<string>("ShopOrderBox"),
                    ShopOrderTrim = row.Field<string>("ShopOrderTrim"),
                    SchedulingGroup = row.Field<string>("SchedulingGroup"),
                    ProductSpecialist = row.Field<string>("ProductSpecialist"),
                    CommitDate = row.IsNull("CommitDate") ? null : row.Field<DateTime?>("CommitDate"),
                    EnteredDate = row.IsNull("EnteredDate") ? null : row.Field<DateTime?>("EnteredDate"),
                    ReleaseDate = row.IsNull("ReleaseDate") ? null : row.Field<DateTime?>("ReleaseDate"),
                });

            // Retrieve new data from tblOrderStatus to be updated
            string commandStr = $"SELECT [GO Item], [Shop Order], [Shop Order B], [Shop Order T], [Attribute1], [Product Specialist], [Commit Date], [Entered Date], [Release Date] FROM [tblOrderStatus] WHERE [GO Item] IN (SELECT [GO_Item] FROM [{currTable}] WHERE [PageNumber] = 0)";
            DataTable newData = Utility.SearchLP(commandStr);
            foreach (DataRow newRow in newData.Rows)
            {
                string goItem = newRow.Field<string>("GO Item");
                var currentInfo = currentDataMap.ContainsKey(goItem) ? currentDataMap[goItem] : null;

                // Create a new InfoGOIPairs object for each row in newData
                var newInfo = new InfoGOIPairs(goItem)
                {
                    ShopOrderInterior = newRow.Field<string>("Shop Order"),
                    ShopOrderBox = newRow.Field<string>("Shop Order B"),
                    ShopOrderTrim = newRow.Field<string>("Shop Order T"),
                    SchedulingGroup = newRow.Field<string>("Attribute1"),
                    ProductSpecialist = newRow.Field<string>("Product Specialist"),
                    CommitDate = newRow.IsNull("Commit Date") ? null : newRow.Field<DateTime?>("Commit Date"),
                    EnteredDate = newRow.IsNull("Entered Date") ? null : newRow.Field<DateTime?>("Entered Date"),
                    ReleaseDate = newRow.IsNull("Release Date") ? null : newRow.Field<DateTime?>("Release Date"),
                };

                // Check for changes between the current database data and new data
                if (currentInfo != null &&
                    (currentInfo.ShopOrderInterior != newInfo.ShopOrderInterior ||
                     currentInfo.ShopOrderBox != newInfo.ShopOrderBox ||
                     currentInfo.ShopOrderTrim != newInfo.ShopOrderTrim ||
                     currentInfo.SchedulingGroup != newInfo.SchedulingGroup ||
                     currentInfo.ProductSpecialist != newInfo.ProductSpecialist ||
                     currentInfo.CommitDate != newInfo.CommitDate ||
                     currentInfo.EnteredDate != newInfo.EnteredDate ||
                     currentInfo.ReleaseDate != newInfo.ReleaseDate))
                {
                    // If changes exist, then add to InfoGOIPairsList for update
                    newInfo.RequiresUpdate = true;
                    InfoGOIPairsList.Add(newInfo);
                }
            }

            // Update only those GO items that have changes
            foreach (var infoPair in InfoGOIPairsList.Where(i => i.RequiresUpdate))
            {
                string updateStr = $"UPDATE [{currTable}] SET [ShopOrderInterior]=?, [ShopOrderBox]=?, [ShopOrderTrim]=?, [SchedulingGroup]=?, [ProductSpecialist]=?, [CommitDate]=?, [EnteredDate]=?, [ReleaseDate]=? WHERE [GO_Item]='{infoPair.GOI}'";
                using (OleDbCommand cmd = new OleDbCommand(updateStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("[ShopOrderInterior]", infoPair.ShopOrderInterior ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[ShopOrderBox]", infoPair.ShopOrderBox ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[ShopOrderTrim]", infoPair.ShopOrderTrim ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[SchedulingGroup]", infoPair.SchedulingGroup ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[ProductSpecialist]", infoPair.ProductSpecialist ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[CommitDate]", infoPair.CommitDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[EnteredDate]", infoPair.EnteredDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("[ReleaseDate]", infoPair.ReleaseDate ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
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


        private void RegularShippingProcess(string GoItem)
        {
            SetupDirectoriesDate(GoItem);
            SaveBLTfiles(GoItem);           //only for PRL123
            GetDataFromLPDB(GoItem);
            GetCSAinfo(GoItem);
            CleanUpDirectories(GoItem);
        }

        

        //only done for PRL123
        private void SaveBLTfiles(string GOI) 
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123) 
            {
                BitmapImage GOIpage = GetBidman(GOI);
                var path = pdfDirectory + @"\BLT_" + GOI + ".pdf";
                Utility.SaveImageToPdf(path, GOIpage);
            }
        }

        private BitmapImage GetBidman(string GOItem)
        {
            BitmapImage output;
            string imageFilePath = Utility.GetImageFilePath(GOItem, CurrentProduct, 0);
            if (string.IsNullOrEmpty(imageFilePath))  //proceed with bidman byte array to bitmap image
            {
                output = Utility.RetrieveBinaryBidman(GOItem);
            }
            else
            {
                output = Utility.PNGtoBitmap(imageFilePath);
            }
            return output;
        }
        



        private void SetupDirectoriesDate(string GOI)
        {
            pdfDirectory = Utility.GetDirectoryForOrderFiles(GOI, CurrentProduct);
            Directory.CreateDirectory(pdfDirectory + @"\Z_FinalShippedDocuments");

            DateTime dt = DateTime.Now;
            shippedDate = dt.ToShortDateString();
        }

        private void GetDataFromLPDB(string GOI)
        {
            //base query for all similar fields 
            string query = "select [GO_Item], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Urgency], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Catalogue], [ProductSpecialist], [SpecialCustomer], [AMO], [PaintedBox], [DNSB], [Complete], [Short], [Notes], ";
            
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query += "[ServiceEntrance], [RatedNeutral200], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO_Item]='" + GOI + "' and [PageNumber] = 0";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                query += "[ServiceEntrance], [RatedNeutral200], [DoorOverDist], [DoorInDoor] from [PRL4] where [GO_Item]='" + GOI + "' and [PageNumber] = 0";
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                query += "[IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [ThirtyDeepEnclosure] from [PRLCS] where [GO_Item]='" + GOI + "' and [PageNumber]=0";
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
                    else if(CurrentProduct == Utility.ProductGroup.PRLCS) 
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


        private void GetCSAinfo(string GOI)
        {
            DataTableReader values = Utility.GetCSAValues(GOI, CurrentProduct);
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



        private void CleanUpDirectories(string GOI)
        {
            var pdfDir = new DirectoryInfo(pdfDirectory);

            //write TXT file with all Quality Info
            string txtPath = pdfDirectory + @"\Z_FinalShippedDocuments\QualityInfo_" + GOI + ".txt";
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
                "Voltage: " + Voltage, "Hz: " + Hz, "P: " + P, "W: " + W, "ShortCircuitRating: " + ShortCircuitRating, "Amps: " + Amps, "Enclosure: " + Enclosure,
                " ", "Notes: " + Notes };
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

        private void PRLCS_Click(object sender, RoutedEventArgs e)
        {
            PRLCS_Set();
        }

        private void PRL123_Set()
        {
            ProductTable = "PRL123";
            CurrentProduct = Utility.ProductGroup.PRL123;
            PWL123.Background = Brushes.DarkBlue;
            PWL4.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadDeleteGrid();
        }

        private void PRL4_Set()
        {
            ProductTable = "PRL4";
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadDeleteGrid();
        }

        private void PRLCS_Set()
        {
            ProductTable = "PRLCS";
            CurrentProduct = Utility.ProductGroup.PRLCS;
            PWL4.Background = Brushes.Blue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.DarkBlue;
            LoadDeleteGrid();
        }

    }
}



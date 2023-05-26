using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
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

namespace LightningPRO.Views
{
    /// <summary>
    /// Interaction logic for Assembler.xaml
    /// </summary>
    /// 

    //when assembler scans barcode all data is loaded except for bidman drawings

    public partial class Assembler : UserControl
    {
        string[] GOItemsArr;
        string[] AmpsArr;
        string[] BusArr;
        string[] AppearanceArr;
        string[] TorqueArr;
        string[] VoltsArr;
        string[] TypeArr;
        string[] UrgencyArr;
        string[] CustomerArr;

        //Overlapping
        Boolean[] SpecialCustomerArr;
        Boolean[] AMOArr;
        Boolean[] ServiceEntranceArr;
        Boolean[] RatedNeutral200Arr;
        Boolean[] PaintedBoxArr;
        Boolean[] DNSBArr;
        Boolean[] CompleteArr;
        Boolean[] ShortArr;

        //PRL123 Specific
        Boolean[] BoxEarlyArr;
        Boolean[] BoxSentArr;
        Boolean[] DoubleSectionArr;

        //PRL4 Specific 
        Boolean[] DoorOverDistArr;
        Boolean[] DoorInDoorArr;

        //PRLCS Specific
        Boolean[] IncLocLeftArr;
        Boolean[] IncLocRightArr;
        Boolean[] CrossBusArr;
        Boolean[] OpenBottomArr;
        Boolean[] ExtendedTopArr;
        Boolean[] ThirtyDeepEnclosureArr;

        int[] MultiPageNumber;
        int page;

        BitmapImage CurrentImage;
        string ImageFilePath;
        Boolean hasImageFilePath;

        string FilePath;
        Boolean SuccessPull = false;

        Utility.ProductGroup CurrentProduct;
        string ProductTable;


        public Assembler()
        {
            InitializeComponent();
            loadComboBox();
        }

        // used to load assembler employees
        private void loadComboBox()
        {
            try
            {
                DataTableReader rd = Utility.LoadData("select * from [Employee]");
                using (rd)
                {
                    while (rd.Read())
                    {
                        Torque.Items.Add(rd[1].ToString());
                        Appearance.Items.Add(rd[1].ToString());
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable to Load Employees");
            }
        }



        private void PRL123_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL123;
            ProductTable = "PRL123";

            PWL123.Visibility = Visibility.Visible;
            PWL4.Visibility = Visibility.Hidden;
            PWLCS.Visibility = Visibility.Hidden;
            PWLEC.Visibility = Visibility.Hidden;

            checkBoxGridPRL123.Visibility = Visibility.Visible;
            checkBoxGridPRL4.Visibility = Visibility.Hidden;
            checkBoxGridPRLCS.Visibility = Visibility.Hidden;

            DNSB.Visibility = Visibility.Visible;
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            ProductTable = "PRL4";

            PWL123.Visibility = Visibility.Hidden;
            PWL4.Visibility = Visibility.Visible;
            PWLCS.Visibility = Visibility.Hidden;
            PWLEC.Visibility = Visibility.Hidden;

            checkBoxGridPRL123.Visibility = Visibility.Hidden;
            checkBoxGridPRL4.Visibility = Visibility.Visible;
            checkBoxGridPRLCS.Visibility = Visibility.Hidden;

            DNSB.Visibility = Visibility.Hidden;
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            ProductTable = "PRLCS";

            PWL123.Visibility = Visibility.Hidden;
            PWL4.Visibility = Visibility.Hidden;
            PWLCS.Visibility = Visibility.Visible;
            PWLEC.Visibility = Visibility.Hidden;

            checkBoxGridPRL123.Visibility = Visibility.Hidden;
            checkBoxGridPRL4.Visibility = Visibility.Hidden;
            checkBoxGridPRLCS.Visibility = Visibility.Visible;

            DNSB.Visibility = Visibility.Hidden;
        }

        private void LoadDataForPage()
        {
            try
            {
                if (Utility.IsDigitsOnly(Scan.Text)) OrderNumToGOI();
                if (Scan.Text != "" && (((Scan.Text.Length >= 15 || Scan.Text.Length == 10) && !Scan.Text.StartsWith("4-") && !Scan.Text.ToUpper().StartsWith("CS-") && !Scan.Text.ToUpper().StartsWith("EC-")) || ((Scan.Text.Length >= 17 || Scan.Text.Length == 12) && Scan.Text.StartsWith("4-")) || ((Scan.Text.Length >= 17 || Scan.Text.Length == 13) && Scan.Text.ToUpper().StartsWith("CS-"))))
                {
                    if (Scan.Text.StartsWith("4-"))     //PWL4
                    {
                        PRL4_Set();
                        getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [FilePath], [DoorOverDist], [DoorInDoor], [PageNumber] from [PRL4] where [GO]='" + Scan.Text.Substring(2, 10) + "' order by [GO_Item],[PageNumber]");
                    }
                    else if (Scan.Text.ToUpper().StartsWith("CS-"))       //PWLCS
                    {
                        PRLCS_Set();
                        getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [FilePath], [PageNumber] from [PRLCS] where [GO]='" + Scan.Text.Substring(3, 10) + "' order by [GO_Item],[PageNumber]");

                    }
                    else if (Scan.Text.ToUpper().StartsWith("EC-"))       //PWLEC
                    {

                    }
                    else            //PWL123
                    {
                        PRL123_Set();
                        getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [FilePath], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO]='" + Scan.Text.Substring(0, 10) + "' order by [GO_Item]");
                    }

                    if (SuccessPull == true)
                    {
                        First_Page();
                        InsertData();
                        updateStatus(Scan.Text + " SCANNED");
                    }
                    else
                    {
                        updateStatus("ENTRY NOT FOUND");
                    }
                }
                else
                {
                    updateStatus("PLEASE SCAN A VALID GO#");
                }
            }
            catch
            {
                MessageBox.Show("An Error Occurred Loading Data For The Page\nPlease Make Sure The GO# Is Entered Correctly");
            }
            finally 
            {
                Scan.Text = "";
            }
        }


        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            LoadDataForPage();
        }

        private void KeyDownClick(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadDataForPage();
            }
        }

        private void OrderNumToGOI() 
        { 
            string query = "SELECT [PRL123.GO_Item] AS [GO_Item], '' AS Source FROM [PRL123] where [ShopOrderInterior] = '" + Scan.Text + "' OR [ShopOrderTrim] = '" + Scan.Text + "' OR [ShopOrderBox] = '" + Scan.Text + "' UNION SELECT [PRL4.GO_Item] AS [GO_Item],'4-' AS Source FROM [PRL4] where [ShopOrderInterior] = '" + Scan.Text + "' OR [ShopOrderTrim] = '" + Scan.Text + "' OR [ShopOrderBox] = '" + Scan.Text + "' UNION SELECT [PRLCS.GO_Item] AS [GO_Item], 'CS-' AS Source FROM [PRLCS] where [ShopOrderInterior] = '" + Scan.Text + "' OR [ShopOrderTrim] = '" + Scan.Text + "' OR [ShopOrderBox] = '" + Scan.Text + "'";
            DataTableReader rd = Utility.LoadData(query);
            if (rd.HasRows == false)
            {
                return;
            }
            string goi = "", prefix = "";
            using (rd)
            {
                while (rd.Read())
                {
                    goi = rd[0].ToString();
                    prefix = rd[1].ToString();          
                }
            }
            Scan.Text = prefix + goi;
        }


        //gets non-image data
        private void getGOs(string query)
        {
            //get the data twice - once for counters and once for operations
            DataTableReader rd = Utility.LoadData(query);
            DataTableReader rb = Utility.LoadData(query);

            if (rd.HasRows == false)
            {
                MessageBox.Show("Unable To Retrieve Data");
                SuccessPull = false;
                return;
            }
            else
            {
                SuccessPull = true;
            }

            var counter = 0;
            int pages = 0;

            //using the first DataTableReader
            using (rd)
            {
                while (rd.Read())
                {
                    pages++;          //get the number or rows/pages that data has
                }
            }

            //initialize fields with the correct size
            GOItemsArr = new string[pages];
            AmpsArr = new string[pages];
            BusArr = new string[pages];
            AppearanceArr = new string[pages];
            TorqueArr = new string[pages];
            VoltsArr = new string[pages];
            TypeArr = new string[pages];
            UrgencyArr = new string[pages];
            CustomerArr = new string[pages];

            //Overlapping
            SpecialCustomerArr = new Boolean[pages];
            AMOArr = new Boolean[pages];

            ServiceEntranceArr = new Boolean[pages];
            RatedNeutral200Arr = new Boolean[pages];
            PaintedBoxArr = new Boolean[pages];
            DNSBArr = new Boolean[pages];
            CompleteArr = new Boolean[pages];
            ShortArr = new Boolean[pages];

            //PRL123 Specific
            BoxEarlyArr = new Boolean[pages];
            BoxSentArr = new Boolean[pages];
            DoubleSectionArr = new Boolean[pages];

            //PRL4 Specific 
            DoorOverDistArr = new Boolean[pages];
            DoorInDoorArr = new Boolean[pages];

            //PRLCS Specific
            IncLocLeftArr = new Boolean[pages];
            IncLocRightArr = new Boolean[pages];
            CrossBusArr = new Boolean[pages];
            OpenBottomArr = new Boolean[pages];
            ExtendedTopArr = new Boolean[pages];
            ThirtyDeepEnclosureArr = new Boolean[pages];

            MultiPageNumber = new int[pages];


            //using the second DataTableReader
            using (rb)
            {
                while (rb.Read())
                {
                    GOItemsArr[counter] = rb[0].ToString();
                    TypeArr[counter] = rb[1].ToString();
                    VoltsArr[counter] = rb[2].ToString();
                    AmpsArr[counter] = rb[3].ToString();
                    TorqueArr[counter] = rb[4].ToString();
                    AppearanceArr[counter] = rb[5].ToString();
                    BusArr[counter] = rb[6].ToString();
                    UrgencyArr[counter] = rb[7].ToString();

                    CustomerArr[counter] = rb[8].ToString();
                    SpecialCustomerArr[counter] = (Boolean)rb[9];
                    AMOArr[counter] = (Boolean)rb[10];

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[11];
                        RatedNeutral200Arr[counter] = (Boolean)rb[12];
                        PaintedBoxArr[counter] = (Boolean)rb[13];
                        DNSBArr[counter] = (Boolean)rb[14];
                        CompleteArr[counter] = (Boolean)rb[15];
                        ShortArr[counter] = (Boolean)rb[16];
                        FilePath = rb[17].ToString();
                        BoxEarlyArr[counter] = (Boolean)rb[18];
                        BoxSentArr[counter] = (Boolean)rb[19];
                        DoubleSectionArr[counter] = (Boolean)rb[20];
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[11];
                        RatedNeutral200Arr[counter] = (Boolean)rb[12];
                        PaintedBoxArr[counter] = (Boolean)rb[13];
                        DNSBArr[counter] = (Boolean)rb[14];
                        CompleteArr[counter] = (Boolean)rb[15];
                        ShortArr[counter] = (Boolean)rb[16];
                        FilePath = rb[17].ToString();                                             
                        DoorOverDistArr[counter] = (Boolean)rb[18];
                        DoorInDoorArr[counter] = (Boolean)rb[19];
                        MultiPageNumber[counter] = (int)rb[20];
                    }
                    else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        IncLocLeftArr[counter] = (Boolean)rb[11];
                        IncLocRightArr[counter] = (Boolean)rb[12];
                        CrossBusArr[counter] = (Boolean)rb[13];
                        OpenBottomArr[counter] = (Boolean)rb[14];
                        ExtendedTopArr[counter] = (Boolean)rb[15];
                        PaintedBoxArr[counter] = (Boolean)rb[16];
                        ThirtyDeepEnclosureArr[counter] = (Boolean)rb[17];
                        DNSBArr[counter] = (Boolean)rb[18];
                        CompleteArr[counter] = (Boolean)rb[19];
                        ShortArr[counter] = (Boolean)rb[20];
                        FilePath = rb[21].ToString();
                        MultiPageNumber[counter] = (int)rb[22];
                    }
                    counter++;
                }
            }
        }



        private void First_Page()
        {
            page = 0;   //all else fails, start at page 0
            for (int i = 0; i < GOItemsArr.Length; i++)
            {
                if (Scan.Text.Contains(GOItemsArr[i]))
                {
                    page = i;
                    break;
                }
            }
        }


        private void setUrgency()
        {
            if (UrgencyArr[page] == "UP")
            {
                Urgency.Content = "Urgent Paid";
                Urgency.Background = System.Windows.Media.Brushes.Red;
                Stop.Visibility = Visibility.Hidden;
            }
            else if (UrgencyArr[page] == "UU")
            {
                Urgency.Content = "Urgent Unpaid";
                Urgency.Background = System.Windows.Media.Brushes.Green;
                Stop.Visibility = Visibility.Hidden;
            }
            else if (UrgencyArr[page] == "HOLD")
            {
                Urgency.Content = "HOLD";
                Urgency.Background = System.Windows.Media.Brushes.Orange;
                Stop.Visibility = Visibility.Visible;
            }
            else
            {
                Urgency.Content = "Normal";
                Urgency.Background = System.Windows.Media.Brushes.LightGray;
                Stop.Visibility = Visibility.Hidden;
            }
        }


        private void InsertData()
        {
            GO_Item.Text = GOItemsArr[page];
            Amps.Text = AmpsArr[page];
            Bus.Text = BusArr[page];
            Appearance.Text = AppearanceArr[page];
            Torque.Text = TorqueArr[page];
            Volts.Text = VoltsArr[page];
            Type.Text = TypeArr[page];
            setUrgency();

            SpecialCustomer.IsChecked = SpecialCustomerArr[page];
            Customer.Text = CustomerArr[page];
            AMO.IsChecked = AMOArr[page];

            if (AMO.IsChecked == true) AMOButton.Visibility = Visibility.Visible; else AMOButton.Visibility = Visibility.Hidden;
           
            if (string.IsNullOrEmpty(Utility.GetNotes(GO_Item.Text, ProductTable))) btnNotes.Background = System.Windows.Media.Brushes.LightGray; else btnNotes.Background = System.Windows.Media.Brushes.Blue;

            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                //checkBoxGridPRL123
                BoxEarly.IsChecked = BoxEarlyArr[page];
                BoxSent.IsChecked = BoxSentArr[page];
                ServiceEntrance.IsChecked = ServiceEntranceArr[page];
                DoubleSection.IsChecked = DoubleSectionArr[page];
                PaintedBox.IsChecked = PaintedBoxArr[page];
                RatedNeutral200.IsChecked = RatedNeutral200Arr[page];
                DNSB.IsChecked = DNSBArr[page];
                Complete.IsChecked = CompleteArr[page];
                Short.IsChecked = ShortArr[page];
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                //checkBoxGridPRL4
                ServiceEntrance_4.IsChecked = ServiceEntranceArr[page];
                RatedNeutral200_4.IsChecked = RatedNeutral200Arr[page];
                PaintedBox_4.IsChecked = PaintedBoxArr[page];
                DoorOverDist_4.IsChecked = DoorOverDistArr[page];
                DoorInDoor_4.IsChecked = DoorInDoorArr[page];
                Complete_4.IsChecked = CompleteArr[page];
                Short_4.IsChecked = ShortArr[page];
                DNSB_4.IsChecked = DNSBArr[page];
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                //checkBoxGridPRLCS
                IncLocLeft_CS.IsChecked = IncLocLeftArr[page];
                IncLocRight_CS.IsChecked = IncLocRightArr[page];
                CrossBus_CS.IsChecked = CrossBusArr[page];
                OpenBottom_CS.IsChecked = OpenBottomArr[page];
                ExtendedTop_CS.IsChecked = ExtendedTopArr[page];
                PaintedBox_CS.IsChecked = PaintedBoxArr[page];
                ThirtyDeepEnclosure_CS.IsChecked = ThirtyDeepEnclosureArr[page];
                DNSB_CS.IsChecked = DNSBArr[page];
                Complete_CS.IsChecked = CompleteArr[page];
                Short_CS.IsChecked = ShortArr[page];
            }


            CurrentImage = getBidman(GOItemsArr[page]);
            imgEditor.loadImage(CurrentImage);

            pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemsArr.Length.ToString();
        }



        private BitmapImage getBidman(string GOItem)
        {
            BitmapImage output;
            string imageFilePath = Utility.GetImageFilePath(GOItem, CurrentProduct, MultiPageNumber[page]);
            if (string.IsNullOrEmpty(imageFilePath))  //proceed with bidman byte array to bitmap image
            {
                hasImageFilePath = false;
                output = Utility.RetrieveBinaryBidman(GOItem);
            }
            else
            {
                hasImageFilePath = true;
                ImageFilePath = imageFilePath;
                output = Utility.PNGtoBitmap(ImageFilePath);
            }
            LastSave.Content = Utility.GetLastSave(GOItem, CurrentProduct, MultiPageNumber[page]);
            return output;
        }



        private void RefreshData() 
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [FilePath], [BoxEarly], [Box Sent], [DoubleSection] from [PRL123] where [GO]='" + GO_Item.Text.Substring(0, 10) + "' order by [GO_Item]");
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], [DNSB], [Complete], [Short], [FilePath], [DoorOverDist], [DoorInDoor], [PageNumber] from [PRL4] where [GO]='" + GO_Item.Text.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                getGOs("select [GO_Item], [Type], [Volts], [Amps], [Torque], [Appearance], [Bus], [Urgency], [Customer], [SpecialCustomer], [AMO], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [FilePath], [PageNumber] from [PRLCS] where [GO]='" + GO_Item.Text.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SuccessPull == true) 
            {
                FullSave();
                updateStatus(GO_Item.Text + " SAVED");
            }
        }


        private void FullSave()
        {
            if (imgEditor.hasDrawings())
            {
                SaveImage();
            }
            pdfSaveBLT();
            SaveData();

            RefreshData();
            InsertData();
        }


        private void SaveImage()
        {
            if (hasImageFilePath)
            {
                imageFilePathSave(ImageFilePath);
            }
            else
            {
                binaryImageSave();
            }
            Utility.UpdateLastSave(GOItemsArr[page], CurrentProduct, MultiPageNumber[page]);
        }

        private void binaryImageSave() //save image as an array of bytes 
        {
            try
            {
                imgEditor.save();
                string commandStr = "update [PRL123] set [Bidman]=? where [GO_Item]='" + GO_Item.Text + "'";
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("Bidman", Utility.ImageToByte((BitmapImage)imgEditor.Back.Source));
                    cmd.ExecuteNonQuery();
                } //end using command
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute BinaryImageSave");
            }
        }


        private void imageFilePathSave(string imageFilePath)    //save with image file path 
        {
            try
            {
                imgEditor.save();
                Utility.SaveBitmapAsPNGinImages(imageFilePath, (BitmapImage)imgEditor.Back.Source);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute ImageSave");
            }
        }


        private void pdfSaveBLT()
        {
            if (CurrentProduct != Utility.ProductGroup.PRL123)      // PRL123 is created when line item is SENT TO SHIPPING
            {
                var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), "BLT_" + GOItemsArr[page] + "_" + MultiPageNumber[page].ToString() + ".pdf");
                Utility.SaveImageToPdf(output, (BitmapImage)imgEditor.Back.Source);
            }
        }


        private void SaveData()
        {
            try
            {
                string commandStr = "";
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    commandStr = "update [PRL123] set [Type]= ?, [Volts]= ?, [Amps]= ?, [Torque]= ?, [Appearance]= ?, [Bus]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, [RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [BoxEarly]= ?, [Box Sent]= ?, [DoubleSection]= ? where [GO_Item]='" + GOItemsArr[page] + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    commandStr = "update [PRL4] set [Type]= ?, [Volts]= ?, [Amps]= ?, [Torque]= ?, [Appearance]= ?, [Bus]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, [RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [DoorOverDist]= ?, [DoorInDoor]= ? where [GO_Item]='" + GOItemsArr[page] + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    commandStr = "update [PRLCS] set [Type]= ?, [Volts]= ?, [Amps]= ?, [Torque]= ?, [Appearance]= ?, [Bus]= ?, [SpecialCustomer]= ?, [AMO]= ?, [IncLocLeft]= ?, [IncLocRight]= ?, [CrossBus]= ?, [OpenBottom]= ?, [ExtendedTop]= ?, [PaintedBox]= ?, [ThirtyDeepEnclosure]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ? where [GO_Item]='" + GOItemsArr[page] + "'";
                }
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("Type", Type.Text);
                    cmd.Parameters.AddWithValue("Volts", Volts.Text);
                    cmd.Parameters.AddWithValue("Amps", Amps.Text);
                    cmd.Parameters.AddWithValue("Torque", Torque.Text);
                    cmd.Parameters.AddWithValue("Appearance", Appearance.Text);
                    cmd.Parameters.AddWithValue("Bus", Bus.Text);
                    cmd.Parameters.AddWithValue("[SpecialCustomer]", SpecialCustomer.IsChecked);
                    cmd.Parameters.AddWithValue("[AMO]", AMO.IsChecked);

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        cmd.Parameters.AddWithValue("[ServiceEntrance]", ServiceEntrance.IsChecked);
                        cmd.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200.IsChecked);
                        cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox.IsChecked);
                        cmd.Parameters.AddWithValue("[DNSB]", DNSB.IsChecked);
                        cmd.Parameters.AddWithValue("[Complete]", Complete.IsChecked);
                        cmd.Parameters.AddWithValue("[Short]", Short.IsChecked);
                        cmd.Parameters.AddWithValue("[BoxEarly]", BoxEarly.IsChecked);
                        cmd.Parameters.AddWithValue("[Box Sent]", BoxSent.IsChecked);
                        cmd.Parameters.AddWithValue("[DoubleSection]", DoubleSection.IsChecked);
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        cmd.Parameters.AddWithValue("[ServiceEntrance]", ServiceEntrance_4.IsChecked);
                        cmd.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200_4.IsChecked);
                        cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DNSB]", DNSB_4.IsChecked);
                        cmd.Parameters.AddWithValue("[Complete]", Complete_4.IsChecked);
                        cmd.Parameters.AddWithValue("[Short]", Short_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DoorOverDist]", DoorOverDist_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DoorInDoor]", DoorInDoor_4.IsChecked);
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        cmd.Parameters.AddWithValue("[IncLocLeft]", IncLocLeft_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[IncLocRight]", IncLocRight_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[CrossBus]", CrossBus_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[OpenBottom]", OpenBottom_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[ExtendedTop]", ExtendedTop_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[ThirtyDeepEnclosure]", ThirtyDeepEnclosure_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[DNSB]", DNSB_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[Complete]", Complete_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[Short]", Short_CS.IsChecked);
                    }
                    cmd.ExecuteNonQuery();
                } //end using command
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To SaveData");
            }
        }


        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuccessPull == true)
                {
                    if (page < GOItemsArr.Length - 1)
                    {
                        if (imgEditor.hasDrawings())
                        {
                            SaveImage();
                        }
                        pdfSaveBLT();

                        page += 1;
                        InsertData();
                    }
                    pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemsArr.Length.ToString();
                }
            }
            catch
            {
                MessageBox.Show("Unable to Move to Next Page");
            }
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuccessPull == true)
                {
                    if (page != 0)
                    {
                        if (imgEditor.hasDrawings())
                        {
                            SaveImage();
                        }
                        pdfSaveBLT();

                        page -= 1;
                        InsertData();
                    }
                    pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemsArr.Length.ToString();
                }
            }
            catch
            {
                MessageBox.Show("Unable to Move to Previous Page");
            }
        }




        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            if (SuccessPull == true) 
            {
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), "BLT_" + GOItemsArr[page] + ".pdf");
                    Utility.SaveImageToPdf(output, (BitmapImage)imgEditor.Back.Source);
                }

                string commandStr = "update [" + ProductTable + "] set [Tracking]='Shipping' where [GO_Item]='" + GO_Item.Text + "'";
                Utility.ExecuteNonQueryLP(commandStr);

                updateStatus(GO_Item.Text + " SENT TO SHIPPING");
                GC.Collect();
            }
        }



        private void AMO_Clicked(object sender, RoutedEventArgs e)
        {
            if (GO_Item.Text != "")
            {
                AMOList sf = new AMOList(GO_Item.Text);
                sf.Show();
            }
        }


        private void BLT_Insert(object sender, RoutedEventArgs e)
        {
            if (SuccessPull == true)
            {
                string pdfDirectory = Utility.GetDirectoryForOrderFiles(GO_Item.Text, CurrentProduct);
                int check = Utility.ExecuteBLTinsert(GO_Item.Text, pdfDirectory, CurrentProduct);

                if (check == 1)
                {
                    updateStatus(GO_Item.Text + " BLT ADDED");
                }
                else if (check == -2)
                {
                    updateStatus(GO_Item.Text + " BLT ERROR");
                }
            }
        }


        private void Click_ProductRecall(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuccessPull == true)
                {
                    if (MessageBox.Show("You Are About To Send " + GO_Item.Text + "\nBack To The Supervisor.\nWould You Like To Proceed?", "Confirm",
                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + GO_Item.Text + "'";
                        Utility.ExecuteNonQueryLP(query);

                        updateStatus(GO_Item.Text + " RECALLED");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable to Send Back to Supervisor");
            }
        }


        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuccessPull == true)
                {
                    NotesWindow sf = new NotesWindow(GO_Item.Text, ProductTable, btnNotes);
                    sf.Show();
                }
            }
            catch
            {
                MessageBox.Show("Unable to Open GOI Notes");
            }
        }


        private void updateStatus(string command)
        {
            Status.Content = command;
            Status.Foreground = System.Windows.Media.Brushes.Green;
        }

    }
}

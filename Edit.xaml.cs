using System;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using iTextSharp.text.pdf;
using PRL123_Final.Views;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Windows.Controls;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        BitmapImage CurrentBidman;

        string[] GOItemArr;
        string[] GOArr;
        string[] SOIArr;
        string[] SOBArr;
        string[] SOTArr;
        string[] QuantityArr;
        string[] EnteredDateArr;
        string[] ReleaseDateArr;
        string[] CommitDateArr;
        string[] TrackingArr;
        string[] UrgencyArr;
        string[] CustomerArr;

        string FilePath;
        string ImageFilePath;
        Boolean hasImageFilePath;

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

        //PRLCS Labels
        string[] Switchboard;
        string[] CSAStandard;
        string[] SMCenter;
        string[] Section;
        string[] MainBusBarCapacity;
        string[] ShortCircuitRating;
        string[] Amps;


        //PRL123 or PRL4
        string[] ProductID;
        string[] Designation;
        string[] Enclosure;
        string[] MA;
        string[] N;
        string[] Voltage;
        string[] P;
        string[] W;
        string[] XSpaceUsed;
        string[] Ground;
        string[] Hz;

        int[] pgNumber;
        int page;


        string current_ID;
        Utility.ProductGroup CurrentProduct;
        string ProductTable;

        public Edit(string GO, Utility.ProductGroup currProduct)
        {
            InitializeComponent();

            current_ID = GO;
            CurrentProduct = currProduct;

            ShowCheckBoxGridSetProductTBL();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataForPage();
            Update_Page();
            PageData();
        }

        private void LoadDataForPage()
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                getGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Customer], " +
                    "[SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], " +
                    "[DNSB], [Complete], [Short], [FilePath], [BoxEarly], [Box Sent], [DoubleSection] " +
                    "from [PRL123] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item]");
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                getGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Customer], " +
                    "[SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], " +
                    "[DNSB], [Complete], [Short], [FilePath], [DoorOverDist], [DoorInDoor], [PageNumber] " +
                    "from [PRL4] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                getGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Customer], " +
                    "[SpecialCustomer], [AMO], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop],[PaintedBox], " +
                    "[ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [FilePath], [PageNumber] " +
                    "from [PRLCS] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");
            }
        }

        private void getGOs(string query)
        {
            //get the data twice - once for counters and once for operations
            DataTableReader rd = Utility.loadData(query);
            DataTableReader rb = Utility.loadData(query);

            var counter = 0;
            int pages = 0;

            //using the first DataTableReader
            using (rd)
            {
                while (rd.Read())
                {
                    pages++;            //get the number or rows/pages that data has
                }
            }

            //initialize fields with the correct size  
            GOItemArr = new string[pages];
            GOArr = new string[pages];
            SOIArr = new string[pages];
            SOBArr = new string[pages];
            SOTArr = new string[pages];
            QuantityArr = new string[pages];
            EnteredDateArr = new string[pages];
            ReleaseDateArr = new string[pages];
            CommitDateArr = new string[pages];
            TrackingArr = new string[pages];
            UrgencyArr = new string[pages];
            CustomerArr = new string[pages];

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

            //PRLCS Labels
            Switchboard = new string[pages];
            CSAStandard = new string[pages];
            SMCenter = new string[pages];
            Section = new string[pages];
            MainBusBarCapacity = new string[pages];
            ShortCircuitRating = new string[pages];
            Amps = new string[pages];

            //PRL4 and PRL123 Labels
            ProductID = new string[pages];
            Designation = new string[pages];
            Enclosure = new string[pages];
            MA = new string[pages];
            N = new string[pages];
            Voltage = new string[pages];
            P = new string[pages];
            W = new string[pages];
            XSpaceUsed = new string[pages];
            Ground = new string[pages];
            Hz = new string[pages];

            pgNumber = new int[pages];


            //using the second DataTableReader
            using (rb)
            {
                while (rb.Read())
                {
                    GOItemArr[counter] = rb[0].ToString();
                    GOArr[counter] = rb[1].ToString();
                    SOIArr[counter] = rb[2].ToString();
                    SOBArr[counter] = rb[3].ToString();
                    SOTArr[counter] = rb[4].ToString();
                    QuantityArr[counter] = rb[5].ToString();
                    EnteredDateArr[counter] = rb[6].ToString();
                    ReleaseDateArr[counter] = rb[7].ToString();
                    CommitDateArr[counter] = rb[8].ToString();
                    TrackingArr[counter] = rb[9].ToString();
                    UrgencyArr[counter] = rb[10].ToString();
                    CustomerArr[counter] = rb[11].ToString();

                    SpecialCustomerArr[counter] = (Boolean)rb[12];
                    AMOArr[counter] = (Boolean)rb[13];
                    

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[14];
                        RatedNeutral200Arr[counter] = (Boolean)rb[15];
                        PaintedBoxArr[counter] = (Boolean)rb[16];
                        DNSBArr[counter] = (Boolean)rb[17];
                        CompleteArr[counter] = (Boolean)rb[18];
                        ShortArr[counter] = (Boolean)rb[19];

                        FilePath = rb[20].ToString();

                        //PRL123 Specific
                        BoxEarlyArr[counter] = (Boolean)rb[21];
                        BoxSentArr[counter] = (Boolean)rb[22];
                        DoubleSectionArr[counter] = (Boolean)rb[23];
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[14];
                        RatedNeutral200Arr[counter] = (Boolean)rb[15];
                        PaintedBoxArr[counter] = (Boolean)rb[16];
                        DNSBArr[counter] = (Boolean)rb[17];
                        CompleteArr[counter] = (Boolean)rb[18];
                        ShortArr[counter] = (Boolean)rb[19];

                        FilePath = rb[20].ToString();
                        //PRL4 Specific 
                        DoorOverDistArr[counter] = (Boolean)rb[21];
                        DoorInDoorArr[counter] = (Boolean)rb[22];

                        pgNumber[counter] = (int)rb[23];
                    }
                    else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        IncLocLeftArr[counter] = (Boolean)rb[14];
                        IncLocRightArr[counter] = (Boolean)rb[15];
                        CrossBusArr[counter] = (Boolean)rb[16];
                        OpenBottomArr[counter] = (Boolean)rb[17];
                        ExtendedTopArr[counter] = (Boolean)rb[18];
                        PaintedBoxArr[counter] = (Boolean)rb[19];
                        ThirtyDeepEnclosureArr[counter] = (Boolean)rb[20];
                        DNSBArr[counter] = (Boolean)rb[21];
                        CompleteArr[counter] = (Boolean)rb[22];
                        ShortArr[counter] = (Boolean)rb[23];
                        

                        FilePath = rb[24].ToString();
                        pgNumber[counter] = (int)rb[25];
                    }

                    DataTableReader rcsa = Utility.getCSAValues(GOItemArr[counter], CurrentProduct);

                    using (rcsa)
                    {
                        while (rcsa.Read())
                        {
                            if(CurrentProduct == Utility.ProductGroup.PRL123 || CurrentProduct == Utility.ProductGroup.PRL4)
                            {
                                ProductID[counter] = rcsa[14].ToString();
                                Designation[counter] = rcsa[3].ToString();
                                Enclosure[counter] = rcsa[4].ToString();
                                MA[counter] = rcsa[7].ToString();
                                N[counter] = rcsa[5].ToString();
                                XSpaceUsed[counter] = rcsa[6].ToString();
                                Voltage[counter] = rcsa[8].ToString();
                                P[counter] = rcsa[9].ToString();
                                W[counter] = rcsa[10].ToString();
                                Ground[counter] = rcsa[11].ToString();
                                Hz[counter] = rcsa[12].ToString();
                            }
                            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                            {
                                Switchboard[counter] = rcsa[3].ToString();
                                CSAStandard[counter] = rcsa[4].ToString();
                                SMCenter[counter] = rcsa[5].ToString();
                                Section[counter] = rcsa[6].ToString();
                                MainBusBarCapacity[counter] = rcsa[7].ToString();
                                Voltage[counter] = rcsa[8].ToString();
                                Hz[counter] = rcsa[9].ToString();
                                P[counter] = rcsa[10].ToString();
                                W[counter] = rcsa[11].ToString();
                                ShortCircuitRating[counter] = rcsa[12].ToString();
                                Amps[counter] = rcsa[13].ToString();
                                Enclosure[counter] = rcsa[14].ToString();
                                ProductID[counter] = rcsa[16].ToString();
                            }
                        }
                    }
                    counter++;
                }
            }
        }

        private void Update_Page()
        {
            for (int i = 0; i < GOItemArr.Length; i++)
            {
                if (current_ID == GOItemArr[i])
                {
                    page = i;
                    break;
                }
            }
        }


        private void PageData()
        {
            GO_Item.Text = GOItemArr[page];
            GO.Text = GOArr[page];
            ShopOrderInterior.Text = SOIArr[page];
            ShopOrderBox.Text = SOBArr[page];
            ShopOrderTrim.Text = SOTArr[page];
            Quantity.Text = QuantityArr[page];
            EnteredDate.Text = EnteredDateArr[page];
            ReleaseDate.Text = ReleaseDateArr[page];
            CommitDate.Text = CommitDateArr[page];
            Tracking.Text = TrackingArr[page];
            Urgency.Text = UrgencyArr[page];
            ProductIDBox.Text = ProductID[page];

            if (CurrentProduct == Utility.ProductGroup.PRL123 || CurrentProduct == Utility.ProductGroup.PRL4)
            {
                DesignationBox.Text = Designation[page];
                EnclosureBox.Text = Enclosure[page];
                MABox.Text = MA[page];
                NeutBox.Text = N[page];
                VoltageBox.Text = Voltage[page];
                PBox.Text = P[page];
                WBox.Text = W[page];
                XSpaceUsedBox.Text = XSpaceUsed[page];
                GroundBox.Text = Ground[page];
                HzBox.Text = Hz[page];
                SpecialCustomer.IsChecked = SpecialCustomerArr[page];
                Customer.Text = CustomerArr[page];
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                SwitchBoardBoxCS.Text = Switchboard[page];
                CSAStandardBoxCS.Text = CSAStandard[page];
                SMCenterBoxCS.Text = SMCenter[page];
                SectionBoxCS.Text = Section[page];
                MainBusBarCapacityBoxCS.Text = MainBusBarCapacity[page];
                VoltageBoxCS.Text = Voltage[page];
                HzBoxCS.Text = Hz[page];
                PBoxCS.Text = P[page];
                WBoxCS.Text = W[page];
                ShortCircuitBoxCS.Text = ShortCircuitRating[page];
                AmpsBoxCS.Text = Amps[page];
                EnclosureBoxCS.Text = Enclosure[page];
                SpecialCustomer.IsChecked = SpecialCustomerArr[page];
                CustomerBoxCS.Text = CustomerArr[page];
            }

            if (string.IsNullOrEmpty(Utility.getNotes(GO_Item.Text, ProductTable))) btnNotes.Background = System.Windows.Media.Brushes.LightGray; else btnNotes.Background = System.Windows.Media.Brushes.Blue;

            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                //checkBoxGridPRL123
                BoxEarly.IsChecked = BoxEarlyArr[page];
                BoxSent.IsChecked = BoxSentArr[page];
                AMO.IsChecked = AMOArr[page];
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
                AMO_4.IsChecked = AMOArr[page];
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
                AMO_CS.IsChecked = AMOArr[page];
                IncLocLeft_CS.IsChecked = IncLocLeftArr[page];
                IncLocRight_CS.IsChecked = IncLocRightArr[page];
                CrossBus_CS.IsChecked = CrossBusArr[page];
                OpenBottom_CS.IsChecked = OpenBottomArr[page];
                ExtendedTop_CS.IsChecked = ExtendedTopArr[page];
                PaintedBox_CS.IsChecked = PaintedBoxArr[page];
                ThirtyDeepEnc_CS.IsChecked = ThirtyDeepEnclosureArr[page];
                DNSB_CS.IsChecked = DNSBArr[page];
                Complete_CS.IsChecked = CompleteArr[page];
                Short_CS.IsChecked = ShortArr[page];
            }

            CurrentBidman = getBidman(GOItemArr[page]);
            imgEditor.loadImage(CurrentBidman);

            pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemArr.Length.ToString();
        }




        private void PreviousPage() 
        {
            try
            {
                if (page != 0)
                {
                    if (imgEditor.hasDrawings())
                    {
                        SaveImage();
                    }
                    page -= 1;
                    current_ID = GOItemArr[page];
                    PageData();
                }
                pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemArr.Length.ToString();
            }
            catch
            {
                MessageBox.Show("Unable To Change Pages");
            }
        }

        private void NextPage() 
        {
            try
            {
                if (page < GOItemArr.Length - 1)
                {
                    if (imgEditor.hasDrawings())
                    {
                        SaveImage();
                    }
                    page += 1;
                    current_ID = GOItemArr[page];
                    PageData();
                }
                pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemArr.Length.ToString();
            }
            catch
            {
                MessageBox.Show("Unable To Change Pages");
            }
        }



        private void KeyDownClick(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
            {
                PreviousPage();
            }
            else if (e.Key == Key.PageUp)
            {
                NextPage();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            PreviousPage();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }



        private BitmapImage getBidman(string GOItem)
        {
            BitmapImage output;
            string imageFilePath = Utility.getImageFilePath(GOItem, CurrentProduct, pgNumber[page]);
            if (string.IsNullOrEmpty(imageFilePath))  //proceed with bidman byte array to bitmap image
            {
                hasImageFilePath = false;
                output = Utility.retrieveBinaryBidman(GOItem);
            }
            else
            {
                hasImageFilePath = true;
                ImageFilePath = imageFilePath;
                output = Utility.PNGtoBitmap(ImageFilePath);
            }
            LastSave.Content = Utility.getLastSave(GOItem, CurrentProduct, pgNumber[page]);
            return output;
        }


        private void Click_Save(object sender, RoutedEventArgs e)
        {
            if (imgEditor.hasDrawings())
            {
                SaveImage();
            }
            SaveData();

            LoadDataForPage();
            PageData();

            updateStatus(current_ID + " SAVED");
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
            Utility.updateLastSave(current_ID, CurrentProduct, pgNumber[page]);
        }

        private void binaryImageSave() //save image as an array of bytes 
        {
            try
            {
                imgEditor.save();
                string commandStr = "update [PRL123] set [Bidman]=? where [GO_Item]='" + current_ID + "'";
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("Bidman", Utility.ImageToByte((BitmapImage)imgEditor.Back.Source));
                    cmd.ExecuteNonQuery();
                } //end using command
                savePDF();
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
                savePDF();
                Utility.SaveBitmapAsPNGinImages(imageFilePath, (BitmapImage)imgEditor.Back.Source);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute ImageSave");
            }
        }


        private void SaveData()
        {
            try
            {
                string commandStr = "";
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    commandStr = "update [PRL123] set [ShopOrderInterior]= ?, [ShopOrderBox]= ?, [ShopOrderTrim]= ?, [Quantity]= ?, " +
                        "[Tracking]= ?, [Urgency]= ?, [Customer]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, " +
                        "[RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [BoxEarly]= ?, [Box Sent]= ?, " +
                        "[DoubleSection]= ? where [GO_Item]='" + current_ID + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    commandStr = "update [PRL4] set [ShopOrderInterior]= ?, [ShopOrderBox]= ?, [ShopOrderTrim]= ?, [Quantity]= ?, " +
                        "[Tracking]= ?, [Urgency]= ?, [Customer]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, " +
                        "[RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [DoorOverDist]= ?, " +
                        "[DoorInDoor]= ? where [GO_Item]='" + current_ID + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    commandStr = "update [PRLCS] set [ShopOrderInterior]= ?, [ShopOrderBox]= ?, [ShopOrderTrim]= ?, [Quantity]= ?, " +
                    "[Tracking]= ?, [Urgency]= ?, [Customer]= ?, " +
                    "[SpecialCustomer]= ?, [AMO]= ?, [IncLocLeft]= ?, [IncLocRight]= ?, [CrossBus]= ?, [OpenBottom]= ?, " +
                    "[ExtendedTop]= ?,[PaintedBox]= ?, " +
                    "[ThirtyDeepEnclosure]= ?, [DNSB]= ?, [Complete]= ?, [Short] = ?" +
                    "where [GO_Item]='" + current_ID + "'";
                }
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("ShopOrderInterior", ShopOrderInterior.Text);
                    cmd.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    cmd.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    cmd.Parameters.AddWithValue("Quantity", Quantity.Text);
                    cmd.Parameters.AddWithValue("Tracking", Tracking.Text);
                    cmd.Parameters.AddWithValue("Urgency", Urgency.Text);
                    if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        cmd.Parameters.AddWithValue("Customer", CustomerBoxCS.Text);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("Customer", Customer.Text);
                    }
                    cmd.Parameters.AddWithValue("[SpecialCustomer]", SpecialCustomer.IsChecked);

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        cmd.Parameters.AddWithValue("[AMO]", AMO.IsChecked);
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
                        cmd.Parameters.AddWithValue("[AMO]", AMO_4.IsChecked);
                        cmd.Parameters.AddWithValue("[ServiceEntrance]", ServiceEntrance_4.IsChecked);
                        cmd.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200_4.IsChecked);
                        cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DNSB]", DNSB_4.IsChecked);
                        cmd.Parameters.AddWithValue("[Complete]", Complete_4.IsChecked);
                        cmd.Parameters.AddWithValue("[Short]", Short_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DoorOverDist]", DoorOverDist_4.IsChecked);
                        cmd.Parameters.AddWithValue("[DoorInDoor]", DoorInDoor_4.IsChecked);
                    }
                    else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        
                        cmd.Parameters.AddWithValue("[AMO]", AMO_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[IncLocLeft]", IncLocLeft_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[IncLocRight]", IncLocRight_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[CrossBus]", CrossBus_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[OpenBottom]", OpenBottom_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[ExtendedTop]", ExtendedTop_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[ThirtyDeepEnclosure]", ThirtyDeepEnc_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[DNSB]", DNSB_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[Complete]", Complete_CS.IsChecked);
                        cmd.Parameters.AddWithValue("[Short]", Short_CS.IsChecked);
                    }
                    cmd.ExecuteNonQuery();
                } //end using command

                MessageBox.Show("Updating All Data ... ");

                if (CurrentProduct == Utility.ProductGroup.PRL4 || CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    string commandCSA = "update [CSALabel] set [Designation]= ?, [Enclosure]= ?, [N]= ?, [XSpaceUsed]= ?, [MA]= ?, " +
                        "[Voltage]= ?, [P]= ?, [W]= ?, [Ground]= ?, [Hz]= ?, [ProductID]= ? where [GO_Item]='" + current_ID + "'";
                    using (OleDbCommand UpdateCSACommand = new OleDbCommand(commandCSA, MainWindow.LPcon))
                    {
                        UpdateCSACommand.Parameters.AddWithValue("[Designation]", DesignationBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Enclosure]", EnclosureBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[N]", NeutBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[XSpaceUsed]", XSpaceUsedBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[MA]", MABox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Voltage]", VoltageBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[P]", PBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[W]", WBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Ground]", GroundBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Hz]", HzBox.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[ProductID]", ProductIDBox.Text);

                        UpdateCSACommand.ExecuteNonQuery();
                    } //end using command
                }
                else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    string commandCSA = "update [CSALabelPRLCS] set [SwitchBoardTitle]= ?, [CSAStandard]= ?, [SMCenter]= ?, [Section]= ?, [MainBusBarCapacity]= ?, " +
                        "[Voltage]= ?, [Hz]= ?, [P]= ?, [W]= ?, [ShortCircuitRating]= ?, [Amps]= ?, [Enclosure]= ? where [GO_Item]='" + current_ID + "'";
                    using (OleDbCommand UpdateCSACommand = new OleDbCommand(commandCSA, MainWindow.LPcon))
                    {
                        UpdateCSACommand.Parameters.AddWithValue("[SwitchBoardTitle]", SwitchBoardBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[CSAStandard]", CSAStandardBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[SMCenter]", SMCenterBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Section]", SectionBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[MainBusBarCapacity]", MainBusBarCapacityBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Voltage]", VoltageBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Hz]", HzBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[P]", PBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[W]", WBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[ShortCircuitRating]", ShortCircuitBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Amps]", AmpsBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Enclosure]", EnclosureBoxCS.Text);
                        UpdateCSACommand.ExecuteNonQuery();
                    } //end using command
                }
                
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To SaveData");
            }
        }


        private void savePDF()
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), GOItemArr[page] + "_CONSTR.pdf");
                Utility.SaveImageToPdf(output, (BitmapImage)imgEditor.Back.Source);
            }
            else
            {
                var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), GOItemArr[page] + "_" + pgNumber[page].ToString() + "_CONSTR.pdf");
                Utility.SaveImageToPdf(output, (BitmapImage)imgEditor.Back.Source);
            }
        }



        private void Print_Page(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintShopPackagePage sf = new PrintShopPackagePage(CurrentBidman);
                sf.Show();
            }
            catch
            {
                MessageBox.Show("Unable To Print Page");
            }
        }


        private void Click_Replace(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "Image files|*.PDF;*.tif|All files|*.*";
                bool? response = ofg.ShowDialog();
                if (response == true)
                {
                    if (MessageBox.Show("You Are About To Replace The Current Page\nWith The First Page Of The Selected PDF\nWould You Like To Continue?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        string filepath = ofg.FileName;
                        BitmapSource ReplacementImage = Utility.ConvertPDFToImage(filepath)[0];
                        CurrentBidman = (BitmapImage)ReplacementImage;
                        imgEditor.loadImage(CurrentBidman);
                        MessageBox.Show("PDF Image Successfully Uploaded");
                        SaveImage();
                        updateStatus("PAGE " + (page + 1).ToString() + " SUCCESSFULLY REPLACED");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable To Replace Page");
            }
        }


        private void Retrieve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SOI = "";
                string SOB = "";
                string SOT = "";
                string qty = "";
                DateTime? enterDate = null;
                DateTime? releaseDate = null;
                DateTime? commitDate = null;
                string customer = "";
                DataTable dt = Utility.SearchMasterDB("select [Shop Order],[Shop Order B],[Shop Order T],[Qty],[Entered Date],[Release Date],[Commit Date],[Customer] from [tblOrderStatus] where [GO Item]='"+ GO_Item.Text +"'");
                using (DataTableReader dtr = new DataTableReader(dt)) 
                {
                    while (dtr.Read()) 
                    { 
                        SOI = dtr[0].ToString();
                        SOB = dtr[1].ToString();
                        SOT = dtr[2].ToString();
                        qty = dtr[3].ToString();
                        enterDate = string.IsNullOrEmpty(dtr[4].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[4].ToString());
                        releaseDate = string.IsNullOrEmpty(dtr[5].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[5].ToString());
                        commitDate = string.IsNullOrEmpty(dtr[6].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[6].ToString());
                        customer = dtr[7].ToString();
                    }
                }

                string commandStr = "";
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    commandStr = "update [PRL123] set [ShopOrderInterior] = ?,[ShopOrderBox] = ?,[ShopOrderTrim] = ?,[Quantity] = ?,[EnteredDate] = ?,[ReleaseDate] = ?,[CommitDate] = ?,[Customer] = ? where [GO_Item]='" + GO_Item.Text + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4) 
                {
                    commandStr = "update [PRL4] set [ShopOrderInterior] = ?,[ShopOrderBox] = ?,[ShopOrderTrim] = ?,[Quantity] = ?,[EnteredDate] = ?,[ReleaseDate] = ?,[CommitDate] = ?,[Customer] = ? where [GO_Item]='" + GO_Item.Text + "'";
                }
                else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    commandStr = "update [PRLCS] set [ShopOrderInterior] = ?,[ShopOrderBox] = ?,[ShopOrderTrim] = ?,[Quantity] = ?,[EnteredDate] = ?,[ReleaseDate] = ?,[CommitDate] = ?,[Customer] = ? where [GO_Item]='" + GO_Item.Text + "'";
                }
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("[ShopOrderInterior]", SOI);
                    cmd.Parameters.AddWithValue("[ShopOrderBox]", SOB);
                    cmd.Parameters.AddWithValue("[ShopOrderTrim]", SOT);
                    cmd.Parameters.AddWithValue("[Quantity]", qty);
                    if (enterDate == null) cmd.Parameters.AddWithValue("[EnteredDate]", DBNull.Value); else cmd.Parameters.AddWithValue("[EnteredDate]", enterDate);
                    if (releaseDate == null) cmd.Parameters.AddWithValue("[ReleaseDate]", DBNull.Value); else cmd.Parameters.AddWithValue("[ReleaseDate]", releaseDate);
                    if (commitDate == null) cmd.Parameters.AddWithValue("[CommitDate]", DBNull.Value); else cmd.Parameters.AddWithValue("[CommitDate]", commitDate);
                    cmd.Parameters.AddWithValue("[Customer]", customer);
                    cmd.ExecuteNonQuery();
                } //end using command

                MessageBox.Show("Updating Fields From Oracle ... ");

                LoadDataForPage();
                PageData();

                updateStatus(current_ID + " AUTO RETRIEVED DATA");
            }
            catch
            {
                updateStatus("UNABLE TO AUTO RETRIEVE");
                MessageBox.Show("An Error Occurred Trying To AutoRetrieveData");
            }
        }

        private void Notes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotesWindow sf = new NotesWindow(current_ID, ProductTable, btnNotes);
                sf.Show();
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

        private void ShowCheckBoxGridSetProductTBL() 
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                checkBoxGridPRL123.Visibility = Visibility.Visible;
                PRL1234Schema.Visibility = Visibility.Visible;
                ProductTable = "PRL123";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                checkBoxGridPRL4.Visibility = Visibility.Visible;
                PRL1234Schema.Visibility = Visibility.Visible;
                ProductTable = "PRL4";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                checkBoxGridPRLCS.Visibility = Visibility.Visible;
                PRLCSSchema.Visibility = Visibility.Visible;
                ProductTable = "PRLCS";
            }
        }

    }
}

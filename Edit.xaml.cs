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
using LightningPRO.Views;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Windows.Controls;

namespace LightningPRO
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
        string[] SpecialistArr;
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
        Boolean[] OpenBottomArr;
        Boolean[] ExtendedTopArr;
        Boolean[] CrossBusArr;
        Boolean[] ThirtyDeepEnclosureArr;

        //PRLCS Labels
        string[] SwitchboardCS;
        string[] CSAStandardCS;
        string[] SMCenterCS;
        string[] SectionCS;
        string[] MainBusBarCapacityCS;
        string[] VoltageCS;
        string[] HzCS;
        string[] PhaseCS;
        string[] WireCS;
        string[] ShortCircuitRatingCS;
        string[] MaxVoltsCS;
        string[] EnclosureCS;

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
        readonly Utility.ProductGroup CurrentProduct;
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
                GetGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [ProductSpecialist], [Customer], " +
                    "[SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], " +
                    "[DNSB], [Complete], [Short], [FilePath], [BoxEarly], [Box Sent], [DoubleSection] " +
                    "from [PRL123] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item]");
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                GetGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [ProductSpecialist], [Customer], " +
                    "[SpecialCustomer], [AMO], [ServiceEntrance], [RatedNeutral200], [PaintedBox], " +
                    "[DNSB], [Complete], [Short], [FilePath], [DoorOverDist], [DoorInDoor], [PageNumber], [BoxEarly], [BoxSent] " +
                    "from [PRL4] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                GetGOs("select [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Quantity], " +
                    "[EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [ProductSpecialist], [Customer], " +
                    "[SpecialCustomer], [AMO], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], " +
                    "[ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [FilePath], [PageNumber] " +
                    "from [PRLCS] where [GO]='" + current_ID.Substring(0, 10) + "' order by [GO_Item],[PageNumber]");

            }
        }

        private void GetGOs(string query)
        {
            //get the data twice - once for counters and once for operations
            DataTableReader rd = Utility.LoadData(query);
            DataTableReader rb = Utility.LoadData(query);

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
            SpecialistArr = new string[pages];
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
                    SpecialistArr[counter] = rb[11].ToString();
                    CustomerArr[counter] = rb[12].ToString();

                    SpecialCustomerArr[counter] = (Boolean)rb[13];
                    AMOArr[counter] = (Boolean)rb[14];

                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[15];
                        RatedNeutral200Arr[counter] = (Boolean)rb[16];
                        PaintedBoxArr[counter] = (Boolean)rb[17];
                        DNSBArr[counter] = (Boolean)rb[18];
                        CompleteArr[counter] = (Boolean)rb[19];
                        ShortArr[counter] = (Boolean)rb[20];

                        FilePath = rb[21].ToString();
                        BoxEarlyArr[counter] = (Boolean)rb[22];
                        BoxSentArr[counter] = (Boolean)rb[23];
                        //PRL123 Specific
                        DoubleSectionArr[counter] = (Boolean)rb[24];
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        ServiceEntranceArr[counter] = (Boolean)rb[15];
                        RatedNeutral200Arr[counter] = (Boolean)rb[16];
                        PaintedBoxArr[counter] = (Boolean)rb[17];
                        DNSBArr[counter] = (Boolean)rb[18];
                        CompleteArr[counter] = (Boolean)rb[19];
                        ShortArr[counter] = (Boolean)rb[20];

                        FilePath = rb[21].ToString();

                        //PRL4 Specific 
                        DoorOverDistArr[counter] = (Boolean)rb[22];
                        DoorInDoorArr[counter] = (Boolean)rb[23];

                        pgNumber[counter] = (int)rb[24];

                        BoxEarlyArr[counter] = (Boolean)rb[25];
                        BoxSentArr[counter] = (Boolean)rb[26];
                    }
                    else if(CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        IncLocLeftArr[counter] = (Boolean)rb[15];
                        IncLocRightArr[counter] = (Boolean)rb[16];
                        CrossBusArr[counter] = (Boolean)rb[17];
                        OpenBottomArr[counter] = (Boolean)rb[18];
                        ExtendedTopArr[counter] = (Boolean)rb[19];
                        PaintedBoxArr[counter] = (Boolean)rb[20];
                        ThirtyDeepEnclosureArr[counter] = (Boolean)rb[21];
                        DNSBArr[counter] = (Boolean)rb[22];
                        CompleteArr[counter] = (Boolean)rb[23];
                        ShortArr[counter] = (Boolean)rb[24];
                        
                        FilePath = rb[25].ToString();
                        pgNumber[counter] = (int)rb[26];

                        ProductID[counter] = "Pow - R - LineCS";
                    }

                    DataTableReader rcsa = Utility.GetCSAValues(GOItemArr[counter], CurrentProduct);

                    using (rcsa)
                    {
                        while (rcsa.Read())
                        {
                            if (CurrentProduct == Utility.ProductGroup.PRL123 || CurrentProduct == Utility.ProductGroup.PRL4)
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
                            else { break; }
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




        private void CSBack_ClickXML(object sender, RoutedEventArgs e)
        {
            if (CSlabelsPage != 0)
            {
                CSlabelsPage -= 1;
                CSPageData();
            }
            LabelsPage.Content = "Label: " + (CSlabelsPage + 1).ToString() + "/" + SwitchboardCS.Length.ToString();
        }

        private void CSForward_ClickXML(object sender, RoutedEventArgs e)
        {
            if (CSlabelsPage < SwitchboardCS.Length - 1)
            {
                CSlabelsPage += 1;
                CSPageData();
            }
            LabelsPage.Content = "Label: " + (CSlabelsPage + 1).ToString() + "/" + SwitchboardCS.Length.ToString();
        }


        int CSlabelsPage;
        private void GetLabelDataCS()
        {
            DataTableReader tcsa = Utility.GetCSAValues(GOItemArr[page], CurrentProduct);
            DataTableReader rcsa = Utility.GetCSAValues(GOItemArr[page], CurrentProduct);

            var counter = 0;
            int NumLabels = 0;

            //using the first DataTableReader
            using (tcsa)
            {
                while (tcsa.Read())
                {
                    NumLabels++;            //get the number of labels
                }
            }

            //PRLCS Labels
            SwitchboardCS = new string[NumLabels];
            CSAStandardCS = new string[NumLabels];
            SMCenterCS = new string[NumLabels];
            SectionCS = new string[NumLabels];
            MainBusBarCapacityCS = new string[NumLabels];
            VoltageCS = new string[NumLabels];
            HzCS = new string[NumLabels];
            PhaseCS = new string[NumLabels];
            WireCS = new string[NumLabels];
            ShortCircuitRatingCS = new string[NumLabels];
            MaxVoltsCS = new string[NumLabels];
            EnclosureCS = new string[NumLabels];


            using (rcsa)
            {
                while (rcsa.Read())
                {
                    SwitchboardCS[counter] = rcsa[3].ToString();
                    CSAStandardCS[counter] = rcsa[4].ToString();
                    SMCenterCS[counter] = rcsa[5].ToString();
                    SectionCS[counter] = rcsa[6].ToString();
                    MainBusBarCapacityCS[counter] = rcsa[7].ToString();
                    VoltageCS[counter] = rcsa[8].ToString();
                    HzCS[counter] = rcsa[9].ToString();
                    PhaseCS[counter] = rcsa[10].ToString();
                    WireCS[counter] = rcsa[11].ToString();
                    ShortCircuitRatingCS[counter] = rcsa[12].ToString();
                    MaxVoltsCS[counter] = rcsa[13].ToString();
                    EnclosureCS[counter] = rcsa[14].ToString();

                    counter++;
                }
            }
            CSlabelsPage = 0;
        }

        private void CSPageData() 
        {
            SwitchBoardBoxCS.Text = SwitchboardCS[CSlabelsPage];
            CSAStandardBoxCS.Text = CSAStandardCS[CSlabelsPage];
            SMCenterBoxCS.Text = SMCenterCS[CSlabelsPage];
            SectionBoxCS.Text = SectionCS[CSlabelsPage];
            MainBusBarCapacityBoxCS.Text = MainBusBarCapacityCS[CSlabelsPage];
            VoltageBoxCS.Text = VoltageCS[CSlabelsPage];
            HzBoxCS.Text = HzCS[CSlabelsPage];
            PBoxCS.Text = PhaseCS[CSlabelsPage];
            WBoxCS.Text = WireCS[CSlabelsPage];
            ShortCircuitBoxCS.Text = ShortCircuitRatingCS[CSlabelsPage];
            MaxVoltsBoxCS.Text = MaxVoltsCS[CSlabelsPage];
            EnclosureBoxCS.Text = EnclosureCS[CSlabelsPage];

            LabelsPage.Content = "Label: " + (CSlabelsPage + 1).ToString() + "/" + SwitchboardCS.Length.ToString();
        }


        string OldGOI = "";
        private void PageData()
        {
            if (CurrentProduct == Utility.ProductGroup.PRLCS && OldGOI.ToUpper() != GOItemArr[page].ToUpper()) 
            {
                GetLabelDataCS();
                OldGOI = GOItemArr[page];
            }

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
            SpecialistBox.Text = SpecialistArr[page]; 
            SpecialCustomer.IsChecked = SpecialCustomerArr[page];
            Customer.Text = CustomerArr[page];

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
            }
            else if(CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                CSPageData();
            }

            if (string.IsNullOrEmpty(Utility.GetNotes(GO_Item.Text, ProductTable))) btnNotes.Background = System.Windows.Media.Brushes.LightGray; else btnNotes.Background = System.Windows.Media.Brushes.Blue;

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
                BoxEarly_4.IsChecked = BoxEarlyArr[page];
                BoxSent_4.IsChecked = BoxSentArr[page];
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

            CurrentBidman = GetBidman(GOItemArr[page]);
            imgEditor.LoadImage(CurrentBidman);

            pg.Content = "Page: " + (page + 1).ToString() + "/" + GOItemArr.Length.ToString();
        }




        private void PreviousPage() 
        {
            try
            {
                if (page != 0)
                {
                    if (imgEditor.HasDrawings())
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
                    if (imgEditor.HasDrawings())
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



        private BitmapImage GetBidman(string GOItem)
        {
            BitmapImage output;
            string imageFilePath = Utility.GetImageFilePath(GOItem, CurrentProduct, pgNumber[page]);
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
            LastSave.Content = Utility.GetLastSave(GOItem, CurrentProduct, pgNumber[page]);
            return output;
        }


        private void Click_Save(object sender, RoutedEventArgs e)
        {
            if (imgEditor.HasDrawings())
            {
                SaveImage();
            }
            SaveData();

            LoadDataForPage();
            PageData();

            UpdateStatus(current_ID + " SAVED");
        }

        private void SaveImage()
        {
            if (hasImageFilePath)
            {
                ImageFilePathSave(ImageFilePath);
            }
            else
            {
                BinaryImageSave();
            }
            Utility.UpdateLastSave(current_ID, CurrentProduct, pgNumber[page]);
        }

        private void BinaryImageSave() //save image as an array of bytes 
        {
            try
            {
                imgEditor.Save();
                string commandStr = "update [PRL123] set [Bidman]=? where [GO_Item]='" + current_ID + "'";
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("Bidman", Utility.ImageToByte((BitmapImage)imgEditor.Back.Source));
                    cmd.ExecuteNonQuery();
                } //end using command
                SavePDF();
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute BinaryImageSave");
            }
        }


        private void ImageFilePathSave(string imageFilePath)    //save with image file path 
        {
            try
            {
                imgEditor.Save();
                SavePDF();
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
                        "[Tracking]= ?, [Urgency]= ?, [ProductSpecialist] = ?, [Customer]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, " +
                        "[RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [BoxEarly]= ?, [Box Sent]= ?, " +
                        "[DoubleSection]= ? where [GO_Item]='" + current_ID + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    commandStr = "update [PRL4] set [ShopOrderInterior]= ?, [ShopOrderBox]= ?, [ShopOrderTrim]= ?, [Quantity]= ?, " +
                        "[Tracking]= ?, [Urgency]= ?, [ProductSpecialist] = ?, [Customer]= ?, [SpecialCustomer]= ?, [AMO]= ?, [ServiceEntrance]= ?, " +
                        "[RatedNeutral200]= ?, [PaintedBox]= ?, [DNSB]= ?, [Complete]= ?, [Short]= ?, [DoorOverDist]= ?,  " +
                        "[DoorInDoor]= ?, [BoxEarly]= ?, [BoxSent]= ? where [GO_Item]='" + current_ID + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    commandStr = "update [PRLCS] set [ShopOrderInterior]= ?, [ShopOrderBox]= ?, [ShopOrderTrim]= ?, [Quantity]= ?, " +
                        "[Tracking]= ?, [Urgency]= ?, [ProductSpecialist] = ?, [Customer]= ?, [SpecialCustomer]= ?, [AMO]= ?, [IncLocLeft]= ?, "+ 
                        "[IncLocRight]= ?, [CrossBus]= ?, [OpenBottom]= ?, [ExtendedTop]= ?, [PaintedBox]= ?, [ThirtyDeepEnclosure]= ?, [DNSB]= ?, " +
                        "[Complete]= ?, [Short] = ? where [GO_Item]='" + current_ID + "'";
                }
                using (OleDbCommand cmd = new OleDbCommand(commandStr, MainWindow.LPcon))
                {
                    cmd.Parameters.AddWithValue("ShopOrderInterior", ShopOrderInterior.Text);
                    cmd.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    cmd.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    cmd.Parameters.AddWithValue("Quantity", Quantity.Text);
                    cmd.Parameters.AddWithValue("Tracking", Tracking.Text);
                    cmd.Parameters.AddWithValue("Urgency", Urgency.Text);
                    cmd.Parameters.AddWithValue("ProductSpecialist", SpecialistBox.Text);
                    cmd.Parameters.AddWithValue("Customer", Customer.Text);
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
                        cmd.Parameters.AddWithValue("[BoxEarly]", BoxEarly_4.IsChecked);
                        cmd.Parameters.AddWithValue("[BoxSent]", BoxSent_4.IsChecked);
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
                    string commandCSA = "update [CSALabelPRLCS] set [SwitchBoardTitle]= ?, [CSAStandard]= ?, [SMCenter]= ?, [MainBusBarCapacity]= ?, " +
                        "[Voltage]= ?, [Hz]= ?, [P]= ?, [W]= ?, [ShortCircuitRating]= ?, [Amps]= ?, [Enclosure]= ? where [GO_Item]='" + current_ID + "' and [Section]='" + SectionBoxCS.Text + "'";
                    using (OleDbCommand UpdateCSACommand = new OleDbCommand(commandCSA, MainWindow.LPcon))
                    {
                        UpdateCSACommand.Parameters.AddWithValue("[SwitchBoard]", SwitchBoardBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[CSAStandard]", CSAStandardBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[SMCenter]", SMCenterBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[MainBusBarCapacity]", MainBusBarCapacityBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Voltage]", VoltageBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Hz]", HzBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[P]", PBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[W]", WBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[ShortCircuitRating]", ShortCircuitBoxCS.Text);
                        UpdateCSACommand.Parameters.AddWithValue("[Amps]", MaxVoltsBoxCS.Text);
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


        private void SavePDF()
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
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files|*.PDF;*.tif|All files|*.*"
                };
                bool? response = ofg.ShowDialog();
                if (response == true)
                {
                    if (MessageBox.Show("You Are About To Replace The Current Page\nWith The First Page Of The Selected PDF\nWould You Like To Continue?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        string filepath = ofg.FileName;
                        BitmapSource ReplacementImage = Utility.ConvertPDFToImage(filepath)[0];
                        CurrentBidman = (BitmapImage)ReplacementImage;
                        imgEditor.LoadImage(CurrentBidman);
                        MessageBox.Show("PDF Image Successfully Uploaded");
                        SaveImage();
                        UpdateStatus("PAGE " + (page + 1).ToString() + " SUCCESSFULLY REPLACED");
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
                // Execute the modified query to get all GO items for the given GO
                DataTable dt = Utility.SearchMasterDB("select [GO Item],[Shop Order],[Shop Order B],[Shop Order T],[Qty],[Entered Date],[Release Date],[Commit Date],[Customer] from [tblOrderStatus] where [GO]='" + GO.Text + "'");
                using (DataTableReader dtr = new DataTableReader(dt))
                {
                    while (dtr.Read())
                    {
                        string GOItem = dtr[0].ToString();
                        string SOI = dtr[1].ToString();
                        string SOB = dtr[2].ToString();
                        string SOT = dtr[3].ToString();
                        string qty = dtr[4].ToString();
                        DateTime? enterDate = string.IsNullOrEmpty(dtr[5].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[5].ToString());
                        DateTime? releaseDate = string.IsNullOrEmpty(dtr[6].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[6].ToString());
                        DateTime? commitDate = string.IsNullOrEmpty(dtr[7].ToString()) ? null : (DateTime?)Convert.ToDateTime(dtr[7].ToString());
                        string customer = dtr[8].ToString();

                        string commandStr = "update [" + ProductTable + "] set [ShopOrderInterior] = ?,[ShopOrderBox] = ?,[ShopOrderTrim] = ?,[Quantity] = ?,[EnteredDate] = ?,[ReleaseDate] = ?,[CommitDate] = ?,[Customer] = ? where [GO_Item]='" + GOItem + "'";

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
                        }
                    }
                }

                MessageBox.Show("Updating Fields From Oracle ... ");

                LoadDataForPage();
                PageData();

                UpdateStatus(current_ID + " AUTO RETRIEVED DATA");
            }
            catch
            {
                UpdateStatus("UNABLE TO AUTO RETRIEVE");
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


        private void UpdateStatus(string command)
        {
            Status.Content = command;
            Status.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void ShowCheckBoxGridSetProductTBL() 
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                checkBoxGridPRL123.Visibility = Visibility.Visible;
                checkBoxGridPRL4.Visibility = Visibility.Hidden;
                PRL1234Schema.Visibility = Visibility.Visible;
                PRLCSSchema.Visibility = Visibility.Hidden;
                ProductTable = "PRL123";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                checkBoxGridPRL123.Visibility = Visibility.Hidden;
                checkBoxGridPRL4.Visibility = Visibility.Visible;
                PRL1234Schema.Visibility = Visibility.Visible;
                PRLCSSchema.Visibility = Visibility.Hidden;
                ProductTable = "PRL4";
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                checkBoxGridPRL123.Visibility = Visibility.Hidden;
                checkBoxGridPRL4.Visibility = Visibility.Hidden;
                PRL1234Schema.Visibility = Visibility.Hidden;
                PRLCSSchema.Visibility = Visibility.Visible;
                ProductTable = "PRLCS";
            }
        }

    }
}

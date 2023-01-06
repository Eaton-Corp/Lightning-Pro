using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for InsertPRLCS.xaml
    /// </summary>
    public partial class InsertPRLCS : Window
    {

        //text version of image array
        string[] ImagesInText;

        BitmapSource[] image;

        int[] pages;


        int page;
        Boolean XMLLoaded = false;
        Boolean PDFLoaded = false;

        string ProductSpecialist = "";

        int[] SliderShowcase = new int[5];

        System.Windows.Controls.Image[] ImagePreviewObjects;
        System.Windows.Controls.Image[] CheckPreviewObjects;

        Boolean[] SelectedPages;



        /*
         * Insert procedure:
         * Upload XML file and find GO number
         * Upload PDF which is converted to PNG
         * Query MSS database for all entries with that GO number
         * Take entries/PNG and insert for fields within the current Database
         * 
        */


        public InsertPRLCS()
        {
            InitializeComponent();
            page = 0;
            ImagePreviewObjects = new System.Windows.Controls.Image[]
            {
                pg1,
                pg2,
                pg3,
                pg4,
                pg5
            };

            CheckPreviewObjects = new System.Windows.Controls.Image[]
            {
                pg1OverLay,
                pg2OverLay,
                pg3OverLay,
                pg4OverLay,
                pg5OverLay
            };
        }


        Boolean isSpecialCustomer = false;
        Boolean DoorInDoor = false;
        Boolean DoorOverDistribution = false;
        Boolean isServiceEntrance = false;
        Boolean PaintedBox = false;
        Boolean RatedNeutral200 = false;


        //quickly written code

        private void IsSpecialCustomer()
        {
            isSpecialCustomer = false;
            foreach (string client in Views.Configuration.specialCustomer)
            {
                if (Customer.Text.Contains(client))
                {
                    isSpecialCustomer = true;
                }
            }
        }


        private void IsDoorInDoor(int index)
        {
            if (ImagesInText[index].Contains("Door-in-Door"))
            {
                DoorInDoor = true;
            }
            else
            {
                DoorInDoor = false;
            }
        }

        private void IsDoorOverDistribution(int index)
        {
            if (ImagesInText[index].Contains("Door over Devices"))
            {
                DoorOverDistribution = true;
            }
            else
            {
                DoorOverDistribution = false;
            }
        }


        private void IsServiceEntrance(int index)
        {
            if (ImagesInText[index].Contains("Service Entrance"))
            {
                isServiceEntrance = true;
            }
            else
            {
                isServiceEntrance = false;
            }
        }

        private void IsPaintedBox(int index)
        {
            if (ImagesInText[index].Contains("Box Finish"))
            {
                PaintedBox = true;
            }
            else
            {
                PaintedBox = false;
            }
        }

        private void IsRatedNeutral(int index)
        {
            if (ImagesInText[index].Contains("200% Rated Neutral"))
            {
                RatedNeutral200 = true;
            }
            else
            {
                RatedNeutral200 = false;
            }
        }


        //call this before inserting boolean values 
        private void LoadAllBooleanValues(int index)
        {
            IsSpecialCustomer();
            IsDoorInDoor(index);
            IsDoorOverDistribution(index);
            IsServiceEntrance(index);
            IsPaintedBox(index);
            IsRatedNeutral(index);
        }







        public void updateSlider()
        {
            int i = 0;
            while (i + page < image.Length && i < 5)
            {
                SliderShowcase[i] = i + page;
                i++;
            }
            while (i < 5)
            {

                SliderShowcase[i] = -1;
                i++;
            }
            for (int n = 0; n < 5; n++)
            {
                if (SliderShowcase[n] != -1)
                {

                    ImagePreviewObjects[n].Source = image[SliderShowcase[n]];
                    if (SelectedPages[SliderShowcase[n]] == true)
                    {
                        CheckPreviewObjects[n].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CheckPreviewObjects[n].Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    ImagePreviewObjects[n].Source = new BitmapImage();
                    CheckPreviewObjects[n].Visibility = Visibility.Hidden;
                }
            }
        }








        public async Task<int> TurnOnStatusBar()
        {
            pbStatus.Visibility = Visibility.Visible;
            await Task.Delay(500);
            return 1;
        }


        private async void Button_Upload(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "Image files|*.PDF;*.tif|All files|*.*";
                bool? response = ofg.ShowDialog();

                if (response == true)
                {

                    Task<int> ShowProgressBar = TurnOnStatusBar();
                    int result = await ShowProgressBar;

                    string filepath = ofg.FileName;

                    string[] parsePDF = Utility.ExtractTextFromPdf(filepath);

                    int amountOfPages = 0;
                    for (int i = 0; i < parsePDF.Length; i++)
                    {

                        if ((parsePDF[i].Contains("PRL4")) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            amountOfPages++;
                        }

                    }

                    pages = new int[amountOfPages];

                    int counter = 0;
                    for (int i = 0; i < parsePDF.Length; i++)
                    {


                        if ((parsePDF[i].Contains("PRL4")) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            pages[counter] = i;
                            counter++;
                        }

                    }

                    BitmapSource[] allImages = Utility.ConvertPDFToImage(filepath);

                    image = new BitmapSource[pages.Length];
                    for (int i = 0; i < pages.Length; i++)
                    {
                        image[i] = allImages[pages[i]];

                    }

                    //get text version of image array
                    ImagesInText = new string[pages.Length];
                    for (int i = 0; i < pages.Length; i++)
                    {
                        ImagesInText[i] = parsePDF[pages[i]];
                    }


                    page = 0;



                    try
                    {
                        ImagePreviewer.Source = image[page];
                        SelectedPages = new Boolean[image.Length];
                        for (int i = 0; i < SelectedPages.Length; i++)
                        {
                            SelectedPages[i] = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error In PDF");
                        pbStatus.Visibility = Visibility.Hidden;
                        return;
                    }

                    updateSlider();


                    pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
                    if (XMLLoaded == true)
                    {
                        //loadXML(page);
                    }
                    PDFLoaded = true;
                    pbStatus.Visibility = Visibility.Hidden;
                    Status.Content = "PDF SUCCESSFULLY UPLOADED";
                }
            }
            catch
            {
                pbStatus.Visibility = Visibility.Hidden;
                MessageBox.Show("Error Occurred");
            }
        }


        private string FilePathFinder(string GO)
        {
            string OrderFilesPath = ConfigurationManager.ConnectionStrings["orderFiles"].ToString();
            var dir = new DirectoryInfo(OrderFilesPath);

            Boolean Found = false;
            foreach (var file in dir.EnumerateDirectories(GO + "*"))    //all files in target directory that begin with GO
            {
                if (file.Exists && (file.FullName.Equals(dir + @"\" + GO)))
                {
                    Found = true;
                    DirectoryInfo[] arr = file.GetDirectories();
                    Boolean Contains = false;
                    Boolean ContainsImageFolder = false;
                    Boolean ContainsPDFstorage = false;
                    Boolean ContainsAMO = false;
                    Boolean ContainsProjectDocuments = false;

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].Name == "Communication")        //checks if the folder already contains other standard folders
                        {
                            Contains = true;
                        }
                    }
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].Name == "Images")           //specially checks if the folder contains image folder
                        {
                            ContainsImageFolder = true;
                        }
                    }
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].Name == "PDF Storage")           //specially checks if the folder contains PDF storage folder
                        {
                            ContainsPDFstorage = true;
                        }
                    }
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].Name == "AMO")           //specially checks if the folder contains AMO folder
                        {
                            ContainsAMO = true;
                        }
                    }
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].Name == "Project Documents")           //specially checks if the folder contains ProjectDocuments folder
                        {
                            ContainsProjectDocuments = true;                //in the future, write project documents to this folder automatically
                        }
                    }


                    if (!Contains)
                    {
                        Directory.CreateDirectory(file.FullName + @"\Communication");
                        Directory.CreateDirectory(file.FullName + @"\Shop Package");
                        Directory.CreateDirectory(file.FullName + @"\Test Documents");
                    }
                    if (!ContainsImageFolder)
                    {
                        Directory.CreateDirectory(file.FullName + @"\Images");
                    }
                    if (!ContainsPDFstorage)
                    {
                        Directory.CreateDirectory(file.FullName + @"\PDF Storage");
                    }
                    if (!ContainsAMO)
                    {
                        Directory.CreateDirectory(file.FullName + @"\AMO");
                    }
                    if (!ContainsProjectDocuments)
                    {
                        Directory.CreateDirectory(file.FullName + @"\Project Documents");
                    }

                    PathIMAGE.Text = dir.FullName + @"\" + GO + @"\Images" + @"\" + GO;
                    return dir.FullName + @"\" + GO + @"\PDF Storage" + @"\" + GO;
                }
            }
            if (!Found)
            {
                Directory.CreateDirectory(dir + @"\" + GO);
                Directory.CreateDirectory(dir + @"\" + GO + @"\Communication");
                Directory.CreateDirectory(dir + @"\" + GO + @"\AMO");
                Directory.CreateDirectory(dir + @"\" + GO + @"\Project Documents");
                Directory.CreateDirectory(dir + @"\" + GO + @"\Shop Package");
                Directory.CreateDirectory(dir + @"\" + GO + @"\Test Documents");
                Directory.CreateDirectory(dir + @"\" + GO + @"\Images");
                Directory.CreateDirectory(dir + @"\" + GO + @"\PDF Storage");
            }
            PathIMAGE.Text = dir.FullName + @"\" + GO + @"\Images" + @"\" + GO;
            return dir.FullName + @"\" + GO + @"\PDF Storage" + @"\" + GO;
        }















        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (PDFLoaded == true)
            {
                if (page < image.Length - 1)
                {
                    page += 1;
                    ImagePreviewer.Source = image[page];
                }
                pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
                if (XMLLoaded == true)
                {
                    //loadXML(page);

                }
                updateSlider();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (PDFLoaded == true)
            {
                if (page != 0)
                {
                    page -= 1;
                    ImagePreviewer.Source = image[page];
                }
                pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
                if (XMLLoaded == true)
                {
                    //loadXML(page);

                }
                updateSlider();
            }

        }







        private void InsertXmlData()
        {
            Designation.Text = DesignationArr[XMLpage];
            MA.Text = MAArr[XMLpage];
            Voltage.Text = VoltageArr[XMLpage];
            P.Text = Parr[XMLpage];
            W.Text = Warr[XMLpage];
            Ground.Text = GroundArr[XMLpage];
            Hz.Text = HzArr[XMLpage];
            Enclosure.Text = EnclosureArr[XMLpage];
            Xspace.Text = XSpaceUsedArr[XMLpage];
            Neut.Text = Narr[XMLpage];
            GOItemXML.Text = GoItemXmlArr[XMLpage];
        }


        private void Forward_ClickXML(object sender, RoutedEventArgs e)
        {
            if ((XMLpage < DesignationArr.Length - 1) && (XmlUploaded))
            {
                XMLpage += 1;
                InsertXmlData();
            }
        }

        private void Back_ClickXML(object sender, RoutedEventArgs e)
        {
            if ((XMLpage != 0) && (XmlUploaded))
            {
                XMLpage -= 1;
                InsertXmlData();
            }
        }

        int XMLpage;

        Boolean XmlUploaded = false;

        string[] DesignationArr;
        string[] MAArr;
        string[] VoltageArr;
        string[] Parr;
        string[] Warr;
        string[] GroundArr;
        string[] HzArr;
        string[] EnclosureArr;
        string[] XSpaceUsedArr;
        string[] Narr;

        string[] GoItemXmlArr;


        //quick solution implementation

        private async void XML_Upload(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "Image files|*.XML;*.tif|All files|*.*";
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    Task<int> ShowProgressBar = TurnOnStatusBar();
                    int result = await ShowProgressBar;


                    //loads the xml into xDoc using the filepath
                    string filepath = ofg.FileName;
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(filepath);


                    //get the GO number
                    XmlNodeList name = xDoc.GetElementsByTagName("OrderInfo");
                    string outputQuery = name[0].FirstChild.OuterXml.Substring(22, 10);
                    SearchBox.Text = outputQuery;

                    loadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL4names) + " and [GO]='" + SearchBox.Text + "'");

                    //each node is a line item
                    XmlNodeList BMConfiguredLineItemNodes = xDoc.GetElementsByTagName("BMConfiguredLineItem");

                    int NumberOfLineItems = BMConfiguredLineItemNodes.Count;

                    DesignationArr = new string[NumberOfLineItems];
                    MAArr = new string[NumberOfLineItems];
                    VoltageArr = new string[NumberOfLineItems];
                    Parr = new string[NumberOfLineItems];
                    Warr = new string[NumberOfLineItems];
                    GroundArr = new string[NumberOfLineItems];
                    HzArr = new string[NumberOfLineItems];
                    EnclosureArr = new string[NumberOfLineItems];
                    XSpaceUsedArr = new string[NumberOfLineItems];
                    Narr = new string[NumberOfLineItems];
                    GoItemXmlArr = new string[NumberOfLineItems];

                    XMLpage = 0;

                    GetXmlData(BMConfiguredLineItemNodes);       //Here is the heavy XML code

                    InsertXmlData();

                    XmlUploaded = true;

                    pbStatus.Visibility = Visibility.Hidden;
                    Status.Content = "XML SUCCESSFULLY UPLOADED";
                }
            }
            catch
            {
                pbStatus.Visibility = Visibility.Hidden;
                MessageBox.Show("Error Occurred Uploading XML");
            }
        }




        private void GetXmlData(XmlNodeList BMConfiguredLineItemNodes)
        {
            try
            {
                string Enc = "";
                Boolean CatalogFound = false;
                int i = 0;
                foreach (XmlNode lineItemNode in BMConfiguredLineItemNodes)
                {
                    if (lineItemNode.OuterXml.ToString().Contains("Pow-R-Line4") || lineItemNode.OuterXml.ToString().Contains("PRL4"))
                    {
                        XmlNodeList childLineItemNodes = lineItemNode.ChildNodes;
                        string GO = "";
                        string itemNum = "";
                        foreach (XmlNode childNode in childLineItemNodes)
                        {
                            if (childNode.OuterXml.ToString().Contains("\"Designations\""))
                            {
                                DesignationArr[i] = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            }
                            if (childNode.OuterXml.ToString().Contains("\"GONumber\""))
                            {
                                GO = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            }
                            if (childNode.OuterXml.ToString().Contains("\"ItemNumber\""))
                            {
                                itemNum = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                                break;
                            }
                        }

                        GoItemXmlArr[i] = GO + "-" + itemNum;     //GOItem number for line item 

                        XmlNode MainMaterialLineItemNode = lineItemNode.SelectSingleNode("BMLineItem");
                        //Main Description
                        string line = MainMaterialLineItemNode.FirstChild.OuterXml.ToString();
                        //Main CatalogNumber
                        string line2 = MainMaterialLineItemNode.FirstChild.NextSibling.OuterXml.ToString();
                        string V = "nothing";

                        XmlNode MaterialLineItemsNode = lineItemNode.SelectSingleNode("BMLineItems");     //node with all list of materials for line item
                        XmlNodeList MaterialListNodes = MaterialLineItemsNode.ChildNodes;

                        int flag200 = 0;
                        foreach (XmlNode BMLineItem in MaterialListNodes)
                        {
                            if (BMLineItem.OuterXml.ToString().Contains("200% Rated Neutral"))
                            {
                                flag200 = 1;
                            }
                            else
                            {
                                flag200 = 0;
                            }
                        }


                        if (line.Contains("208Y/120V"))
                        {
                            V = "208Y/120V 3Ph 4W";
                            VoltageArr[i] = "208Y/120V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "4";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";

                        }
                        else if (line.Contains("120/240V"))
                        {
                            V = "120/240V 1Ph 3W";
                            VoltageArr[i] = "120 / 240V AC/CA";
                            Parr[i] = "1";
                            Warr[i] = "3";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("600Y/347V"))
                        {
                            V = "600Y/347V 3Ph 4W";
                            VoltageArr[i] = "600Y/347V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "4";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("600V"))
                        {
                            V = "600V 3Ph 3W";
                            VoltageArr[i] = "600V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "3";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("480Y/277V"))
                        {
                            V = "480Y/277V 3Ph 4W";
                            VoltageArr[i] = "480Y/277V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "4";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("480V"))
                        {
                            V = "480V 3Ph 3W";
                            VoltageArr[i] = "480V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "3";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("240V"))
                        {
                            V = "240V 3Ph 3W";
                            VoltageArr[i] = "240V AC/CA";
                            Parr[i] = "3";
                            Warr[i] = "3";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("125/250V DC"))
                        {
                            V = "125/250V DC 3W";
                            VoltageArr[i] = "125/250V DC";
                            Parr[i] = "DC";
                            Warr[i] = "3";
                            GroundArr[i] = "False";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("125V DC") && line.Contains("2W Grounded"))
                        {
                            V = "125V DC 2W Grounded";
                            VoltageArr[i] = "125V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "True";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("125V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "125V DC 2W Ungrounded";
                            VoltageArr[i] = "125V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "False";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("250V DC") && line.Contains("2W Grounded"))
                        {
                            V = "250V DC 2W Grounded";
                            VoltageArr[i] = "250V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "True";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("250V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "250V DC 2W Ungrounded";
                            VoltageArr[i] = "250V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "False";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("600V DC") && line.Contains("2W Grounded"))
                        {
                            V = "600V DC 2W Grounded";
                            VoltageArr[i] = "600V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "True";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("416/240V  AC/CA"))
                        {
                            V = "416/240V 3Ph 4W";
                            VoltageArr[i] = "416/240V";
                            Parr[i] = "3";
                            Warr[i] = "4";
                            GroundArr[i] = "False";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("600V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "240/120V 3Ph 4W A HiLeg";
                            VoltageArr[i] = "600V DC";
                            Parr[i] = "DC";
                            Warr[i] = "2";
                            GroundArr[i] = "False";
                            HzArr[i] = "-";
                        }



                        if (line.Contains("Enclosure"))
                        {
                            if (line.Contains("TYPE1"))
                            {
                                EnclosureArr[i] = "TYPE1";
                            }
                            else if (line.Contains("SPRINKLERPROOF"))
                            {
                                EnclosureArr[i] = "SPRINKLERPROOF";
                            }
                            else if (line.Contains("TYPE12") && (line.Contains("TYPE4X") == false))
                            {
                                EnclosureArr[i] = "TYPE12";
                            }
                            else if (line.Contains("TYPE3R"))
                            {
                                EnclosureArr[i] = "TYPE3R";
                            }
                            else if (line.Contains("TYPE4") && (line.Contains("TYPE4X") == false))
                            {
                                EnclosureArr[i] = "TYPE4";
                            }
                            else if (line.Contains("TYPE4X"))
                            {
                                EnclosureArr[i] = "TYPE4X";
                            }
                            else
                            {
                                EnclosureArr[i] = "No Box";
                            }
                        }



                        if (line.Contains("100A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Narr[i] = "100A";
                            }
                            MAArr[i] = "100A";
                        }
                        else if (line.Contains("225A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Narr[i] = "225A";
                            }
                            MAArr[i] = "225A";
                        }
                        else if (line.Contains("250A"))
                        {
                            if (line.Contains("4W") && line.Contains("DC") == false)
                            {
                                Narr[i] = "250A";
                            }
                            MAArr[i] = "250A";
                        }
                        else if (line.Contains("400A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Narr[i] = "400A";
                            }
                            MAArr[i] = "400A";
                        }
                        else if (line.Contains("600A"))
                        {
                            if (line.Contains("4W") && line.Contains("DC") == false)
                            {
                                Narr[i] = "600A";
                            }
                            MAArr[i] = "600A";
                        }
                        else if (line.Contains("800A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Narr[i] = "800A";
                            }
                            MAArr[i] = "800A";
                        }
                        else if (line.Contains("1200A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Narr[i] = "1200A";
                            }
                            MAArr[i] = "1200A";
                        }



                        if (line.Contains("Ungrounded"))
                        {
                            Narr[i] = "-";
                            Parr[i] = "-";
                            HzArr[i] = "-";
                        }



                        if (line.Contains("Max X-Space for Branch Devices:"))
                        {
                            XSpaceUsedArr[i] = Utility.getBetween(line, "Max X-Space for Branch Devices: ", "\"");
                            CatalogFound = true;
                        }
                        else
                        {
                            if (line2.Contains("CatalogNumber") && CatalogFound == false)
                            {
                                XSpaceUsedArr[i] = Utility.getBetween(line2, "-", "\"");
                                CatalogFound = true;
                            }
                        }


                        if (V.Contains("Grounded"))
                        {
                            GroundArr[i] = "True";
                        }
                        else
                        {
                            GroundArr[i] = "False";
                        }



                        if (flag200 == 1)
                        {
                            if (!Narr[i].Equals("-") && !string.IsNullOrEmpty(Narr[i]))
                            {
                                Narr[i] = (Int32.Parse(Regex.Replace(Narr[i], "[^0-9]", "")) * 2).ToString() + "A";
                            }
                        }




                        foreach (XmlNode BMLineItem in MaterialListNodes)
                        {
                            if (BMLineItem.FirstChild.OuterXml.ToString().Contains("Enclosure"))
                            {
                                Enc = BMLineItem.FirstChild.OuterXml.ToString();
                            }
                        }



                        if (Enc.Contains("Type 12") && (Enc.Contains("TYPE 4X") == false))
                        {
                            EnclosureArr[i] = "TYPE12";
                        }
                        else if (Enc.Contains("SPRINKLERPROOF"))
                        {
                            EnclosureArr[i] = "SPRINKLERPROOF";
                        }
                        else if (Enc.Contains("Type 1"))
                        {
                            EnclosureArr[i] = "TYPE1";
                        }
                        else if (Enc.Contains("Type 2"))
                        {
                            EnclosureArr[i] = "TYPE2";
                        }
                        else if (Enc.Contains("Type 3R"))
                        {
                            EnclosureArr[i] = "TYPE3R";
                        }
                        else if (Enc.Contains("Type 4X"))
                        {
                            EnclosureArr[i] = "TYPE4X";
                        }
                        else if (Enc.Contains("Type 4") && (Enc.Contains("TYPE 4X") == false))
                        {
                            EnclosureArr[i] = "TYPE4";
                        }
                        else
                        {
                            EnclosureArr[i] = "No Box";
                        }

                        i++;
                    }

                }//end for loop of line item

            }
            catch
            {
                MessageBox.Show("Unable To GetXmlData");
            }
        }




        private void loadGrid(string query)
        {
            DataTable dt = Utility.SearchMasterDB(query);
            dg.ItemsSource = dt.DefaultView;
            Urgency.Text = "N";     //default to normal urgency
            ProductID.Text = "Pow-R-Line4";   //always PRL4
        }






        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;

            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;

                GO1.Text = row["GO"];
                GO_Item.Text = row["GO Item"];
                Item.Text = row["Item"];
                Suffix.Text = row["Suffix"];
                ShopOrder.Text = row["Shop Order"].ToString();
                ShopOrderTrim.Text = row["Shop Order T"].ToString();
                ShopOrderBox.Text = row["Shop Order B"].ToString();
                Customer.Text = row["Customer"];
                Qty.Text = row["Qty"].ToString();
                ReleaseDate.Text = row["Release Date"].ToString();
                CommitDate.Text = row["Commit Date"].ToString();
                EnteredDate.Text = row["Entered Date"].ToString();
                Urgency.Text = "N";
                Status.Content = GO_Item.Text + " SELECTED";
                ProductSpecialist = row["Product Specialist"].ToString();
                jobName.Text = row["Job Name"].ToString();
            }

        }









        private int getFirstPageIndex()
        {
            for (int i = 0; i < SelectedPages.Length; i++)
            {
                if (SelectedPages[i] == true)
                {
                    return i;
                }
            }
            return -1;
        }

        private void Insert_Entry(object sender, RoutedEventArgs e)
        {
            if (Utility.isDuplicate(GO_Item.Text, Utility.ProductGroup.PRL4) == false)
            {
                int amountOfPages = 0;
                int firstPageIndex = getFirstPageIndex();
                if (firstPageIndex != -1)
                {
                    for (int i = 0; i < SelectedPages.Length; i++)
                    {
                        if (SelectedPages[i] == true)
                        {
                            InsertPage(i, firstPageIndex);
                            amountOfPages++;
                        }
                    }
                    InsertTestReport(amountOfPages);
                }
            }
            else
            {
                MessageBox.Show("You Have A Duplicate. GO Items Must Be Unique.", "Duplicate Detected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private FormattedText TestReportText(string input)
        {
            return new FormattedText(input, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 36, System.Windows.Media.Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        private void InsertTestReport(int pageNumber)
        {
            if (PDFLoaded == true)
            {

                string reportSourceSR = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString() + "TestReport.png";
                Uri imageURI = new Uri(reportSourceSR, UriKind.RelativeOrAbsolute);
                BitmapImage initailIMG = new BitmapImage(imageURI);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(initailIMG, new Rect(0, 0, initailIMG.Width, initailIMG.Height));
                drawingContext.DrawText(TestReportText(jobName.Text), new System.Windows.Point(385, 460));
                drawingContext.DrawText(TestReportText("Sales Order #"), new System.Windows.Point(1900, 460));
                drawingContext.DrawText(TestReportText(Customer.Text), new System.Windows.Point(385, 520));
                drawingContext.DrawText(TestReportText(GO_Item.Text), new System.Windows.Point(1900, 520));

                drawingContext.DrawText(TestReportText(Views.Configuration.addressLocation[0]), new System.Windows.Point(1620, 70));
                drawingContext.DrawText(TestReportText(Views.Configuration.addressLocation[1]), new System.Windows.Point(1620, 120));
                drawingContext.DrawText(TestReportText(Views.Configuration.addressLocation[2]), new System.Windows.Point(1620, 195));

                drawingContext.Close();
                RenderTargetBitmap renderBmap = new RenderTargetBitmap(initailIMG.PixelWidth, initailIMG.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                renderBmap.Render(drawingVisual);
                BitmapImage img = Utility.TargetToBitmap(renderBmap);


                string Tracking;
                if (Approve.IsChecked == true)
                {
                    Tracking = "MIComplete";
                }
                else
                {
                    Tracking = "InDevelopment";
                }


                String pgNumStr = pageNumber.ToString();
                PathPDF.Text = FilePathFinder(GO1.Text);

                Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathPDF.Text), GO_Item.Text + "_" + pgNumStr + "_CONSTR.pdf"), img);
                Utility.SaveBitmapAsPNGinImages(PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png", img);


                string StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, PageNumber, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);
                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);

                    InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", (Boolean)isSpecialCustomer);

                    InsertCommand.Parameters.AddWithValue("[ServiceEntrance]", (Boolean)isServiceEntrance);
                    InsertCommand.Parameters.AddWithValue("[PaintedBox]", (Boolean)PaintedBox);
                    InsertCommand.Parameters.AddWithValue("[RatedNeutral200]", (Boolean)RatedNeutral200);
                    InsertCommand.Parameters.AddWithValue("[DoorOverDist]", (Boolean)DoorOverDistribution);
                    InsertCommand.Parameters.AddWithValue("[DoorInDoor]", (Boolean)DoorInDoor);


                    InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);
                    InsertCommand.Parameters.AddWithValue("EnteredDate", ReleaseDate.Text);
                    InsertCommand.Parameters.AddWithValue("FilePath", PathPDF.Text);
                    //MessageBox.Show(PathPDF.Text);
                    InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                    //InsertCommand.Parameters.AddWithValue("Suffix", Suffix.Text);
                    InsertCommand.Parameters.AddWithValue("PageNumber", pageNumber);
                    InsertCommand.Parameters.AddWithValue("ImageFilePath", PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png");
                    InsertCommand.ExecuteNonQuery();
                }//end using command

            }
        }



        private void InsertPage(int pageNumber, int firstPgIndex)
        {
            if (PDFLoaded == true)
            {

                string Tracking;
                if (Approve.IsChecked == true)
                {
                    Tracking = "MIComplete";
                }
                else
                {
                    Tracking = "InDevelopment";
                }

                String pgNumStr = (pageNumber - firstPgIndex).ToString();
                PathPDF.Text = FilePathFinder(GO1.Text);

                if ((pageNumber - firstPgIndex) == 0) //first page
                {
                    LoadAllBooleanValues(firstPgIndex);
                    Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathPDF.Text), GO_Item.Text + "_" + pgNumStr + "_CONSTR.pdf"), (BitmapImage)image[pageNumber]);
                    Utility.SaveBitmapAsPNGinImages(PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png", (BitmapImage)image[pageNumber]);
                }
                else
                {
                    Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathPDF.Text), GO_Item.Text + "_" + pgNumStr + "_CONSTR.pdf"), (BitmapImage)image[pageNumber]);
                    Utility.SaveBitmapAsPNGinImages(PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png", (BitmapImage)image[pageNumber]);
                }

                string StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, PageNumber, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);
                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);

                    InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", (Boolean)isSpecialCustomer);

                    InsertCommand.Parameters.AddWithValue("[ServiceEntrance]", (Boolean)isServiceEntrance);
                    InsertCommand.Parameters.AddWithValue("[PaintedBox]", (Boolean)PaintedBox);
                    InsertCommand.Parameters.AddWithValue("[RatedNeutral200]", (Boolean)RatedNeutral200);
                    InsertCommand.Parameters.AddWithValue("[DoorOverDist]", (Boolean)DoorOverDistribution);
                    InsertCommand.Parameters.AddWithValue("[DoorInDoor]", (Boolean)DoorInDoor);


                    InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);
                    InsertCommand.Parameters.AddWithValue("EnteredDate", ReleaseDate.Text);
                    InsertCommand.Parameters.AddWithValue("FilePath", PathPDF.Text);
                    //MessageBox.Show(PathPDF.Text);
                    InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                    //InsertCommand.Parameters.AddWithValue("Suffix", Suffix.Text);
                    InsertCommand.Parameters.AddWithValue("PageNumber", pageNumber - firstPgIndex);
                    InsertCommand.Parameters.AddWithValue("ImageFilePath", PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png");
                    InsertCommand.ExecuteNonQuery();
                }//end using command


                if ((pageNumber - firstPgIndex) == 0)
                {
                    string command = "insert into CSALabel ([GONumber], ItemNumber, Designation, [MA], Voltage, [P], [W], ground, [Hz], GO_Item, ProductID, Enclosure, XSpaceUsed, [N]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand InsertCSA = new OleDbCommand(command, MainWindow.LPcon))
                    {
                        InsertCSA.Parameters.AddWithValue("[GONumber]", GO1.Text);
                        InsertCSA.Parameters.AddWithValue("ItemNumber", Item.Text);
                        InsertCSA.Parameters.AddWithValue("Designation", Designation.Text);
                        InsertCSA.Parameters.AddWithValue("[MA]", MA.Text);
                        InsertCSA.Parameters.AddWithValue("Voltage", Voltage.Text);
                        InsertCSA.Parameters.AddWithValue("[P]", P.Text);
                        InsertCSA.Parameters.AddWithValue("[W]", W.Text);
                        InsertCSA.Parameters.AddWithValue("ground", Ground.Text);
                        InsertCSA.Parameters.AddWithValue("[Hz]", Hz.Text);
                        InsertCSA.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                        InsertCSA.Parameters.AddWithValue("ProductID", ProductID.Text);
                        InsertCSA.Parameters.AddWithValue("Enclosure", Enclosure.Text);
                        InsertCSA.Parameters.AddWithValue("XSpaceUsed", Xspace.Text);
                        InsertCSA.Parameters.AddWithValue("[N]", Neut.Text);

                        InsertCSA.ExecuteNonQuery();
                    }//end using command  
                }

                Status.Content = GO_Item.Text + " SUCCESSFULLY INSERTED";

                DataGridRow dataGridRow = dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) as DataGridRow;
                if (dataGridRow != null)
                    dataGridRow.Background = System.Windows.Media.Brushes.LightGreen;
            }
        }




        private void Search_GOs(object sender, RoutedEventArgs e)
        {
            loadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL4names) + " and [GO]='" + SearchBox.Text + "'");
        }

        private void Pg1_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPages.Length > page)
            {
                if (SelectedPages[page] == false)
                {
                    SelectedPages[page] = true;
                    pg1OverLay.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedPages[page] = false;
                    pg1OverLay.Visibility = Visibility.Hidden;
                }
            }

        }

        private void Pg2_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPages.Length > page + 1)
            {
                if (SelectedPages[page + 1] == false)
                {
                    SelectedPages[page + 1] = true;
                    pg2OverLay.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedPages[page + 1] = false;
                    pg2OverLay.Visibility = Visibility.Hidden;
                }
            }

        }

        private void Pg3_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPages.Length > page + 2)
            {
                if (SelectedPages[page + 2] == false)
                {
                    SelectedPages[page + 2] = true;
                    pg3OverLay.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedPages[page + 2] = false;
                    pg3OverLay.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Pg4_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPages.Length > page + 3)
            {
                if (SelectedPages[page + 3] == false)
                {
                    SelectedPages[page + 3] = true;
                    pg4OverLay.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedPages[page + 3] = false;
                    pg4OverLay.Visibility = Visibility.Hidden;
                }
            }

        }


        private void Pg5_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPages.Length > page + 4)
            {
                if (SelectedPages[page + 4] == false)
                {
                    SelectedPages[page + 4] = true;
                    pg5OverLay.Visibility = Visibility.Visible;
                }
                else
                {
                    SelectedPages[page + 4] = false;
                    pg5OverLay.Visibility = Visibility.Hidden;
                }
            }
        }



    }


}

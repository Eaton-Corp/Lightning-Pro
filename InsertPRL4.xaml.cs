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
using static LightningPRO.Insert;
using iTextSharp.text.pdf.qrcode;
//using System.Windows.Forms;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for Insert.xaml
    /// </summary>
    public partial class InsertPRL4 : Window
    {

        //text version of image array
        string[] ImagesInText;

        BitmapSource[] image;
        Boolean AMOFound;   //boolean indicator for isAMO()

        int[] pages;
        

        int page;
        readonly Boolean XMLLoaded = false;
        Boolean PDFLoaded = false;

        XmlNodeList lines;      //BMConfiguredLineItem nodes -> XmlNodeList of each GoItem
        XmlNodeList BMLines;    //BMLineItems nodes -> Bill of Materials Nodes (Materials Listed Here)

        string ProductSpecialist = "";
        string strPathPDF;

        readonly int[] SliderShowcase = new int[5];

        readonly System.Windows.Controls.Image[] ImagePreviewObjects;
        readonly System.Windows.Controls.Image[] CheckPreviewObjects;

        Boolean[] SelectedPages; 

        
        
        /*
         * Insert procedure:
         * Upload XML file and find GO number
         * Upload PDF which is converted to PNG
         * Query MSS database for all entries with that GO number
         * Take entries/PNG and insert for fields within the current Database
         * 
        */

        
        public InsertPRL4()
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







        public void UpdateSlider()
        {           
            int i = 0;
            while(i + page < image.Length && i < 5)
            {
                SliderShowcase[i] = i + page;
                i++;
            }
            while (i < 5)
            {
                
                SliderShowcase[i] = -1;
                i++;
            }
            for(int n = 0; n < 5; n++)
            {
                if(SliderShowcase[n] != -1)
                {
                    
                    ImagePreviewObjects[n].Source = image[SliderShowcase[n]];
                    if(SelectedPages[SliderShowcase[n]] == true)
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
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files|*.PDF;*.tif|All files|*.*"
                };
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
                        for(int i = 0; i < SelectedPages.Length; i++)
                        {
                            SelectedPages[i] = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error In PDF: {ex.Message}");
                        pbStatus.Visibility = Visibility.Hidden;
                        return;

                    }

                    UpdateSlider();


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
            catch (Exception ex)
            {
                pbStatus.Visibility = Visibility.Hidden;
                MessageBox.Show($"Error Occurred in upload: {ex.Message}");
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
                UpdateSlider();
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
                if(XMLLoaded == true)
                {
                    //loadXML(page);

                }
                UpdateSlider();
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

        private void matchXMLPage(string GOItem)
        {
            for(int i = 0; i < GoItemXmlArr.Length; i++)
            {
                if(GOItem == GoItemXmlArr[i])
                {
                    XMLpage = i;
                    InsertXmlData();
                }
            }
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
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files|*.XML;*.tif|All files|*.*"
                };
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

                    LoadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL4names) + " and [GO]='" + SearchBox.Text + "'");

                    //each node is a line item
                    XmlNodeList BMConfiguredLineItemNodes = xDoc.GetElementsByTagName("BMConfiguredLineItem");
                    lines = BMConfiguredLineItemNodes;



                    int NumberOfLineItems = 0;
                    foreach (XmlNode lineItemNode in BMConfiguredLineItemNodes)
                    {
                        if (lineItemNode.OuterXml.ToString().Contains("Pow-R-Line4") || lineItemNode.OuterXml.ToString().Contains("PRL4"))
                        {
                            NumberOfLineItems++;
                        }
                    }

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

                    //sets global variable BMLines to XMLNodesList where each node has the list of materials for a line item
                    BMLines = xDoc.GetElementsByTagName("BMLineItems");
                    strPathPDF = FilePathFinder(SearchBox.Text);
                    isAMO();


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
                                DesignationArr[i] = Utility.GetBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            }
                            if (childNode.OuterXml.ToString().Contains("\"GONumber\""))
                            {
                                GO = Utility.GetBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            }
                            if (childNode.OuterXml.ToString().Contains("\"ItemNumber\""))
                            {
                                itemNum = Utility.GetBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
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
                            XSpaceUsedArr[i] = Utility.GetBetween(line, "Max X-Space for Branch Devices: ", "\"");
                            CatalogFound = true;
                        }
                        else
                        {
                            if (line2.Contains("CatalogNumber") && CatalogFound == false)
                            {
                                XSpaceUsedArr[i] = Utility.GetBetween(line2, "-", "\"");
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
            catch (Exception ex)
            {
                MessageBox.Show($"Unable To GetXmlData: {ex.Message}");

            }
        }




        private void LoadGrid(string query)
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
                SetRow(row);

                /*GO1.Text = row["GO"];
                GO_Item.Text = row["GO Item"];
                Item.Text = row["Item"];
                SchedulingGroup.Text = row["Attribute1"].ToString();
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
                */
            }

        }

        private void SetRow(dynamic row)
        {
            
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

        bool HasDates = true;
        private Boolean PassPrerequisites () 
        {
            if (string.IsNullOrEmpty(ShopOrder.Text))
            {
                if (MessageBox.Show("You Are About To Insert\nWithout A ShopOrderInterior Number\nWould You Like To Continue?", "No ShopOrderInterior Number",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    Status.Content = GO_Item.Text + " INSERT CANCELLED";
                    return false;
                }
            }

            if (string.IsNullOrEmpty(ReleaseDate.Text) || string.IsNullOrEmpty(CommitDate.Text) || string.IsNullOrEmpty(EnteredDate.Text))
            {
                if (MessageBox.Show("You Are About To Insert With\nMissing Dates (Entered, Commit, Release)\nWould You Like To Continue?", "Missing Dates",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    Status.Content = GO_Item.Text + " INSERT CANCELLED";
                    return false;
                }
                else
                {
                    HasDates = false;
                }
            }
            return true;
        }

        private void Insert_Entry(object sender, RoutedEventArgs e)
        {
            insertSingleEntry();
        }

        private Boolean insertSingleEntry()
        {
            if (Utility.IsDuplicate(GO_Item.Text, Utility.ProductGroup.PRL4) == false)
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
                return false;
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


                string StrInsertCommand = "";
                if (HasDates)
                {
                    StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, SchedulingGroup, Customer, Quantity, Tracking, Urgency, [AMO], NameplateRequired, NameplateOrdered, [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, PageNumber, ImageFilePath, [BoxEarly], [BoxSent]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                }
                else
                {
                    StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, SchedulingGroup, Customer, Quantity, Tracking, Urgency, [AMO], NameplateRequired, NameplateOrdered, [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], FilePath, ProductSpecialist, PageNumber, ImageFilePath, [BoxEarly], [BoxSent]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                }

                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("SchedulingGroup", SchedulingGroup.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);
                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);
                    InsertCommand.Parameters.AddWithValue("NameplateRequired", NameplateRequired.IsChecked);
                    InsertCommand.Parameters.AddWithValue("NameplateOrdered", NameplateOrdered.IsChecked);

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
                    InsertCommand.Parameters.AddWithValue("BoxEarly", BoxEarly.IsChecked);
                    InsertCommand.Parameters.AddWithValue("BoxSent", BoxSent.IsChecked);

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

                string StrInsertCommand = "";
                if (HasDates)
                {
                    StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, SchedulingGroup, Customer, Quantity, Tracking, Urgency, [AMO], NameplateRequired, NameplateOrdered, [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, PageNumber, ImageFilePath, BoxEarly, BoxSent) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                }
                else
                {
                    StrInsertCommand = "Insert into PRL4 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, SchedulingGroup, Customer, Quantity, Tracking, Urgency, [AMO], NameplateRequired, NameplateOrdered, [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], FilePath, ProductSpecialist, PageNumber, ImageFilePath, BoxEarly, BoxSent) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                }

                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("SchedulingGroup", SchedulingGroup.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);
                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);
                    InsertCommand.Parameters.AddWithValue("NameplateRequired", NameplateRequired.IsChecked);
                    InsertCommand.Parameters.AddWithValue("NameplateOrdered", NameplateOrdered.IsChecked);

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
                    InsertCommand.Parameters.AddWithValue("BoxEarly", BoxEarly.IsChecked);
                    InsertCommand.Parameters.AddWithValue("BoxSent", BoxSent.IsChecked);
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

                if (dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) is DataGridRow dataGridRow)
                    dataGridRow.Background = System.Windows.Media.Brushes.LightGreen;
            }
        }




        private void Search_GOs(object sender, RoutedEventArgs e)
        {
            LoadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL4names) + " and [GO]='" + SearchBox.Text + "'");
        }

        private void Auto_Insert(object sender, RoutedEventArgs e)
        {
            AutomaticInsert();
        }


        public class GOItemPages
        {
            public List<int> Pages { get; set; }
        }

        public class GOData
        {
            public string GO { get; set; }
            public Dictionary<string, GOItemPages> GOItems { get; set; }
        }

        public static class PDFPageOrganizer
        {
            public static GOData FindPagesForGOItem(string[] imagesInText, string goKeyword)
            {
                GOData goData = new GOData
                {
                    GO = null,
                    GOItems = new Dictionary<string, GOItemPages>()
                };

                goData.GO = goKeyword;
                Regex goItemRegex = new Regex($@"{goKeyword}\s+(?<GOItem>\d+[A-Za-z])\s+(?<Page>\d+)\s+of\s+(?<TotalPages>\d+)", RegexOptions.IgnoreCase);


                for (int pageIndex = 0; pageIndex < imagesInText.Length; pageIndex++)
                {
                    string pageText = imagesInText[pageIndex];
                    Match match = goItemRegex.Match(pageText);
                    if (match.Success)
                    {
                        string GOItem = match.Groups["GOItem"].Value;
                        string PageNumber = match.Groups["Page"].Value;
                        string TotalPages = match.Groups["TotalPages"].Value;

                        if (!goData.GOItems.ContainsKey(GOItem))
                        {
                            goData.GOItems.Add(GOItem, new GOItemPages { Pages = new List<int>() });
                        }

                        goData.GOItems[GOItem].Pages.Add(pageIndex);

                    }

                }
                return goData;
            }

            public static string GODataToString(GOData goData)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine($"GO: {goData.GO}");
                foreach (var goItemPage in goData.GOItems)
                {
                    stringBuilder.AppendLine($"GOItem: {goItemPage.Key}, Pages: [{string.Join(", ", goItemPage.Value.Pages)}]");
                }

                return stringBuilder.ToString();
            }
        }

        public string GetFirstGO(DataGrid dataGrid)
        {
            foreach (var dataGridRow in dataGrid.Items)
            {
                DataRowView rowView = dataGridRow as DataRowView;

                if (rowView != null && rowView.Row.Table.Columns.Contains("GO"))
                {
                    return rowView["GO"].ToString();
                }
            }

            // Return null (or any default value) if no "GO" is found in the DataGrid
            return null;
        }

        public List<int> GetGOItemPages(string goItem, GOData goData)
        {
            if (goData.GOItems.TryGetValue(goItem, out GOItemPages goItemPages))
            {
                return goItemPages.Pages;
            }
            else
            {
                return new List<int>(); // Return an empty list if the GOItem is not found in GOData
            }
        }

        private void AutomaticInsert()
        {
            //pbStatus.Visibility = Visibility.Visible;
            try
            {
            //returns the pageArray for the 
            GOData result = PDFPageOrganizer.FindPagesForGOItem(ImagesInText, GetFirstGO(dg));

                string status = "Successfully inserted items:\n"; // Initialize with a newline
                // Output the result as a loggable string
                string logString = PDFPageOrganizer.GODataToString(result);
                System.Windows.MessageBox.Show(logString);

                if (dg.ItemsSource == null)
                {
                    System.Windows.MessageBox.Show("DataGrid is empty.");
                    return;
                }

                foreach (var dataGridRow in dg.Items)
                {
                    DataRowView rowView = dataGridRow as DataRowView;

                    if (rowView != null)
                    {
                        List<int> matchingPage = GetGOItemPages(rowView["Item"].ToString(), result);

                        if (matchingPage.Count > 0)
                        {
                            
                            SetRow(rowView);
                            matchXMLPage(rowView["GO Item"].ToString());

                            foreach (int pageNumber in matchingPage)
                            {
                                SelectedPages[pageNumber] = true;
                            }
                            Boolean insertionStatus = insertSingleEntry();
                            if (insertionStatus)
                            {
                                status += rowView["Item"] + "\n"; // Append the item name with a newline
                            }
                            for (int i = 0; i < SelectedPages.Length; i++)
                            {
                                SelectedPages[i] = false;
                            }
                        }
                    }
                }
                MessageBox.Show(status);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when trying to auto insert drawings : {ex.Message}");
            }

            //pbStatus.Visibility = Visibility.Hidden;
        }


        private string convertToRequired(string partname, int enclosure, int paint, int mount, int multiple)
        {
            string output = partname;

            if (output != null)
            {
                if (!(output.StartsWith("CN")) && !(output.StartsWith("ItemNumber")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P25")) && !(output.StartsWith("A29")) && !(output.StartsWith("H5")) && !(output.StartsWith("SP")) && !(output.StartsWith("BX")))
                {
                    if (output.StartsWith("JD3") || output.StartsWith("JDB3") || output.StartsWith("HJD3") || output.StartsWith("KD3") || output.StartsWith("KDB3") && !(output.StartsWith("JDB3070") || output.StartsWith("JDB3090") || output.StartsWith("JD3070") || output.StartsWith("JD3090") || output.StartsWith("HJD3070") || output.StartsWith("HJD3090")))
                    {
                        output = "HKD3400F";
                    }

                    else if (output.StartsWith("JD2") || output.StartsWith("JDB2") || output.StartsWith("HJD2") || output.StartsWith("KD2") || output.StartsWith("KDB3") && !(output.StartsWith("JDB2070") || output.StartsWith("JDB2090") || output.StartsWith("JD2070") || output.StartsWith("JD2090") || output.StartsWith("HJD3070") || output.StartsWith("HJD2090")))
                    {
                        output = "HKD3400F";
                    }

                    else if (output.StartsWith("DK2") || output.StartsWith("DK3"))
                    {
                        output = output.Remove(0, 2);
                    }

                    else if (output.StartsWith("LG"))
                    {
                        //what to do if we have an LG breaker
                        //previous implementation
                        //Sends message to Label
                        MessageBox.Show("LG Breaker Present");
                    }

                    else if (output.StartsWith("EZB") && output.EndsWith("RCSP") && multiple == 1 && mount == 0)
                    {
                        output = EnclosureFlush1(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && output.EndsWith("SPEC") && multiple == 1 && mount == 0)
                    {
                        output = EnclosureFlush(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && output.EndsWith("RCSP") && multiple == 1 && mount == 1)
                    {
                        output = EnclosureSurface1(output, enclosure, paint);
                    }

                    else if (output.StartsWith("EZB") && output.EndsWith("SPEC") && multiple == 1 && mount == 1)
                    {
                        output = EnclosureSurface(output, enclosure, paint);
                    }

                    else if (output.StartsWith("EZB") && multiple == 3 && mount == 0)
                    {
                        output = TripleFlush(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple == 3 && mount == 1)
                    {
                        output = TripleSurface(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple == 2 && mount == 0)
                    {
                        output = DoubleFlush(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple == 2 && mount == 1)
                    {
                        output = DoubleSurface(output, enclosure, paint);
                    }
                }
            }
            return output;
        }


        // 8 functions of Random Ass Logic to alter the partname

        private string EnclosureSurface1(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;

            if (enclosure == 0 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("RCSP", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZBP", "EZT");
                a = a.Replace("RC", "S");
            }
            else if (enclosure == 0 && !(paint == 1))
            {
                a = a.Replace("RCSP", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZB", "EZT");
                a = a.Replace("RC", "S");
            }
            else if ((enclosure == 0 || enclosure == 2) && !(paint == 1))
            {
                a = a.Replace("RCSP", "RC");
            }
            else if ((enclosure == 1 || enclosure == 2) && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("RCSP", "RC");
            }

            if (enclosure == 0 || enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H01");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }


        private string EnclosureFlush1(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (enclosure == 0 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("RCSP", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZBP", "EZT");
                a = a.Replace("RC", "F");
            }
            else if (enclosure == 0 && !(paint == 1))
            {
                a = a.Replace("RCSP", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZB", "EZT");
                a = a.Replace("RC", "F");
            }
            else if ((enclosure == 1 || enclosure == 2) && !(paint == 1))
            {
                a = a.Replace("RCSP", "RC");
            }
            else if ((enclosure == 1 || enclosure == 2) && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("RCSP", "RC");
            }

            if (enclosure == 0 || enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H01");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }

        private string EnclosureFlush(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (enclosure == 0 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZBP", "EZT");
                a = a.Replace("RC", "F");
            }
            else if (enclosure == 0 && !(paint == 1))
            {
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZB", "EZT");
                a = a.Replace("RC", "F");
            }
            else if (enclosure == 1 || enclosure == 2 && !(paint == 1))
            {
                a = a.Replace("SPEC", "RC");
            }
            else if (enclosure == 1 || enclosure == 2 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("SPEC", "RC");
            }

            if (enclosure == 0 || enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H01");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }

        private string EnclosureSurface(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;

            if (enclosure == 0 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZBP", "EZT");
                a = a.Replace("RC", "S");
            }
            else if (enclosure == 0 && !(paint == 1))
            {
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("EZB", "EZT");
                a = a.Replace("RC", "S");
            }
            else if (enclosure == 1 || enclosure == 2 && !(paint == 1))
            {p
                a = a.Replace("SPEC", "RC");
            }
            else if (enclosure == 1 || enclosure == 2 && paint == 1)
            {
                a = a.Replace("EZB", "EZBP");
                a = a.Replace("SPEC", "RC");
            }

            if (enclosure == 0 || enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H01");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }


        private string TripleFlush(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (!(paint == 1))
            {
                a = a.Replace("EZB", "CTR-EZB");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("CTR-EZB", "C-EZB");
            }
            else if (paint == 1)
            {
                a = a.Replace("EZB", "CTR-EZBP");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("CTR-EZBP", "C-EZBP");
            }

            if (enclosure == 1 && enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H03");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }



        private string TripleSurface(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (!(paint == 1))
            {
                a = a.Replace("EZB", "CTR-EZB");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
                a = a.Replace("CTR-EZB", "C-EZB");
                DictionaryLoop(a, 1);
                a = a.Replace("C-EZB", "EZT");
                a = a.Replace("RC", "S");
            }
            else if (paint == 1)
            {
                a = a.Replace("EZB", "CTR-EZBP");
                a = a.Replace("SPEC:3", "RC:1");
                DictionaryLoop(a, 1);
                a = a.Replace("CTR-EZBP", "C-EZBP");
                DictionaryLoop(a, 1);
                a = a.Replace("C-EZBP", "EZT");
                a = a.Replace("RC", "S");
            }

            if (enclosure == 1 && enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H03");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }


        private string DoubleFlush(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (!(paint == 1))
            {
                a = a.Replace("EZB", "C-EZB");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
            }
            else if (paint == 1)
            {
                a = a.Replace("EZB", "C-EZBP");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);
            }

            if (e == 0 || e == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H02");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }

        private string DoubleSurface(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if (!(paint == 1))
            {
                a = a.Replace("EZB", "C-EZB");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);

                a = a.Replace("C-EZB", "EZT");
                a = a.Replace("RC", "S");
            }
            else if (paint == 1)
            {
                a = a.Replace("EZB", "C-EZBP");
                a = a.Replace("SPEC", "RC");
                DictionaryLoop(a, 1);

                a = a.Replace("C-EZBP", "EZT");
                a = a.Replace("RC", "S");
            }
            if (enclosure == 0 || enclosure == 2)
            {
                string RainCover = Utility.ReplacePart("CE24331H02");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }


        private void DictionaryLoop(string par, int quantity)
        {
            if (!(par.Contains("Enclosure") && !(par.Contains("Mount")) && !(par.Contains("SpecialTrim")) && !(par.Contains("Neutral")) && !(par.Contains("Define on Notes Tab"))))
            {
                if (ContainsPart(partslist, par))
                {
                    partslist[FindIndexPartsList(partslist, par)].addToQuantity(1);
                }
                else
                {
                    partslist.Add(new part(par, quantity));
                }
            }
        }

        public class part
        {
            private string partName;
            private int Quantity;

            public part(string partNumber, int Amount)
            {
                partName = partNumber;
                Quantity = Amount;
            }

            public int Get_Amount()
            {
                return this.Quantity;
            }

            public string Get_partName()
            {
                return this.partName;
            }

            public void addToQuantity(int numberOfPeices)
            {
                this.Quantity += numberOfPeices;
            }

            public void setName(string name)
            {
                this.partName = name;
            }
        }


        // Used to determine if a list of parts contains a given part
        public Boolean ContainsPart(List<part> parts, string part)
        {
            if (parts != null)
            {
                foreach (part p in parts)
                {
                    if (p.Get_partName().Contains(part))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Used to determine the index of a part within a parts list
        public int FindIndexPartsList(List<part> parts, string part)
        {
            int counter = 0;
            foreach (part p in parts)
            {
                if (p.Get_partName().Contains(part))
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }



        public enum info
        {
            None, AMO, KanbanSpike
        }
        /*int mount;
        int enclosure;
        int paint;*/


        //final list that is displayed on AMOdg
        public class PartsList
        {
            public string PartName { get; set; }
            public int Quantity { get; set; }
            public info Status { get; set; }
            public string IsActive { get; set;}
            public string Description { get; set; }
            public string GO {get; set;}
        }


        List<part> partslist;
        private void isAMO()
        {

            //look through to find all parts with amount 
            //do this via an object to amount system 
            //each part has an object number which is a string and the value which is the quantity --> note this gets added to the object amount
            //once found convert names using flipping database
            //then use database to check for AMO spike ie 1/2 a kanban card
            //also check if AMO part


            string AMOreport = "";
            partslist = new List<part>();

            int counter = 0;
            foreach (XmlNode node in BMLines)    //iterate through BMLines (BMLineItems -> Materials List Information)
            {
                if (node.OuterXml.ToString().Contains("PRL4"))
                {

                    XmlNodeList n = node.ChildNodes;    //NodeList of all BMLineItem under BMLineItems

                    int mount = -1;
                    int encl = -1;
                    int paint = -1;
                    int multiple = 1;
                    foreach (XmlNode kid in lines[counter])  //NodeList of each line GoItem (BMConfiguredLineItem)
                    {
                        string output = kid.OuterXml.ToString();    //line item node by node  

                        if (!(output.StartsWith("CN")) && !(output.StartsWith("ItemNumber")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("Start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P25")) && !(output.StartsWith("A29")) && !(output.StartsWith("H5")) && !(output.StartsWith("SP")) && !(output.StartsWith("BX")))
                        {
                            if (output.Contains("2P Main Breaker"))
                            {
                                MessageBox.Show("Please Add 2P Main Breaker Implementation");
                            }
                            if (output.Contains("310+LS") || output.Contains("310+LSI") || output.Contains("310+LSG") || output.Contains("310+LSIG") || output.Contains("310+ALSI") || output.Contains("Optim550 LSI"))
                            {
                                //Some sort of trip as well
                                MessageBox.Show("310+ Trip");
                            }
                        }

                        if (output.Contains("Painted Box - ASA 61 Grey"))
                        {
                            paint = 1;
                        }
                        else if (output.Contains("Painted Box - Special Colour"))
                        {
                            //Label 6 check
                            MessageBox.Show("Special Colour");
                        }
                        else if (output.Contains("Trim, EZ Trim, Painted") || (output.Contains("Special Trim")))
                        {
                            //Label 7 check
                            MessageBox.Show("Painted Box trim");
                        }

                        else if (output.StartsWith("Special") && (!(output.Contains("Trim")) || !(output.Contains("Box"))))
                        {
                            output = "EZB-Error";
                            paint = 0;
                        }

                        if (output.Contains("Enclosure") && output.Contains("SPRINKLERPROOF"))
                        {
                            encl = 0;
                        }
                        else if (output.Contains("Enclosure") && output.Contains("Type 1"))
                        {
                            encl = 1;
                        }
                        else if (output.Contains("Enclosure") && output.Contains("Type 2"))
                        {
                            encl = 2;
                        }

                        if (output.Contains("Flush") && output.Contains("3Ph") || output.Contains("1Ph"))
                        {
                            mount = 0;
                        }
                        else if (output.Contains("Surface") && output.Contains("3Ph") || output.Contains("1Ph"))
                        {
                            mount = 1;
                        }

                        if (output.Contains("2 Sections") && output.Contains("Multi-Section Panel"))
                        {
                            multiple = 2;
                        }
                        else if (output.Contains("3 Sections") && output.Contains("Multi-Section Panel"))
                        {
                            multiple = 3;
                        }
                    }// end for loop for line item


                    foreach (XmlNode x in n)    //n is NodeList of all BMLineItem under BMLineItems
                    {
                        XmlNodeList m = x.ChildNodes;      //Attributes of BMLineItem

                        part currentpart = new part("HOLDUP", 0);

                        foreach (XmlNode child in m)    //loop each attribute line in BMLineItem
                        {
                            if (child.OuterXml.ToString().Contains("\"CatalogNumber\""))                    //get CatalogNumber value and alter it using convertToRequired
                            {
                                string output = Utility.GetBetween(child.OuterXml.ToString(), "V=\"", "\"");
                                if (!(output.StartsWith("CN")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P2")) && !(output == "C1") && !(output.StartsWith("A29")) && !(output.StartsWith("H5")) && !(output.StartsWith("SP")) && !(output.StartsWith("BX")))
                                {
                                    currentpart.setName(convertToRequired(output, encl, paint, mount, multiple));
                                }
                            }
                            if (child.OuterXml.ToString().Contains("\"Quantity\""))             //get quantity value of current part
                            {
                                currentpart.addToQuantity(Int32.Parse(Utility.GetBetween(child.OuterXml.ToString(), "V=\"", "\"")));
                            }
                        }//end for loop attributes

                        if (ContainsPart(partslist, currentpart.Get_partName()))
                        {
                            partslist[FindIndexPartsList(partslist, currentpart.Get_partName())].addToQuantity(currentpart.Get_Amount());
                        }
                        else if (!currentpart.Get_partName().Contains("HOLDUP"))
                        {
                            partslist.Add(currentpart);
                        }
                    }//end for loop of BMLineItem
                    counter++;
                }
            }//end for loop with BMLines



            //get parts list ready for AMOdg; get IsActive and Description + get status info -> AMO, KanBanKpike, or None
            List<info> statuses = new List<info>();
            List<string> EnableOrDisable = new List<string>();
            List<string> Description = new List<string>();

            for (int i = 0; i < partslist.Count; i++)
            {
                partslist[i].setName(Utility.ReplacePart(partslist[i].Get_partName()));     //look for replacement part

                if (Utility.StandardAMO(partslist[i].Get_partName()))                                //check PullSequence if standardAMO
                {
                    statuses.Add(info.AMO);
                }
                else if (Utility.KanBanSpike(partslist[i].Get_partName(), partslist[i].Get_Amount()))       //check PullSequence if KanBanSpike
                {
                    statuses.Add(info.KanbanSpike);
                }
                else
                {
                    statuses.Add(info.None);
                }

                string[] outputPullPartStatus = Utility.PullPartStatus(partslist[i].Get_partName());
                EnableOrDisable.Add(outputPullPartStatus[0]);
                Description.Add(outputPullPartStatus[1]);
            }


            List<PartsList> listOfParts = new List<PartsList>();
            for (int i = 0; i < partslist.Count; i++)
            {
                listOfParts.Add(new PartsList() { PartName = partslist[i].Get_partName(), Quantity = partslist[i].Get_Amount(), Status = statuses[i], IsActive = EnableOrDisable[i], Description = Description[i] });
            }
            
         
            AMOdg.ItemsSource = listOfParts;

          
            //Writing AMO report is currently disabled
            //write AMOreport
            /*for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    for (int n = 0; n < partslist.Count; n++)
                    {
                        if (statuses[n] == info.AMO)
                        {
                            AMOreport = AMOreport + partslist[n].Get_partName() + ": " + partslist[n].Get_Amount().ToString() + ": " + statuses[n].ToString() + ": " + EnableOrDisable[i] + ": " + Description[i] + "\n";
                        }
                    }
                }
                else if (i == 1)
                {
                    for (int n = 0; n < partslist.Count; n++)
                    {
                        if (statuses[n] == info.KanbanSpike)
                        {
                            AMOreport = AMOreport + partslist[n].Get_partName() + ": " + partslist[n].Get_Amount().ToString() + ": " + statuses[n].ToString() + ": " + EnableOrDisable[i] + ": " + Description[i] + "\n";
                        }
                    }
                }
                else
                {
                    for (int n = 0; n < partslist.Count; n++)
                    {
                        if (statuses[n] == info.None)
                        {
                            AMOreport = AMOreport + partslist[n].Get_partName() + ": " + partslist[n].Get_Amount().ToString() + ": " + statuses[n].ToString() + ": " + EnableOrDisable[i] + ": " + Description[i] + "\n";
                        }
                    }
                }
            }*/


            //write AMOreport to txt file and PDF file
            string[] AmoReportLines = AMOreport.Split('\n');
            string documentPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(strPathPDF.Substring(0, strPathPDF.Length - 11), @"..\"));  //one folder back from pdfDirectory

            string txtPathAMOReport = documentPath + @"\AMO\AMO_Report.txt";
            Utility.WriteLinesToTXT(AmoReportLines, txtPathAMOReport);

            string pdfPathAMOReport = documentPath + @"\AMO\AMO_Report.pdf";
            Utility.ConvertTXTtoPDF(txtPathAMOReport, pdfPathAMOReport);


            if (AMOreport.Contains("AMO") || AMOreport.Contains("Kanban"))
            {
                AMO.IsChecked = true;
                AMOFound = true;
            }
            else
            {
                AMO.IsChecked = false;
                AMOFound = false;
            }
        }

        private void PageSelectDeselect(int pageOffset)
        {
            try
            {
                if (SelectedPages.Length > page + pageOffset)
                {
                    if (SelectedPages[page + pageOffset] == false)
                    {
                        SelectedPages[page + pageOffset] = true;
                        pg2OverLay.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SelectedPages[page + pageOffset] = false;
                        pg2OverLay.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when trying to select the drawing : {ex.Message}");
            }
        }

        private void Pg1_Click(object sender, RoutedEventArgs e)
        {
            PageSelectDeselect(0);
        }

        private void Pg2_Click(object sender, RoutedEventArgs e)
        {
            PageSelectDeselect(1);

        }

        private void Pg3_Click(object sender, RoutedEventArgs e)
        {
            PageSelectDeselect(2);
        }

        private void Pg4_Click(object sender, RoutedEventArgs e)
        {
            PageSelectDeselect(3);

        }

        private void Pg5_Click(object sender, RoutedEventArgs e)
        {
            PageSelectDeselect(4);
        }
    }
}




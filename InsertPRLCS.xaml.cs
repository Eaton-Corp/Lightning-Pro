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
        Boolean isIncLocLeft = false;
        Boolean isIncLocRight = false;
        Boolean isOpenBottom = false;
        Boolean isExtendedTop = false;
        Boolean isPaintedBox = false;




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


        private void IsIncLocLeft(int index)
        {
            if (ImagesInText[index].Contains("Incoming Location: Left"))
            {
                isIncLocLeft = true;
            }
            else
            {
                isIncLocLeft = false;
            }
        }

        private void IsIncLocRight(int index)
        {
            if (ImagesInText[index].Contains("Incoming Location: Right"))
            {
                isIncLocRight = true;
            }
            else
            {
                isIncLocRight = false;
            }
        }


        private void IsOpenBottom(int index)
        {
            if (ImagesInText[index].Contains("OPEN BOTTOM") || ImagesInText[index].Contains("Open Bottom"))
            {
                isOpenBottom = true;
            }
            else
            {
                isOpenBottom = false;
            }
        }

        private void IsExtendedTop(int index)
        {
            if (ImagesInText[index].Contains("Sprinklerproof") || ImagesInText[index].Contains("SPRINKLERPROOF") || ImagesInText[index].Contains("sprinklerproof"))
            {
                isExtendedTop = true;
            }
            else
            {
                isExtendedTop = false;
            }
        }


        private void IsPaintedBox(int index)
        {
            if (ImagesInText[index].Contains("Paint ") || ImagesInText[index].Contains("PAINT ") || ImagesInText[index].Contains("paint "))
            {
                isPaintedBox = true;
            }
            else
            {
                isPaintedBox = false;
            }
        }




        //call this before inserting boolean values 
        private void LoadAllBooleanValues(int index)
        {
            IsSpecialCustomer();
            IsIncLocLeft(index);
            IsIncLocRight(index);
            IsOpenBottom(index);
            IsExtendedTop(index);
            IsPaintedBox(index);
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
                        if ((parsePDF[i].Contains("Canadian Pow-R-Line CS")) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            if ((parsePDF[i].Contains("1 of 2"))) 
                            {
                                amountOfPages += 2;
                            }
                            else if ((parsePDF[i].Contains("1 of 3")))
                            {
                                amountOfPages += 3;
                            }
                            else if ((parsePDF[i].Contains("1 of 4")))
                            {
                                amountOfPages += 4;
                            }
                            else if ((parsePDF[i].Contains("1 of 5")))
                            {
                                amountOfPages += 5;
                            }
                            else 
                            {
                                amountOfPages++;
                            }
                        }
                    }

                    pages = new int[amountOfPages];

                    int counter = 0;
                    for (int i = 0; i < parsePDF.Length; i++)
                    {
                        if ((parsePDF[i].Contains("Canadian Pow-R-Line CS")) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            if ((parsePDF[i].Contains("1 of 2")))
                            {
                                pages[counter] = i;
                                pages[counter+1] = i+1;
                                counter+=2;
                            }
                            else if ((parsePDF[i].Contains("1 of 3")))
                            {
                                pages[counter] = i;
                                pages[counter + 1] = i + 1;
                                pages[counter + 2] = i + 2;
                                counter += 3;
                            }
                            else if ((parsePDF[i].Contains("1 of 4")))
                            {
                                pages[counter] = i;
                                pages[counter + 1] = i + 1;
                                pages[counter + 2] = i + 2;
                                pages[counter + 3] = i + 3;
                                counter += 4;
                            }
                            else if ((parsePDF[i].Contains("1 of 5")))
                            {
                                pages[counter] = i;
                                pages[counter + 1] = i + 1;
                                pages[counter + 2] = i + 2;
                                pages[counter + 3] = i + 3;
                                pages[counter + 4] = i + 4;
                                counter += 5;
                            }
                            else
                            {
                                pages[counter] = i;
                                counter++;
                            }
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





        //Need to figure out the following for each line item structure

        //Swtichboard Title
        //CSAStandard
        //CSAFooter
        //Section

        Structure[] LineItemStructuresArr;
        int NumOfStructrues;

        public class Structure
        {
            private bool isWireWay;
            private bool hasFSMCs;

            private string SwitchboardTitle;
            private string CSAStandard;
            private string CSAFooter;
            private string Section;


            public Structure()
            {
            }

            public void setWireWay(bool wireway)
            {
                this.isWireWay = wireway;
            }

            public bool getWireWay()
            {
                return this.isWireWay;
            }


            public void setFSMCs(bool FSMCs)
            {
                this.hasFSMCs = FSMCs;
            }

            public bool getFSMCs()
            {
                return this.hasFSMCs;
            }

            public void setCSA244() 
            { 
                SwitchboardTitle = "SWITCHBOARD UNIT/SOUS STATION";
                CSAStandard = "CSA STANDARD C22.2 No.244 / SERIE CSA No.244";
                CSAFooter = "SWITCHBOARD UNIT/TABLEAUX DE CONTROLE/LL47168";
            }

            public void setCSA229()
            {
                SwitchboardTitle = "SWITCHBOARD METER CENTER/TABLEAUX DE COMMUTATION ET DE/MESURAGE";
                CSAStandard = "CSA STANDARD C22.2 No.229 / SERIE CSA No.229";
                CSAFooter = "SWITCHING AND METERING CENTER/TABLEAUX DE COMMUTATION ET DE/MESURAGE/LR47167";
            }

            public void setCSA31()
            {
                SwitchboardTitle = "SWITCHGEAR UNIT/APPAREIL DE COMMUTATION";
                CSAStandard = "CSA STANDARD C22.2 No.31 / SERIE CSA No.31";
                CSAFooter = "SWITCHGEAR UNIT/APPAREIL DE COMMUTATION/LL47168";
            }

            public string getSwitchboardTitle()
            {
                return this.SwitchboardTitle;
            }
            public string getCSAStandard()
            {
                return this.CSAStandard;
            }
            public string getCSAFooter()
            {
                return this.CSAFooter;
            }


            public void setSection(string section)
            {
                this.Section = section;
            }

            public string getSection()
            {
                return this.Section;
            }
        }




        //given the first page PDF of the line item, handle structures algorithm
        private void HandleStructures(int firstPage)
        {
            int NumOfPages = 0;
            if ((ImagesInText[firstPage].Contains("1 of 2")))
            {
                NumOfPages += 2;
            }
            else if ((ImagesInText[firstPage].Contains("1 of 3")))
            {
                NumOfPages += 3;
            }
            else if ((ImagesInText[firstPage].Contains("1 of 4")))
            {
                NumOfPages += 4;
            }
            else if ((ImagesInText[firstPage].Contains("1 of 5")))
            {
                NumOfPages += 5;
            }
            else
            {
                NumOfPages++;
            }


            bool LineItemHas90degWireway;
            if (ImagesInText[firstPage].Contains("90 deg C Wire Way") || ImagesInText[firstPage].Contains("90 deg rated wireway") || ImagesInText[firstPage].Contains("90 deg"))
            {
                LineItemHas90degWireway = true;
            }
            else
            {
                LineItemHas90degWireway = false;
            }

            NumOfStructrues = Int32.Parse(Utility.getBetween(ImagesInText[firstPage + 1], "Total of ", " Structures"));
            LineItemStructuresArr = new Structure[NumOfStructrues];


            int IndexOfStructureInformation = 0;
            for (int i = firstPage; i < (firstPage+NumOfPages); i++) 
            {
                if ((ImagesInText[i].Contains("Switchboard Units Information")))
                {
                    IndexOfStructureInformation = i;
                }
            }


            //get information for each structure in an array
            List<string> PageAsArray = new List<string>();
            int index = IndexOfStructureInformation-1;
            do
            {
                index++;
                string[] intermediatePageAsArray = ImagesInText[index].Split('\n');
                foreach (string line in intermediatePageAsArray) 
                {
                    if (line.Contains("PREPARED BY DATE"))
                    {
                        break;
                    }
                    else 
                    {
                        PageAsArray.Add(line);
                    }                   
                }
            } while (!ImagesInText[index].Contains(NumOfPages.ToString() + " of " + NumOfPages.ToString()));



            string[] StructureInformation = new string[NumOfStructrues];
            int currentStructure = 0;
            do
            { 
                currentStructure++;

                string description = "";

                int IndexOfStructure = 0;
                int IndexOfNextStructure = 0;

                for (int i = 0; i < PageAsArray.Count; i++)
                {
                    if (PageAsArray[i].Equals(currentStructure.ToString())) 
                    {
                        IndexOfStructure = i;
                    }
                    if (currentStructure == NumOfStructrues)
                    {
                        IndexOfNextStructure = PageAsArray.Count - 1;
                    }
                    else 
                    {
                        if (PageAsArray[i].Equals((currentStructure + 1).ToString()))
                        {
                            IndexOfNextStructure = i;
                            break;
                        }
                    }
                }

                if ((IndexOfNextStructure - IndexOfStructure) <= 1) 
                {
                    StructureInformation[currentStructure - 1] = description;
                    break;
                }


                for (int i = IndexOfStructure + 1; i < IndexOfNextStructure; i++)
                {
                    description += PageAsArray[i];
                }

                StructureInformation[currentStructure - 1] = description;

            } while ( currentStructure < NumOfStructrues);



            for (int i = 0; i < NumOfStructrues; i++)
            {
                Structure CurrentStructure = new Structure();

                CurrentStructure.setSection("Section " + (i+1).ToString() + " of " + NumOfStructrues.ToString());

                if (string.IsNullOrEmpty(StructureInformation[i]))
                {
                    CurrentStructure.setWireWay(true);
                }
                else 
                {
                    CurrentStructure.setWireWay(false);
                }

                if (StructureInformation[i].Contains("FSMC"))
                {
                    CurrentStructure.setFSMCs(true);
                }
                else
                {
                    CurrentStructure.setFSMCs(false);
                }

                if (CurrentStructure.getWireWay())
                {
                    if (LineItemHas90degWireway)
                    {
                        CurrentStructure.setCSA31();
                    }
                    else
                    {
                        CurrentStructure.setCSA244();
                    }
                }
                else 
                {
                    if (CurrentStructure.getFSMCs())
                    {
                        CurrentStructure.setCSA229();
                    }
                    else
                    {
                        CurrentStructure.setCSA244();
                    }
                }

                LineItemStructuresArr[i] = CurrentStructure;
            }

        }



        private void InsertXmlData()
        {
            ManBusBarCpacity.Text = MainBusBarCapacityArr[XMLpage];
            Voltage.Text = VoltageArr[XMLpage];
            Hertz.Text = HzArr[XMLpage];
            Pbox.Text = Parr[XMLpage];
            Wbox.Text = Warr[XMLpage];
            shortCircRating.Text = ShortCircuitRatingArr[XMLpage];
            Amps.Text = AmpsArr[XMLpage];
            Enclosure.Text = EnclosureArr[XMLpage];
            GoItemXML.Text = GoItemXmlArr[XMLpage];
        }


        private void Forward_ClickXML(object sender, RoutedEventArgs e)
        {
            if ((XMLpage < MainBusBarCapacityArr.Length - 1) && (XmlUploaded))
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

        string[] MainBusBarCapacityArr;
        string[] VoltageArr;
        string[] HzArr;
        string[] Parr;
        string[] Warr;
        string[] ShortCircuitRatingArr;
        string[] AmpsArr;
        string[] EnclosureArr;

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

                    loadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRLCSnames) + " and [GO]='" + SearchBox.Text + "'");

                    //each node is a line item
                    XmlNodeList BMConfiguredLineItemNodes = xDoc.GetElementsByTagName("BMConfiguredLineItem");

                    int NumberOfLineItems = 0;
                    foreach (XmlNode lineItemNode in BMConfiguredLineItemNodes)
                    {
                        if (lineItemNode.OuterXml.ToString().Contains("Pow-R-Line CS") || lineItemNode.OuterXml.ToString().Contains("PRLCS"))
                        {
                            NumberOfLineItems++;
                        }
                    }
             
                    MainBusBarCapacityArr = new string[NumberOfLineItems];
                    VoltageArr = new string[NumberOfLineItems];
                    HzArr = new string[NumberOfLineItems];
                    Parr = new string[NumberOfLineItems];
                    Warr = new string[NumberOfLineItems];
                    ShortCircuitRatingArr = new string[NumberOfLineItems];
                    AmpsArr = new string[NumberOfLineItems];
                    EnclosureArr = new string[NumberOfLineItems];
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
                int i = 0;
                foreach (XmlNode lineItemNode in BMConfiguredLineItemNodes)
                {
                    if (lineItemNode.OuterXml.ToString().Contains("Pow-R-Line CS") || lineItemNode.OuterXml.ToString().Contains("PRLCS"))
                    {
                        XmlNodeList childLineItemNodes = lineItemNode.ChildNodes;
                        string GO = "";
                        string itemNum = "";
                        foreach (XmlNode childNode in childLineItemNodes)
                        {
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
                        ShortCircuitRatingArr[i] = Utility.getBetween(line, "Rating: ", "kA,");

                        MainBusBarCapacityArr[i] = Utility.getNumberInString(Utility.getBetween(line, "-Wire, ", ","));

                        if (line.Contains("1-Phase"))
                        {
                            Parr[i] = "1";
                        }
                        else if (line.Contains("2-Phase"))
                        {
                            Parr[i] = "2";
                        }
                        else if (line.Contains("3-Phase"))
                        {
                            Parr[i] = "3";
                        }
                        else if (line.Contains("4-Phase"))
                        {
                            Parr[i] = "4";
                        }
                        else 
                        {
                            Parr[i] = "-";
                        }


                        if (line.Contains("1-Wire"))
                        {
                            Warr[i] = "1";
                        }
                        else if (line.Contains("2-Wire"))
                        {
                            Warr[i] = "2";
                        }
                        else if (line.Contains("3-Wire"))
                        {
                            Warr[i] = "3";
                        }
                        else if (line.Contains("4-Wire"))
                        {
                            Warr[i] = "4";
                        }
                        else
                        {
                            Warr[i] = "-";
                        }


                        if (line.Contains("480Y/277V"))
                        {
                            VoltageArr[i] = "480Y/277V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("208Y/120V"))
                        {
                            VoltageArr[i] = "208Y/120V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("120/240V"))
                        {
                            VoltageArr[i] = "120/240V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("600Y/347V"))
                        {
                            VoltageArr[i] = "600Y/347V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("415/240V"))
                        {
                            VoltageArr[i] = "415/240V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("380/220V"))
                        {
                            VoltageArr[i] = "380/220V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("220/127V"))
                        {
                            VoltageArr[i] = "220/127V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("125V DC"))
                        {
                            VoltageArr[i] = "125V DC";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("250V DC"))
                        {
                            VoltageArr[i] = "250V DC";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("600V DC"))
                        {
                            VoltageArr[i] = "600V DC";
                            HzArr[i] = "-";
                        }
                        else if (line.Contains("220V"))
                        {
                            VoltageArr[i] = "220V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("380V"))
                        {
                            VoltageArr[i] = "380V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("415V"))
                        {
                            VoltageArr[i] = "415V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("600V"))
                        {
                            VoltageArr[i] = "600V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("480V"))
                        {
                            VoltageArr[i] = "480V";
                            HzArr[i] = "60";
                        }
                        else if (line.Contains("240V"))
                        {
                            VoltageArr[i] = "240V";
                            HzArr[i] = "60";
                        }
                        else
                        {
                            VoltageArr[i] = "";
                            HzArr[i] = "";
                        }


                        if (line.Contains("Type 1") || line.Contains("Type1") || line.Contains("TYPE 1") || line.Contains("TYPE1"))
                        {
                            EnclosureArr[i] = "TYPE 1";
                        }
                        else if (line.Contains("Sprinklerproof") || line.Contains("SPRINKLERPROOF"))
                        {
                            EnclosureArr[i] = "TYPE 1 SPRINKLERED";
                        }
                        else if (line.Contains("Type 2") || line.Contains("Type2") || line.Contains("TYPE 2") || line.Contains("TYPE2"))
                        {
                            EnclosureArr[i] = "TYPE 2";
                        }
                        else if (line.Contains("Type 4X") || line.Contains("Type4X") || line.Contains("TYPE 4X") || line.Contains("TYPE4X"))
                        {
                            EnclosureArr[i] = "TYPE 4X";
                        }
                        else if (line.Contains("Type 3R") || line.Contains("Type3R") || line.Contains("TYPE 3R") || line.Contains("TYPE3R"))
                        {
                            EnclosureArr[i] = "TYPE 3R";
                        }
                        else if (line.Contains("Type 4") || line.Contains("Type4") || line.Contains("TYPE 4") || line.Contains("TYPE4"))
                        {
                            EnclosureArr[i] = "TYPE 4";
                        }
                        else if (line.Contains("Type 12") || line.Contains("Type12") || line.Contains("TYPE 12") || line.Contains("TYPE12"))
                        {
                            EnclosureArr[i] = "TYPE 12";
                        }
                        else
                        {
                            EnclosureArr[i] = "";
                        }

                        AmpsArr[i] = Utility.getMaxVoltage(VoltageArr[i]);

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
            ProductID.Text = "Pow-R-LineCS";   //always PRL-CS
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
                ProductID.Text = "Pow-R-LineCS";   //always PRL-CS
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
            if (Utility.isDuplicate(GO_Item.Text, Utility.ProductGroup.PRLCS) == false)
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


                string StrInsertCommand = "Insert into PRLCS ([GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [FilePath], [ProductSpecialist], [PageNumber], [ImageFilePath]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);

                    if (string.IsNullOrEmpty(EnteredDate.Text)) InsertCommand.Parameters.AddWithValue("EnteredDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("EnteredDate", EnteredDate.Text);
                    if (string.IsNullOrEmpty(ReleaseDate.Text)) InsertCommand.Parameters.AddWithValue("ReleaseDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    if (string.IsNullOrEmpty(CommitDate.Text)) InsertCommand.Parameters.AddWithValue("CommitDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);


                    //InsertCommand.Parameters.AddWithValue("EnteredDate", EnteredDate.Text);
                    //InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    //InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);

                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    //can use checkbox.isChecked option if you cannot do it automatically
                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);
                    InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", (Boolean)isSpecialCustomer);
                    InsertCommand.Parameters.AddWithValue("[IncLocLeft]", (Boolean)isIncLocLeft);
                    InsertCommand.Parameters.AddWithValue("[IncLocRight]", (Boolean)isIncLocRight);
                    InsertCommand.Parameters.AddWithValue("[CrossBus]", CrossBus.IsChecked);
                    InsertCommand.Parameters.AddWithValue("[OpenBottom]", (Boolean)isOpenBottom);
                    InsertCommand.Parameters.AddWithValue("[ExtendedTop]", (Boolean)isExtendedTop);
                    InsertCommand.Parameters.AddWithValue("[PaintedBox]", (Boolean)isPaintedBox);
                    InsertCommand.Parameters.AddWithValue("[ThirtyDeepEnclosure]", ThirtyDeepEnclosure.IsChecked);

                    InsertCommand.Parameters.AddWithValue("FilePath", PathPDF.Text);
                    InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
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
                    HandleStructures(firstPgIndex);
                    Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathPDF.Text), GO_Item.Text + "_" + pgNumStr + "_CONSTR.pdf"), (BitmapImage)image[pageNumber]);
                    Utility.SaveBitmapAsPNGinImages(PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png", (BitmapImage)image[pageNumber]);
                }
                else
                {
                    Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(PathPDF.Text), GO_Item.Text + "_" + pgNumStr + "_CONSTR.pdf"), (BitmapImage)image[pageNumber]);
                    Utility.SaveBitmapAsPNGinImages(PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png", (BitmapImage)image[pageNumber]);
                }

                string StrInsertCommand = "Insert into PRLCS ([GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [FilePath], [ProductSpecialist], [PageNumber], [ImageFilePath]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                using (OleDbCommand InsertCommand = new OleDbCommand(StrInsertCommand, MainWindow.LPcon))
                {
                    InsertCommand.Parameters.AddWithValue("GO_Item", GO_Item.Text);
                    InsertCommand.Parameters.AddWithValue("GO", GO1.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderInterior", ShopOrder.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderBox", ShopOrderBox.Text);
                    InsertCommand.Parameters.AddWithValue("ShopOrderTrim", ShopOrderTrim.Text);
                    InsertCommand.Parameters.AddWithValue("Customer", Customer.Text);
                    InsertCommand.Parameters.AddWithValue("Quantity", Qty.Text);

                    if (string.IsNullOrEmpty(EnteredDate.Text)) InsertCommand.Parameters.AddWithValue("EnteredDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("EnteredDate", EnteredDate.Text);
                    if (string.IsNullOrEmpty(ReleaseDate.Text)) InsertCommand.Parameters.AddWithValue("ReleaseDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    if (string.IsNullOrEmpty(CommitDate.Text)) InsertCommand.Parameters.AddWithValue("CommitDate", DBNull.Value); else InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);


                    //InsertCommand.Parameters.AddWithValue("EnteredDate", EnteredDate.Text);
                    //InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                    //InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);

                    InsertCommand.Parameters.AddWithValue("Tracking", Tracking);
                    InsertCommand.Parameters.AddWithValue("Urgency", Urgency.Text);

                    //can use checkbox.isChecked option if you cannot do it automatically
                    InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);
                    InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", (Boolean)isSpecialCustomer);
                    InsertCommand.Parameters.AddWithValue("[IncLocLeft]", (Boolean)isIncLocLeft);
                    InsertCommand.Parameters.AddWithValue("[IncLocRight]", (Boolean)isIncLocRight);
                    InsertCommand.Parameters.AddWithValue("[CrossBus]", CrossBus.IsChecked);
                    InsertCommand.Parameters.AddWithValue("[OpenBottom]", (Boolean)isOpenBottom);
                    InsertCommand.Parameters.AddWithValue("[ExtendedTop]", (Boolean)isExtendedTop);
                    InsertCommand.Parameters.AddWithValue("[PaintedBox]", (Boolean)isPaintedBox);
                    InsertCommand.Parameters.AddWithValue("[ThirtyDeepEnclosure]", ThirtyDeepEnclosure.IsChecked);

                    InsertCommand.Parameters.AddWithValue("FilePath", PathPDF.Text);
                    InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                    InsertCommand.Parameters.AddWithValue("PageNumber", pageNumber - firstPgIndex);
                    InsertCommand.Parameters.AddWithValue("ImageFilePath", PathIMAGE.Text + "_" + Item.Text + "_" + pgNumStr + ".png");

                    InsertCommand.ExecuteNonQuery();
                }//end using command



                if ((pageNumber - firstPgIndex) == 0)
                {
                    string command = "insert into CSALabelPRLCS ([GONumber], [ItemNumber], [SwitchBoardTitle], [CSAStandard], [SMCenter], [Section], [MainBusBarCapacity], [Voltage], [Hz], [P], [W], [ShortCircuitRating], [Amps], [Enclosure], [GO_Item], [ProductID]) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    for (int i = 0; i < NumOfStructrues; i++) 
                    {
                        using (OleDbCommand InsertCSA = new OleDbCommand(command, MainWindow.LPcon))
                        {
                            InsertCSA.Parameters.AddWithValue("[GONumber]", GO1.Text);
                            InsertCSA.Parameters.AddWithValue("[ItemNumber]", Item.Text);
                            InsertCSA.Parameters.AddWithValue("[SwitchBoardTitle]", LineItemStructuresArr[i].getSwitchboardTitle());
                            InsertCSA.Parameters.AddWithValue("[CSAStandard]", LineItemStructuresArr[i].getCSAStandard());
                            InsertCSA.Parameters.AddWithValue("[SMCenter]", LineItemStructuresArr[i].getCSAFooter());
                            InsertCSA.Parameters.AddWithValue("[Section]", LineItemStructuresArr[i].getSection());
                            InsertCSA.Parameters.AddWithValue("[MainBusBarCapacity]", ManBusBarCpacity.Text);
                            InsertCSA.Parameters.AddWithValue("[Voltage]", Voltage.Text);
                            InsertCSA.Parameters.AddWithValue("[Hz]", Hertz.Text);
                            InsertCSA.Parameters.AddWithValue("[P]", Pbox.Text);
                            InsertCSA.Parameters.AddWithValue("[W]", Wbox.Text);
                            InsertCSA.Parameters.AddWithValue("[ShortCircuitRating]", shortCircRating.Text);
                            InsertCSA.Parameters.AddWithValue("[Amps]", Amps.Text);
                            InsertCSA.Parameters.AddWithValue("[Enclosure]", Enclosure.Text);
                            InsertCSA.Parameters.AddWithValue("[GO_Item]", GO_Item.Text);
                            InsertCSA.Parameters.AddWithValue("[ProductID]", ProductID.Text);

                            InsertCSA.ExecuteNonQuery();
                        }//end using command 
                    }
                }

                Status.Content = GO_Item.Text + " SUCCESSFULLY INSERTED";

                DataGridRow dataGridRow = dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) as DataGridRow;
                if (dataGridRow != null)
                    dataGridRow.Background = System.Windows.Media.Brushes.LightGreen;
            }
        }




        private void Search_GOs(object sender, RoutedEventArgs e)
        {
            loadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRLCSnames) + " and [GO]='" + SearchBox.Text + "'");
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

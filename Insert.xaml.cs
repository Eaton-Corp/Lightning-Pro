using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for Insert.xaml
    /// </summary>
    public partial class Insert : Window
    {
        
        //image is an array full of bitmap images, each index contains a bitmap image of a page we care about
        BitmapSource[] image;

        //text version of image array
        string[] ImagesInText;


        XmlNodeList lines;      //BMConfiguredLineItem nodes -> XmlNodeList of each GoItem
        XmlNodeList BMLines;    //BMLineItems nodes -> Bill of Materials Nodes (Materials Listed Here)

        int page;      //current page -> start at page = 0
        int[] pages;     //mapped indices of page numbers we care about (without detail bill information) 

        Boolean AMOFound;   //boolean indicator for isAMO()

        //indicator variables for when xml and pdf have been uploaded
        Boolean XMLLoaded = false;
        Boolean PDFLoaded = false;

        List<int> duplicates;   //each index of the duplicates list corresponds to a line item, the integer at the index corresponds to number of sections 


        string GOnum;           //job GO number -> value comes from xml upload
        string strPathPDF;      //path for pdf output -> value comes from FilePathFinder function
        string strPathImage;    //path for image(PNG) output -> value comes from FilePathFinder function


        string ProductSpecialist = "";


        Boolean isSpecialCustomer = false;
        Boolean isMuliSection = false;
        Boolean isServiceEntrance = false;
        Boolean PaintedBox = false;
        Boolean RatedNeutral200 = false;



        /*
         * Insert procedure:
         * Upload XML file and find GO number
         * Upload PDF which is converted to PNG
         * Query MSS database for all entries with that GO number
         * Take entries/PNG and insert for fields within the LightningPro Database
        */

        public Insert()
        {
            InitializeComponent();
            page = 0;
        }



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


        private void IsMultiSectional()
        {
            if (ImagesInText[page].Contains(" of 2)") || ImagesInText[page].Contains(" of 3)") || ImagesInText[page].Contains(" of 4)") )
            {
                isMuliSection = true;
            }
            else
            {
                isMuliSection = false;
            }
        }

        private void IsServiceEntrance()
        {
            if (ImagesInText[page].Contains("Service Entrance"))
            {
                isServiceEntrance = true;
            }
            else
            {
                isServiceEntrance = false;
            }
        }

        private void IsPaintedBox()
        {
            if (ImagesInText[page].Contains("Box Finish"))
            {
                PaintedBox = true;
            }
            else
            {
                PaintedBox = false;
            }
        }

        private void IsRatedNeutral()
        {
            if (ImagesInText[page].Contains("200% Rated Neutral"))
            {
                RatedNeutral200 = true;
            }
            else
            {
                RatedNeutral200 = false;
            }
        }


        //call this before inserting boolean values 
        private void LoadAllBooleanValues() 
        {
            IsSpecialCustomer();
            IsMultiSectional();
            IsServiceEntrance();
            IsPaintedBox();
            IsRatedNeutral();
        }











        private async Task<int> TurnOnStatusBar()
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


                if (response == true)           //if the user selects a file and clicks OK
                {
                    Task<int> ShowProgressBar = TurnOnStatusBar();
                    int result = await ShowProgressBar;


                    string filepath = ofg.FileName;

                    string[] parsePDF = Utility.ExtractTextFromPdf(filepath);     //parsePDF is a string array where each index is a text block of a page

                    int amountOfPages = 0;
                    for (int i = 0; i < parsePDF.Length; i++)       //finds the amount of pages we care about
                    {
                        if (((parsePDF[i].Contains("PRL1")) || (parsePDF[i].Contains("PRL2")) || (parsePDF[i].Contains("PRL3"))) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            amountOfPages++;
                        }
                    }

                    pages = new int[amountOfPages];         //new integer array of size equal to the amount of pages we care about

                    int counter = 0;
                    for (int i = 0; i < parsePDF.Length; i++)       //maps the index of page numbers we care about into pages array
                    {
                        if (((parsePDF[i].Contains("PRL1")) || (parsePDF[i].Contains("PRL2")) || (parsePDF[i].Contains("PRL3"))) && (!parsePDF[i].Contains("Detail Bill")))
                        {
                            pages[counter] = i;     
                            counter++;
                        }
                    }

                    BitmapSource[] allImages = Utility.ConvertPDFToImage(filepath);     //allImages, is an array where each index is a bitmap image of a page


                    image = new BitmapSource[pages.Length];
                    for (int i = 0; i < pages.Length; i++)          //transfers over to image array only the bitmap images we care about 
                    {
                        image[i] = allImages[pages[i]];

                    }   //end result --> image is an array full of bitmap images, each index contains a bitmap image of a page we care about 


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
                    }
                    catch
                    {
                        MessageBox.Show("Error In PDF");
                        pbStatus.Visibility = Visibility.Hidden;
                        return;
                    }

                    pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
                    if (XMLLoaded == true)
                    {
                        loadXML(page);
                        string GOIkey = GOnum + " " + currentItemNum;
                        FindImageIndex(GOIkey);
                        ImagePreviewer.Source = image[ImagePage];
                    }
                    PDFLoaded = true;
                    pbStatus.Visibility = Visibility.Hidden;
                    Status.Content = "PDF SUCCESSFULLY UPLOADED";
                }
            }
            catch
            {
                pbStatus.Visibility = Visibility.Hidden;
                MessageBox.Show("Error Occurred Uploading PDF");
            }
        }



   
       
        private void FilePathFinder(string GO)
        {
            string OrderFilesPath = ConfigurationManager.ConnectionStrings["orderFiles"].ToString();
            var dir = new DirectoryInfo(OrderFilesPath);
                        
            Boolean Found = false;
            foreach (var file in dir.EnumerateDirectories(GO + "*"))  //all files in target directory that begin with GO
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
            strPathImage = dir.FullName + @"\" + GO + @"\Images" + @"\" + GO;
            strPathPDF = dir.FullName + @"\" + GO + @"\PDF Storage" + @"\" + GO;
        }



        //new function POWELL ERROR
        private void FindImageIndex(string GoItemKey) 
        {
            int i;
            for (i = 0; i < ImagesInText.Length; i++)
            {
                if (ImagesInText[i].Contains(GoItemKey))
                {
                    ImagePage = i;
                    break;
                }
            }
        }


        string currentItemNum;
        int ImagePage;

        //new function POWELL ERROR
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (XMLLoaded == true && PDFLoaded == true)
            {
                if (page < image.Length - 1)
                {
                    page += 1;
                    loadXML(page);
                    string GOIkey = GOnum + " " + currentItemNum;
                    FindImageIndex(GOIkey);
                    ImagePreviewer.Source = image[ImagePage];
                }
                pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
            }
        }

        //new function POWELL ERROR
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (XMLLoaded == true && PDFLoaded == true)
            {
                if (page != 0)
                {
                    page -= 1;
                    loadXML(page);
                    string GOIkey = GOnum + " " + currentItemNum;
                    FindImageIndex(GOIkey);
                    ImagePreviewer.Source = image[ImagePage];
                }
                pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
            }
        }

        //private void Forward_Click(object sender, RoutedEventArgs e)
        //{
        //    if (XMLLoaded == true && PDFLoaded == true)
        //    {
        //        if (page < image.Length - 1)
        //        {
        //            page += 1;
        //            ImagePreviewer.Source = image[page];
        //        }
        //        pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
        //        loadXML(page);
        //    }
        //}



        //private void Back_Click(object sender, RoutedEventArgs e)
        //{
        //    if (XMLLoaded == true && PDFLoaded == true)
        //    {
        //        if (page != 0)
        //        {
        //            page -= 1;
        //            ImagePreviewer.Source = image[page];
        //        }
        //        pg.Content = "Page:" + (page + 1).ToString() + "/" + image.Length.ToString();
        //        loadXML(page);
        //    }
        //}










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


                    //find duplicates and mulit-section panels
                    XmlNodeList BMLine = xDoc.GetElementsByTagName("BMConfiguredLineItem");  //XMLNodeList where each node is a line item for the Job
                    int n = 0;
                    duplicates = new List<int>();
                    while (n < BMLine.Count)                    //checks XML for double, triple, quadruple duplicates
                    {
                        Boolean BMLineItemFound = false;
                        foreach (XmlNode node in BMLine[n])     //for each node of a line item
                        {
                            if (node.OuterXml.ToString().Contains("BMLineItem") && BMLineItemFound == false)
                            {
                                if (node.OuterXml.ToString().Contains("2 Sections") && node.OuterXml.ToString().Contains("Multi-Section Panel"))
                                {
                                    duplicates.Add(2);
                                }
                                else if (node.OuterXml.ToString().Contains("3 Sections") && node.OuterXml.ToString().Contains("Multi-Section Panel"))
                                {
                                    duplicates.Add(3);
                                }
                                else if (node.OuterXml.ToString().Contains("4 Sections") && node.OuterXml.ToString().Contains("Multi-Section Panel"))
                                {
                                    duplicates.Add(4);
                                }
                                else
                                {
                                    duplicates.Add(0);
                                }
                                BMLineItemFound = true;     //only need the first BMLineItem to check for multi-section
                            }
                        }
                        n++;
                    }


                    //get the GO number of job
                    XmlNodeList name = xDoc.GetElementsByTagName("OrderInfo");
                    string outputQuery = name[0].FirstChild.OuterXml.Substring(22, 10);
                    GOnum = outputQuery;
                    
                    //load grid with all relavent jobs; deal with directories and filepaths
                    loadGrid("select * from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL123names) + " and [GO]='" + GOnum + "'");
                    FilePathFinder(GOnum);

                    //sets global variable lines to XMLNodesList where each node is a line item
                    lines = xDoc.GetElementsByTagName("BMConfiguredLineItem");

                    //sets global variable BMLines to XMLNodesList where each node has the list of materials for a line item
                    BMLines = xDoc.GetElementsByTagName("BMLineItems");

                    //start isAMO function to do work on the materials 
                    isAMO();

                    if (PDFLoaded == true)
                    {
                        loadXML(page);
                        string GOIkey = GOnum + " " + currentItemNum;
                        FindImageIndex(GOIkey);
                        ImagePreviewer.Source = image[ImagePage];
                    }

                    XMLLoaded = true;
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




        private void loadGrid(string query)
        {
            DataTable dt = Utility.SearchMasterDB(query);
            dg.ItemsSource = dt.DefaultView;
            Urgency.Text = "N";     //default to normal urgency
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
            }
        }















        private void loadXML(int page)
        {
            try
            {
                string Enc = "";
                Boolean CatalogFound = false;
                List<int> activeLines = new List<int>();
                
                for (int i = 0; i < lines.Count; i++)
                {
                    foreach (XmlNode node in lines[i])
                    {
                        if ((node.OuterXml.ToString().Contains("Pow-R-Line1")) || (node.OuterXml.ToString().Contains("Pow-R-Line2")) || (node.OuterXml.ToString().Contains("Pow-R-Line3")))
                        {
                            activeLines.Add(i);
                            if (duplicates[i] == 2)
                            {
                                activeLines.Add(i);
                            }
                            else if (duplicates[i] == 3)
                            {
                                activeLines.Add(i);
                                activeLines.Add(i);
                            }
                            else if (duplicates[i] == 4)
                            {
                                activeLines.Add(i);
                                activeLines.Add(i);
                                activeLines.Add(i);
                            }
                        }
                    }
                }

                foreach (XmlNode node in lines[activeLines[page]])
                {
                    if (node.OuterXml.ToString().Contains("ProductID"))
                    {
                        ProductID.Text = node.OuterXml.Substring(23, 12);
                    }
                    if (node.OuterXml.ToString().Contains("Designations"))
                    {

                        Designation.Text = Utility.getBetween(node.OuterXml.ToString(), "V=\"", "\"");
                       
                    }
                    if (node.OuterXml.ToString().Contains("ItemNumber"))
                    {
                        string itemNumber = Utility.getBetween(node.OuterXml.ToString(), "V=\"", "\"");
                        Item.Text = itemNumber;
                        if (duplicates[activeLines[page]] == 2)
                        {
                            if (page != 0 && activeLines[page - 1] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 1);
                            }

                        }
                        else if (duplicates[activeLines[page]] == 3)
                        {
                            if (page != 0 && activeLines[page - 1] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 1);
                            }
                            else if (page != 1 && activeLines[page - 2] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 2);
                            }
                        }
                        else if (duplicates[activeLines[page]] == 4)
                        {
                            if (page != 0 && activeLines[page - 1] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 1);
                            }
                            else if (page != 0 && activeLines[page - 2] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 2);
                            }
                            else if (page != 0 && activeLines[page - 3] == activeLines[page])
                            {
                                Item.Text = Utility.IterateItem(itemNumber, 3);
                            }
                        }
                        else
                        {
                            Item.Text = itemNumber;
                        }
                        currentItemNum = Item.Text;
                    }

                    int flag200 = 1;
                    if (node.OuterXml.ToString().Contains("200% Rated Neutral "))
                    {
                        flag200 = 1;
                    }
                    else
                    {
                        flag200 = 0;
                    }

                    if (node.OuterXml.ToString().Contains("BMLineItem"))
                    {
                        string V = "nothing";
                       
                        string line = node.FirstChild.OuterXml.ToString();
                        string line2 = node.FirstChild.NextSibling.OuterXml.ToString();

                      
                        if (line.Contains("208Y/120V"))
                        {
                            V = "208Y/120V 3Ph 4W";
                            Voltage.Text = "208Y/120V AC/CA";
                            P.Text = "3";
                            W.Text = "4";
                            Ground.Text = "False";
                            Hz.Text = "60";


                        }
                        else if (line.Contains("120/240V"))
                        {
                            V = "120/240V 1Ph 3W";
                            Voltage.Text = "120 / 240V AC/CA";
                            P.Text = "1";
                            W.Text = "3";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("600Y/347V"))
                        {
                            V = "600Y/347V 3Ph 4W";
                            Voltage.Text = "600Y/347V AC/CA";
                            P.Text = "3";
                            W.Text = "4";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("600V"))
                        {
                            V = "600V 3Ph 3W";
                            Voltage.Text = "600V AC/CA";
                            P.Text = "3";
                            W.Text = "3";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("480Y/277V"))
                        {
                            V = "480Y/277V 3Ph 4W";
                            Voltage.Text = "480Y/277V AC/CA";
                            P.Text = "3";
                            W.Text = "4";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("480V"))
                        {
                            V = "480V 3Ph 3W";
                            Voltage.Text = "480V AC/CA";
                            P.Text = "3";
                            W.Text = "3";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("240V"))
                        {
                            V = "240V 3Ph 3W";
                            Voltage.Text = "240V AC/CA";
                            P.Text = "3";
                            W.Text = "3";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("125/250V DC"))
                        {
                            V = "125/250V DC 3W";
                            Voltage.Text = "125/250V DC";
                            P.Text = "DC";
                            W.Text = "3";
                            Ground.Text = "False";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("125V DC") && line.Contains("2W Grounded"))
                        {
                            V = "125V DC 2W Grounded";
                            Voltage.Text = "125V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "True";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("125V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "125V DC 2W Ungrounded";
                            Voltage.Text = "125V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "False";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("250V DC") && line.Contains("2W Grounded"))
                        {
                            V = "250V DC 2W Grounded";
                            Voltage.Text = "250V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "True";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("250V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "250V DC 2W Ungrounded";
                            Voltage.Text = "250V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "False";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("600V DC") && line.Contains("2W Grounded"))
                        {
                            V = "600V DC 2W Grounded";
                            Voltage.Text = "600V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "True";
                            Hz.Text = "-";
                        }
                        else if (line.Contains("416/240V  AC/CA"))
                        {
                            V = "416/240V 3Ph 4W";
                            Voltage.Text = "416/240V";
                            P.Text = "3";
                            W.Text = "4";
                            Ground.Text = "False";
                            Hz.Text = "60";
                        }
                        else if (line.Contains("600V DC") && line.Contains("2W Ungrounded"))
                        {
                            V = "240/120V 3Ph 4W A HiLeg";
                            Voltage.Text = "600V DC";
                            P.Text = "DC";
                            W.Text = "2";
                            Ground.Text = "False";
                            Hz.Text = "-";
                        }

                        if (line.Contains("Enclosure"))
                        {
                            if (line.Contains("TYPE1"))
                            {
                                Enclosure.Text = "TYPE1";
                            }
                            else if (line.Contains("SPRINKLERPROOF"))
                            {
                                Enclosure.Text = "SPRINKLERPROOF";
                            }
                            else if (line.Contains("TYPE12") && (Enc.Contains("TYPE4X") == false))
                            {
                                Enclosure.Text = "TYPE12";
                            }
                            else if (line.Contains("TYPE3R"))
                            {
                                Enclosure.Text = "TYPE3R";
                            }
                            else if (line.Contains("TYPE4") && (Enc.Contains("TYPE4X") == false))
                            {
                                Enclosure.Text = "TYPE4";
                            }
                            else if (line.Contains("TYPE4X"))
                            {
                                Enclosure.Text = "TYPE4X";
                            }
                            else
                            {
                                Enclosure.Text = "No Box";
                            }
                        }

                        if (line.Contains("100A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {

                                    Neut.Text = "100A";                                
                                
                            }
                            
                            MA.Text = "100A";
                        }
                        else if (line.Contains("225A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Neut.Text = "225A";
                            }
                            
                            MA.Text = "225A";
                        }
                        else if (line.Contains("250A"))
                        {
                            if (line.Contains("4W") && line.Contains("DC") == false)
                            {
                                Neut.Text = "250A";
                            }
                            
                            MA.Text = "250A";
                        }
                        else if (line.Contains("400A"))
                        {
                            if ((line.Contains("4W") || line.Contains("3W")) && line.Contains("DC") == false)
                            {
                                Neut.Text = "400A";
                            }
                            
                            MA.Text = "400A";
                        }
                        else if (line.Contains("600A"))
                        {
                            if (line.Contains("4W") && line.Contains("DC") == false)
                            {
                                Neut.Text = "600A";
                            }
                            
                            MA.Text = "600A";
                        }


                        if (line.Contains("Ungrounded"))
                        {
                            Neut.Text = "-";
                            P.Text = "-";
                            Hz.Text = "-";
                        }

                        if(line.Contains("Max X-Space for Branch Devices:"))
                        {
                            
                            if (duplicates[activeLines[page]] == 2)
                            {
                                string str = Utility.getBetween(line, "Max X-Space for Branch Devices:", "\"");
                                if(str.Contains("X"))
                                {
                                    str.Remove(str.Length - 1);
                                    Xspace.Text = (Int32.Parse(str) * 2).ToString() + "X";
                                }
                                else
                                {
                                    Xspace.Text = (Int32.Parse(str) * 2).ToString();
                                }
                                CatalogFound = true;
                            }
                            else
                            {
                                Xspace.Text = Utility.getBetween(line, "Max X-Space for Branch Devices: ", "\"");
                                CatalogFound = true;                                
                            }

                                
                        }
                        else
                        {
                            if (line2.Contains("CatalogNumber") && CatalogFound == false)
                            {
                                if (duplicates[activeLines[page]] == 2)
                                {
                                    Xspace.Text = (Int32.Parse(Regex.Replace(line2.Substring(35, 4), "[^0-9]", "")) * 2).ToString();
                                    CatalogFound = true;
                                }
                                else
                                {
                                    Xspace.Text = Regex.Replace(line2.Substring(35, 4), "[^0-9]", "");
                                    CatalogFound = true;
                                }

                            }
                        }


                        if (V.Contains("Grounded"))
                        {
                            Ground.Text = "True";
                        }
                        else
                        {
                            Ground.Text = "False";
                        }



                        if(flag200 == 1)
                        {
                            Neut.Text = (Int32.Parse(Regex.Replace(Neut.Text.Substring(0, 3), "[^0-9]", "")) * 2).ToString() + "A";
                        }

                    }
                }

                foreach (XmlNode val in BMLines[activeLines[page]])
                {
                    if (val.FirstChild.OuterXml.ToString().Contains("Enclosure"))
                    {
                        Enc = val.FirstChild.OuterXml.ToString();
                    }
                }
                


                if (Enc.Contains("Type 12") && (Enc.Contains("TYPE 4X") == false))
                {
                    Enclosure.Text = "TYPE12";
                }
                else if (Enc.Contains("SPRINKLERPROOF"))
                {
                    Enclosure.Text = "SPRINKLERPROOF";
                }
                else if (Enc.Contains("Type 1"))
                {
                    Enclosure.Text = "TYPE1";
                }
                else if (Enc.Contains("Type 2"))
                {
                    Enclosure.Text = "TYPE2";
                }
                else if (Enc.Contains("Type 3R"))
                {
                    Enclosure.Text = "TYPE3R";
                }
                else if (Enc.Contains("Type 4X"))
                {
                    Enclosure.Text = "TYPE4X";
                }
                else if (Enc.Contains("Type 4") && (Enc.Contains("TYPE 4X") == false))
                {
                    Enclosure.Text = "TYPE4";
                }
                
                else
                {
                    Enclosure.Text = "No Box";
                }

                if (Enc.Contains("EZB2030RCSP")) BoxCatalogue.Text = "EZB2030RCSP";
                else if (Enc.Contains("EZB2036RCSP")) BoxCatalogue.Text = "EZB2036RCSP";
                else if (Enc.Contains("EZB2042RCSP")) BoxCatalogue.Text = "EZB2042RCSP";
                else if (Enc.Contains("EZB2048RCSP")) BoxCatalogue.Text = "EZB2048RCSP";
                else if (Enc.Contains("EZB2054RCSP")) BoxCatalogue.Text = "EZB2054RCSP";
                else if (Enc.Contains("EZB2060RCSP")) BoxCatalogue.Text = "EZB2060RCSP";
                else if (Enc.Contains("EZB2072RCSP")) BoxCatalogue.Text = "EZB2072RCSP";
                else if (Enc.Contains("EZB2090RCSP")) BoxCatalogue.Text = "EZB2090RCSP";
                else if (Enc.Contains("EZB2030RC")) BoxCatalogue.Text = "EZB2030RC";
                else if (Enc.Contains("EZB2036RC")) BoxCatalogue.Text = "EZB2036RC";
                else if (Enc.Contains("EZB2042RC")) BoxCatalogue.Text = "EZB2042RC";
                else if (Enc.Contains("EZB2048RC")) BoxCatalogue.Text = "EZB2048RC";
                else if (Enc.Contains("EZB2054RC")) BoxCatalogue.Text = "EZB2054RC";
                else if (Enc.Contains("EZB2060RC")) BoxCatalogue.Text = "EZB2060RC";
                else if (Enc.Contains("EZB2072RC")) BoxCatalogue.Text = "EZB2072RC";
                else if (Enc.Contains("EZB2090RC")) BoxCatalogue.Text = "EZB2090RC";
                else BoxCatalogue.Text = "null";

            }
            catch
            {
                MessageBox.Show("Unable To Load XML");
            }   
        }









        






      

        

     
       

        private void Insert_Entry(object sender, RoutedEventArgs e)
        {
            if (XMLLoaded == true && PDFLoaded == true)
            {
                try
                {                 
                    Boolean Duplicate = false;
                    if (Utility.isDuplicate(GO_Item.Text,Utility.ProductGroup.PRL123))
                    {
                        MessageBox.Show("You Have A Duplicate. GO Items Must Be Unique.", "Duplicate Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                        Duplicate = true;
                    }

                    if (Duplicate == false)
                    {
                        if (string.IsNullOrEmpty(ShopOrder.Text)) 
                        {
                            if (MessageBox.Show("You Are About To Insert\nWithout A ShopOrderInterior Number\nWould You Like To Continue?", "No ShopOrderInterior Number",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                Status.Content = GO_Item.Text + " INSERT CANCELLED";
                                return;
                            }
                        }

                        bool hasDates = true;
                        if (string.IsNullOrEmpty(ReleaseDate.Text) || string.IsNullOrEmpty(CommitDate.Text) || string.IsNullOrEmpty(EnteredDate.Text))
                        {
                            if (MessageBox.Show("You Are About To Insert With\nMissing Dates (Entered, Commit, Release)\nWould You Like To Continue?", "Missing Dates",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                Status.Content = GO_Item.Text + " INSERT CANCELLED";
                                return;
                            }
                            else 
                            { 
                                hasDates = false;
                            }
                        }

                        LoadAllBooleanValues();

                        string Tracking;
                        if (Approve.IsChecked == true)
                        {
                            Tracking = "MIComplete";
                        }
                        else
                        {
                            Tracking = "InDevelopment";
                        }

                        Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(strPathPDF), GO_Item.Text + "_CONSTR.pdf"), (BitmapImage)image[ImagePage]);
                        Utility.SaveBitmapAsPNGinImages(strPathImage + "_" + Item.Text + ".png", (BitmapImage)image[ImagePage]);

                        string commandStr = "";
                        if (hasDates)
                        {
                            commandStr = "insert into PRL123 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, BoxEarly, [AMO], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, Catalogue, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        }
                        else 
                        {
                            commandStr = "insert into PRL123 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, BoxEarly, [AMO], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], FilePath, ProductSpecialist, Catalogue, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        }

                        using (OleDbCommand InsertCommand = new OleDbCommand(commandStr,MainWindow.LPcon))
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
                            //InsertCommand.Parameters.AddWithValue("Bidman", ToByteArray(DrawInfo(image[page])));
                            InsertCommand.Parameters.AddWithValue("BoxEarly", BoxFirst.IsChecked);
                            InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);

                            InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", (Boolean)isSpecialCustomer);

                            InsertCommand.Parameters.AddWithValue("[ServiceEntrance]", (Boolean)isServiceEntrance);
                            InsertCommand.Parameters.AddWithValue("[DoubleSection]", (Boolean)isMuliSection);
                            InsertCommand.Parameters.AddWithValue("[PaintedBox]", (Boolean)PaintedBox);
                            InsertCommand.Parameters.AddWithValue("[RatedNeutral200]", (Boolean)RatedNeutral200);

                            if (hasDates) 
                            {
                                InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                                InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);
                                InsertCommand.Parameters.AddWithValue("EnteredDate", ReleaseDate.Text);
                            }

                            InsertCommand.Parameters.AddWithValue("FilePath", strPathPDF);
                            InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                            //InsertCommand.Parameters.AddWithValue("Suffix", Suffix.Text);
                            InsertCommand.Parameters.AddWithValue("Catalogue", BoxCatalogue.Text);
                            InsertCommand.Parameters.AddWithValue("ImageFilePath", strPathImage + "_" + Item.Text + ".png");

                            InsertCommand.ExecuteNonQuery();

                        } //end using command

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

                        } //end using command


                        Status.Content = GO_Item.Text + " SUCCESSFULLY INSERTED";

                        DataGridRow dataGridRow = dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) as DataGridRow;
                        if (dataGridRow != null)
                            dataGridRow.Background = System.Windows.Media.Brushes.LightGreen;
                    }
                }
                catch
                {
                    Status.Content = GO_Item.Text + " INSERTION ERROR";
                    MessageBox.Show("Error Occurred While Inserting Entry");
                }
            }
        }



        private void updateDB(int p) 
        {
            try
            {
                Boolean Duplicate = false;
                if (Utility.isDuplicate(GO_Item.Text,Utility.ProductGroup.PRL123))
                {
                    MessageBox.Show("You Have A Duplicate. GO Items Must Be Unique.", "Duplicate Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                    Duplicate = true;
                }          

                if (Duplicate == false)
                {
                    LoadAllBooleanValues();


                    string Tracking = "InDevelopment";


                    Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(strPathPDF), GO_Item.Text + "_CONSTR.pdf"), (BitmapImage)image[p]);
                    Utility.SaveBitmapAsPNGinImages(strPathImage + "_" + Item.Text + ".png", (BitmapImage)image[p]);


                    string commandStr = "insert into PRL123 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, BoxEarly, [AMO], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, Catalogue, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand InsertCommand = new OleDbCommand(commandStr, MainWindow.LPcon))
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
                        //InsertCommand.Parameters.AddWithValue("Bidman", ToByteArray(DrawInfo(image[p])));
                        InsertCommand.Parameters.AddWithValue("BoxEarly", false);
                        InsertCommand.Parameters.AddWithValue("[AMO]", AMOFound);

                        InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", isSpecialCustomer);

                        InsertCommand.Parameters.AddWithValue("[ServiceEntrance]", isServiceEntrance);
                        InsertCommand.Parameters.AddWithValue("[DoubleSection]", isMuliSection);
                        InsertCommand.Parameters.AddWithValue("[PaintedBox]", PaintedBox);
                        InsertCommand.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200);

                        InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                        InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);
                        InsertCommand.Parameters.AddWithValue("EnteredDate", EnteredDate.Text);
                        InsertCommand.Parameters.AddWithValue("FilePath", strPathPDF);
                        InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                        //InsertCommand.Parameters.AddWithValue("Suffix", Suffix.Text);
                        InsertCommand.Parameters.AddWithValue("Catalogue", ProductSpecialist);
                        InsertCommand.Parameters.AddWithValue("ImageFilePath", strPathImage + "_" + Item.Text + ".png");

                        InsertCommand.ExecuteNonQuery();

                    } //end using command


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

                    } //end using command

                    Status.Content = GO_Item.Text + " SUCCESSFULLY INSERTED";
                }
            }
            catch
            {
                Status.Content = GO_Item.Text + " INSERTION ERROR";
                MessageBox.Show("Error Occurred While Updating Database");
            }
        }






       

      

        
   








        //used to alter partNames in isAMO
        private string convertToRequired(string partname, int enclosure, int paint, int mount, int multiple)
        {
            string output = partname;
            
            if (output != null)
            {
                if (!(output.StartsWith("CN")) && !(output.StartsWith("ItemNumber")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P25")) && !(output.StartsWith("A29")) && !(output.StartsWith("H5")))
                {
                    if(output.StartsWith("JD3") || output.StartsWith("JDB3") || output.StartsWith("HJD3") || output.StartsWith("KD3") || output.StartsWith("KDB3") && !(output.StartsWith("JDB3070") || output.StartsWith("JDB3090") || output.StartsWith("JD3070") || output.StartsWith("JD3090") || output.StartsWith("HJD3070") || output.StartsWith("HJD3090")))
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

                    else if(output.StartsWith("EZB") && multiple == 3 && mount == 0)
                    {
                        output = TripleFlush(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple == 3 && mount == 1)
                    {
                        output = TripleSurface(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple==2 && mount == 0)
                    {
                        output = DoubleFlush(output, enclosure, paint);
                    }
                    else if (output.StartsWith("EZB") && multiple==2 && mount == 1)
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
            else if(enclosure == 0 && !(paint == 1))
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
            else if ((enclosure == 1|| enclosure == 2) && paint == 1)
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


        private string TripleFlush(string partname, int e, int p)
        {
            string a = partname;
            int enclosure = e;
            int paint = p;
            if(!(paint == 1))
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
                DictionaryLoop(a,1);

                a = a.Replace("C-EZBP", "EZT");
                a = a.Replace("RC", "S");
            }
            if(enclosure == 0 || enclosure ==2)
            {
                string RainCover = Utility.ReplacePart("CE24331H02");
                DictionaryLoop(RainCover, 1);
            }
            return a;
        }




        //if par exists in partslist, will add to quantity, else it will add a new instance to partslist
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
            public string IsActive { get; set; }
            public string Description { get; set; }
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
            foreach(XmlNode node in BMLines)    //iterate through BMLines (BMLineItems -> Materials List Information)
            {
                if(node.OuterXml.ToString().Contains("PRL1") || node.OuterXml.ToString().Contains("PRL2") ||  node.OuterXml.ToString().Contains("PRL3"))
                {
                    
                    XmlNodeList n = node.ChildNodes;    //NodeList of all BMLineItem under BMLineItems
                    
                    int mount = -1;
                    int encl = -1;
                    int paint = -1;
                    int multiple = 1;
                    foreach (XmlNode kid in lines[counter])  //NodeList of each line GoItem (BMConfiguredLineItem)
                    {
                        string output = kid.OuterXml.ToString();    //line item node by node  

                        if (!(output.StartsWith("CN")) && !(output.StartsWith("ItemNumber")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("Start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P25")) && !(output.StartsWith("A29")) && !(output.StartsWith("H5")))
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
                                string output = Utility.getBetween(child.OuterXml.ToString(), "V=\"", "\"");    
                                if (!(output.StartsWith("CN")) && !(output.Contains("-")) && !(output.Contains("PROV")) && !(output.Contains("start")) && !(output.StartsWith("S3")) && !(output.StartsWith("P2")) && !(output == "C1") && !(output.StartsWith("A29")) && !(output.StartsWith("H5")))
                                {
                                    currentpart.setName(convertToRequired(output, encl, paint, mount, multiple));
                                }
                            }
                            if (child.OuterXml.ToString().Contains("\"Quantity\""))             //get quantity value of current part
                            {
                                currentpart.addToQuantity(Int32.Parse(Utility.getBetween(child.OuterXml.ToString(), "V=\"", "\"")));
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
                
                if(Utility.standardAMO(partslist[i].Get_partName()))                                //check PullSequence if standardAMO
                {
                    statuses.Add(info.AMO);
                }
                else if(Utility.KanBanSpike(partslist[i].Get_partName(), partslist[i].Get_Amount()))       //check PullSequence if KanBanSpike
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


            //write AMOreport
            for (int i = 0; i < 3; i++)
            {
                if(i == 0)
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
            }


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























        private void Automatic_Insert()
        {
                try
                {
                    if (PDFLoaded == false || XMLLoaded == false)
                    {
                        MessageBox.Show("Cannot Automatically Insert");
                    }
                    else
                    {
                           
                            string commandText = "select [GO Item],[GO],[Item],[Suffix],[Shop Order],[Shop Order B],[Shop Order T],[Customer],[Qty],[Entered Date],[Release Date],[Commit Date],[Product Specialist] from [tblOrderStatus] where [Prod Group] in " + Utility.GetProductNameListInString(Views.Configuration.PRL123names) + " and [GO]='" + GOnum + "'";
                            using (OleDbCommand cmd = new OleDbCommand(commandText, MainWindow.Mcon))
                            {
                                for (int i = 0; i < pages.Length; i++)
                                {
                                    loadXML(i);
                                    using (OleDbDataReader rd = cmd.ExecuteReader())
                                    {
                                        while (rd.Read())
                                        {
                                            if (rd[2].ToString() == Item.Text)
                                            {

                                                GO1.Text = rd[1].ToString();
                                                GO_Item.Text = rd[0].ToString();
                                                Item.Text = rd[2].ToString();
                                                Suffix.Text = rd[3].ToString();
                                                ShopOrder.Text = rd[4].ToString();
                                                ShopOrderBox.Text = rd[5].ToString();
                                                ShopOrderTrim.Text = rd[6].ToString();
                                                Customer.Text = rd[7].ToString();
                                                Qty.Text = rd[8].ToString();
                                                EnteredDate.Text = rd[9].ToString();
                                                ReleaseDate.Text = rd[10].ToString();
                                                CommitDate.Text = rd[11].ToString();
                                                ProductSpecialist = rd[12].ToString();
                                                Urgency.Text = "N";
                                            }
                                        }
                                    }   //end using reader
                                    updateDB(i);
                                }
                            } //end using command

                        Status.Content = GOnum + " AUTO INSERTED SUCCESSFULLY";
                    }
                }
                catch
                {
                    Status.Content = GOnum + " INSERTION ERROR";
                    MessageBox.Show("An Error Occurred Trying To Automatically Insert An Entry");
                }
        }


        private void Auto_Insert(object sender, RoutedEventArgs e)
        {
           Automatic_Insert();
        }




        private void DoubleClickInsert(object sender, MouseButtonEventArgs e)
        {
            if (XMLLoaded == true && PDFLoaded == true)
            {
                try
                {
                    Boolean Duplicate = false;
                    if (Utility.isDuplicate(GO_Item.Text,Utility.ProductGroup.PRL123))
                    {
                        MessageBox.Show("You Have A Duplicate. GO Items Must Be Unique.", "Duplicate Detected", MessageBoxButton.OK, MessageBoxImage.Information);
                        Duplicate = true;
                    }

                    if (Duplicate == false)
                    {
                        LoadAllBooleanValues();


                        string Tracking;
                        if (Approve.IsChecked == true)
                        {
                            Tracking = "MIComplete";
                        }
                        else
                        {
                            Tracking = "InDevelopment";
                        }

                        Utility.SaveImageToPdf(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(strPathPDF), GO_Item.Text + "_CONSTR.pdf"), (BitmapImage)image[ImagePage]);
                        Utility.SaveBitmapAsPNGinImages(strPathImage + "_" + Item.Text + ".png", (BitmapImage)image[ImagePage]);


                        string commandStr = "insert into PRL123 (GO_Item, [GO], ShopOrderInterior, ShopOrderBox, ShopOrderTrim, Customer, Quantity, Tracking, Urgency, BoxEarly, [AMO], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], ReleaseDate, CommitDate, EnteredDate, FilePath, ProductSpecialist, Catalogue, ImageFilePath) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        using (OleDbCommand InsertCommand = new OleDbCommand(commandStr, MainWindow.LPcon))
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
                            //InsertCommand.Parameters.AddWithValue("Bidman", ToByteArray(DrawInfo(image[page])));
                            InsertCommand.Parameters.AddWithValue("BoxEarly", BoxFirst.IsChecked);
                            InsertCommand.Parameters.AddWithValue("[AMO]", AMO.IsChecked);

                            InsertCommand.Parameters.AddWithValue("[SpecialCustomer]", isSpecialCustomer);

                            InsertCommand.Parameters.AddWithValue("[ServiceEntrance]", isServiceEntrance);
                            InsertCommand.Parameters.AddWithValue("[DoubleSection]", isMuliSection);
                            InsertCommand.Parameters.AddWithValue("[PaintedBox]", PaintedBox);
                            InsertCommand.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200);


                            InsertCommand.Parameters.AddWithValue("ReleaseDate", ReleaseDate.Text);
                            InsertCommand.Parameters.AddWithValue("CommitDate", CommitDate.Text);
                            InsertCommand.Parameters.AddWithValue("EnteredDate", ReleaseDate.Text);
                            InsertCommand.Parameters.AddWithValue("FilePath", strPathPDF);
                            InsertCommand.Parameters.AddWithValue("ProductSpecialist", ProductSpecialist);
                            //InsertCommand.Parameters.AddWithValue("Suffix", Suffix.Text);
                            InsertCommand.Parameters.AddWithValue("Catalogue", BoxCatalogue.Text);
                            InsertCommand.Parameters.AddWithValue("ImageFilePath", strPathImage + "_" + Item.Text + ".png");

                            InsertCommand.ExecuteNonQuery();
                        } //end using command


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

                        } //end using command


                        Status.Content = GO_Item.Text + " SUCCESSFULLY INSERTED";

                        DataGridRow dataGridRow = dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) as DataGridRow;
                        if (dataGridRow != null)
                            dataGridRow.Background = System.Windows.Media.Brushes.LightGreen;
                    }
                }
                catch
                {
                    Status.Content = GO_Item.Text + " INSERTION ERROR";
                    MessageBox.Show("Error Occurred While Double-Click Inserting Entry");
                }
            }
        }








    }
}




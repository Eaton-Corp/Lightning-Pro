using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Data;
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
    /// Interaction logic for MaterialPlanning.xaml
    /// </summary>
    public partial class MaterialPlanning : UserControl
    {
        public MaterialPlanning()
        {
            InitializeComponent();
            ListGenerator_Set();
        }

        private void ListGenerator_Set()
        {
            GeneratorGrid.Visibility = Visibility.Visible;
            TablesGrid.Visibility = Visibility.Hidden;

            btnMaterialsGenerator.Background = System.Windows.Media.Brushes.DarkBlue;
            btnEditTables.Background = System.Windows.Media.Brushes.Blue;

            updateStatus("");
        }

        private void MaterialsTables_Set()
        {
            GeneratorGrid.Visibility = Visibility.Hidden;
            TablesGrid.Visibility = Visibility.Visible;

            btnEditTables.Background = System.Windows.Media.Brushes.DarkBlue;
            btnMaterialsGenerator.Background = System.Windows.Media.Brushes.Blue;

            updateStatus("");

            loadPullPartStatus();
            loadPullSequence();
            loadReplacementParts();
        }

        private void updateStatus(string command)
        {
            Status.Content = command;
        }


        private void btnGenerator_Click(object sender, RoutedEventArgs e)
        {
            ListGenerator_Set();
        }

        private void Tables_Click(object sender, RoutedEventArgs e)
        {
            MaterialsTables_Set();
        }





        //List Generator Code
        private async Task<int> TurnOnStatus()
        {
            updateStatus("LOADING ...");
            await Task.Delay(500);
            return 1;
        }

        public enum info
        {
            None, AMO, KanbanSpike, KB, Main
        }

        public class Parts
        {
            public string GoItem { get; set; }
            public string PartName { get; set; }
            public int Quantity { get; set; }
            public info Status { get; set; }
            public string IsActive { get; set; }
            public string Description { get; set; }
        }

        List<Parts> partslist;

        private async void XML_Upload(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "Image files|*.XML;*.tif|All files|*.*";
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    Task<int> ShowProgressBar = TurnOnStatus();
                    int result = await ShowProgressBar;


                    //loads the xml into xDoc using the filepath
                    string filepath = ofg.FileName;
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(filepath);

                    //each node is a line item
                    XmlNodeList BMConfiguredLineItemNodes = xDoc.GetElementsByTagName("BMConfiguredLineItem");

                    GeneratePartsList(BMConfiguredLineItemNodes);

                    updateStatus("XML SUCCESSFULLY UPLOADED");
                }
            }
            catch
            {
                updateStatus("XML UPLOAD ERROR");
                MessageBox.Show("Error Occurred Uploading XML");
            }
        }

        private void GeneratePartsList(XmlNodeList BMConfiguredLineItemNodes)
        {
            partslist = new List<Parts>();

            foreach (XmlNode lineItemNode in BMConfiguredLineItemNodes) 
            {
                //get GoItem
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

                string lineItemGO = GO + "-" + itemNum;     //GOItem number for line item 


                //main line for product

                XmlNode MainMaterialLineItemNode = lineItemNode.SelectSingleNode("BMLineItem");     
                XmlNodeList MainLineChildNodes = MainMaterialLineItemNode.ChildNodes;
                int check = 0;
                string Description = "";
                string NamePart = "";
                string Quantity = "";
                foreach (XmlNode childNode in MainLineChildNodes)
                {
                    if (childNode.OuterXml.ToString().Contains("\"Description\""))
                    {
                        Description = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                        check++;
                    }
                    if (childNode.OuterXml.ToString().Contains("\"CatalogNumber\""))
                    {
                        NamePart = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                        check++;
                    }
                    if (childNode.OuterXml.ToString().Contains("\"Quantity\""))
                    {
                        Quantity = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                        check++;
                    }
                    if (check == 3) 
                    {
                        partslist.Add(new Parts() { GoItem = lineItemGO, PartName = NamePart, Quantity = Int32.Parse(Quantity), Status = info.Main, IsActive = "Main", Description = Description });
                        break;
                    }
                }


                //List of Materials

                XmlNode MaterialLineItemsNode = lineItemNode.SelectSingleNode("BMLineItems");     //node with all list of materials for line item
                XmlNodeList MaterialListNodes = MaterialLineItemsNode.ChildNodes;
                foreach (XmlNode BMLineItem in MaterialListNodes)                    
                {
                    XmlNodeList AttributeNodes = BMLineItem.ChildNodes;
                    int check2 = 0;
                    string Description2 = "";
                    string NamePart2 = "";
                    string Quantity2 = "";
                    foreach (XmlNode childNode in AttributeNodes)
                    {
                        if (childNode.OuterXml.ToString().Contains("\"Description\""))
                        {
                            Description2 = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            check2++;
                        }
                        if (childNode.OuterXml.ToString().Contains("\"CatalogNumber\""))
                        {
                            NamePart2 = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            check2++;
                        }
                        if (childNode.OuterXml.ToString().Contains("\"Quantity\""))
                        {
                            Quantity2 = Utility.getBetween(childNode.OuterXml.ToString(), "V=\"", "\"");
                            check2++;
                        }
                        if (check2 == 3)
                        {
                            NamePart2 = Utility.ReplacePart(NamePart2);
                            Boolean isInItemMaster = false;

                            string[] outputPullPartStatus = Utility.PullPartStatus(NamePart2);
                            if (!string.IsNullOrEmpty(outputPullPartStatus[0]))                 //checks if IsActive is empty
                            {
                                isInItemMaster = true;
                            }
                            if (!string.IsNullOrEmpty(outputPullPartStatus[1])) 
                            {
                                Description2 = outputPullPartStatus[1];
                            }

                            info PartStatus;
                            if (isInItemMaster) 
                            {
                                if (Utility.standardAMO(NamePart2))                                //check PullSequence if standardAMO
                                {
                                    PartStatus = info.AMO;
                                }
                                else if (Utility.KanBanSpike(NamePart2, Int32.Parse(Quantity2)))       //check PullSequence if KanBanSpike
                                {
                                    PartStatus = info.KanbanSpike;
                                }
                                else
                                {
                                    PartStatus = info.KB;
                                }
                            }
                            else
                            {
                                PartStatus = info.None;
                            }

                            partslist.Add(new Parts() { GoItem = lineItemGO, PartName = NamePart2, Quantity = Int32.Parse(Quantity2), Status = PartStatus, IsActive = outputPullPartStatus[0], Description = Description2 });
                            break;
                        }
                    }
                }
            }//end for loop with line items

            GeneratorDG.ItemsSource = partslist;

            GenerateUniquePartsList();
        }


        public class UniqueParts
        {
            public string PartName { get; set; }
            public int Quantity { get; set; }
            public info Status { get; set; }
            public string IsActive { get; set; }
            public string Description { get; set; }
        }

        List<UniqueParts> UniquePartsList;

        private void GenerateUniquePartsList() 
        {
            UniquePartsList = new List<UniqueParts>();

            for (int i = 0; i < partslist.Count; i++) 
            { 
                string partName = partslist[i].PartName;
                if (AlreadyContains(UniquePartsList, partName))
                {
                    UniquePartsList[FindIndex(UniquePartsList, partName)].Quantity += partslist[i].Quantity;
                }
                else 
                {
                    UniquePartsList.Add(new UniqueParts() { PartName = partslist[i].PartName, Quantity = partslist[i].Quantity, Status = partslist[i].Status, IsActive = partslist[i].IsActive, Description = partslist[i].Description });
                }
            }


            //re-check Statuses of unique part list
            for (int i = 0; i < UniquePartsList.Count; i++)
            {
                if (UniquePartsList[i].Status == info.KB) 
                {
                    if (Utility.KanBanSpike(UniquePartsList[i].PartName, UniquePartsList[i].Quantity))       //check PullSequence if KanBanSpike
                    {
                        UniquePartsList[i].Status = info.KanbanSpike;
                    }
                }
            }

            UniqueGeneratorDG.ItemsSource = UniquePartsList;
        }

        private Boolean AlreadyContains(List<UniqueParts> UniquePartsList, string part)
        {
            if (UniquePartsList != null)
            {
                foreach (UniqueParts par in UniquePartsList)
                {
                    if (par.PartName.Contains(part))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int FindIndex(List<UniqueParts> UniquePartsList, string part)
        {
            int counter = 0;
            foreach (UniqueParts par in UniquePartsList)
            {
                if (par.PartName.Contains(part))
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }








        //Material Tables Page Code


        private void loadPullPartStatus() 
        {
            DataTable dt = Utility.SearchLP("select * from [PullPartStatus]");
            PullPartStatusDG.ItemsSource = dt.DefaultView;
        }

        private void loadPullSequence()
        {
            DataTable dt = Utility.SearchLP("select * from [PullSequence]");
            PullSequenceDG.ItemsSource = dt.DefaultView;
        }

        private void loadReplacementParts()
        {
            DataTable dt = Utility.SearchLP("select * from [ReplacementParts]");
            ReplacementPartsDG.ItemsSource = dt.DefaultView;
        }



        

        //used to check if the user is entering a duplicate item
        private Boolean IsDuplicate(string Part, string Table, string Field)
        {
            string query = "select [" +Field+ "] from [" +Table+ "] where [" +Field+ "]='" + Part + "'";

            using (DataTableReader dtr = Utility.loadData(query))
            {
                while (dtr.Read())
                {
                    if (dtr[0] != null)
                    {
                        return true;      //there is a duplicate in LPdatabase
                    }
                }
            } //end using reader

            return false;
        }
        




        private void insertButtonClickedPullPart(object sender, RoutedEventArgs e)
        {
            string command = "INSERT INTO [PullPartStatus](Item, Description, ItemStatus) VALUES(" + ItemPullPart.Text + ", " + DescriptionPullPart.Text + ", " + StatusPullPart.Text + ")";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullPart.Text + " INSERTED");
        }

        private void updateButtonClickedPullPart(object sender, RoutedEventArgs e)
        {
            string command = "UPDATE [PullPartStatus] SET [Description] ='" + DescriptionPullPart.Text + "', [ItemStatus] = '" + StatusPullPart.Text + "' WHERE [Item]='" + ItemPullPart.Text + "'";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullPart.Text + " UPDATED");
        }

        private void deleteButtonClickedPullPart(object sender, RoutedEventArgs e)
        {
            string command = "DELETE * FROM [PullPartStatus] WHERE [Item]='" + ItemPullPart.Text + "'";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullPart.Text + " DELETED");
        }

        private void clearButtonClickedPullPart(object sender, RoutedEventArgs e)
        {
            ItemPullPart.Clear();
            DescriptionPullPart.Clear();
            StatusPullPart.Clear();
        }







        private void insertButtonClickedPullSequence(object sender, RoutedEventArgs e)
        {
            string command = "INSERT INTO [PullSequence](Item, Size) VALUES(" +ItemPullSequence.Text+ ", " +Int32.Parse(SizePullSequence.Text)+ ")";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullSequence.Text + " INSERTED");
        }

        private void updateButtonClickedPullSequence(object sender, RoutedEventArgs e)
        {
            string command = "UPDATE [PullSequence] SET [Size] =" + Int32.Parse(SizePullSequence.Text) + " WHERE [Item]='" + ItemPullSequence.Text + "'";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullSequence.Text + " UPDATED");
        }

        private void deleteButtonClickedPullSequence(object sender, RoutedEventArgs e)
        {
            string command = "DELETE * FROM [PullSequence] WHERE [Item]='" + ItemPullSequence.Text + "'";
            Utility.executeNonQueryLP(command);
            updateStatus(ItemPullSequence.Text + " DELETED");
        }

        private void clearButtonClickedPullSequence(object sender, RoutedEventArgs e)
        {
            ItemPullSequence.Clear();
            SizePullSequence.Clear();
        }






        private void insertButtonClickedReplacement(object sender, RoutedEventArgs e)
        {
            string command = "INSERT INTO [ReplacementParts](Find, Replace, Description) VALUES(" + FindReplacement.Text + ", " + ReplaceReplacement.Text + ", " + DescriptionReplacement.Text + ")";
            Utility.executeNonQueryLP(command);
            updateStatus(FindReplacement.Text + " INSERTED");
        }

        private void updateButtonClickedReplacement(object sender, RoutedEventArgs e)
        {
            string command = "UPDATE [ReplacementParts] SET [Replace] ='" +ReplaceReplacement.Text+ "', [Description] = '" +DescriptionReplacement.Text+ "' WHERE [Find]='" +FindReplacement.Text+ "'";
            Utility.executeNonQueryLP(command);
            updateStatus(FindReplacement.Text + " UPDATED");
        }

        private void deleteButtonClickedReplacement(object sender, RoutedEventArgs e)
        {
            string command = "DELETE * FROM [ReplacementParts] WHERE [Find]='" + FindReplacement.Text + "'";
            Utility.executeNonQueryLP(command);
            updateStatus(FindReplacement.Text + " DELETED");
        }
        private void clearButtonClickedReplacement(object sender, RoutedEventArgs e)
        {
            FindReplacement.Clear();
            ReplaceReplacement.Clear();
            DescriptionReplacement.Clear();
        }

        
    }
}

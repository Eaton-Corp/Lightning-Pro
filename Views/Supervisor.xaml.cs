using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
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
using QRCoder;
using System.Collections.ObjectModel;

namespace LightningPRO.Views
{
    /// <summary>
    /// Interaction logic for Supervisor.xaml
    /// </summary>
    public partial class Supervisor : UserControl
    {
        int Selected = -1;      //autonumber ID of job item + used as a checking variable
        string SelectedGO;
        string GOnum;
        string customer;

        List<int> MultipleSelected;             //autonumber ID's of job items
        List<string> MultipleSelectedGO;

        string Current_Tab;

        Utility.ProductGroup CurrentProduct;
        string ProductTable;

        public Supervisor()
        {
            InitializeComponent();

            Field.Text = "GO_Item";
            Current_Tab = "InDevelopment";
            PRL123_Set();
            ButtonColorChanges();
        }


        private void LoadGrid()
        {
            try
            {
                DataTable dt;
                string query = "";
                if (Current_Tab == "Search")
                {
                    Search();
                    return;
                }
                else
                {
                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted], [Notes] from [PRL123] where [Tracking]='" + Current_Tab + "'";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [LabelsPrinted], [Notes] from [PRL4] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [LabelsPrinted], [Notes] from [PRLCS] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                }

                dt = Utility.SearchLP(query);
                dg.ItemsSource = dt.DefaultView;
                HideFullNotesColoumn();
            }
            catch
            {
                MessageBox.Show("Unable To Load Grid");
            }
        }


        private void Development_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "InDevelopment";
            LoadGrid();
            ButtonColorChanges();
        }

        private void MI_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "MIComplete";
            LoadGrid();
            ButtonColorChanges();
        }

        private void Production_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Production";
            LoadGrid();
            ButtonColorChanges();
        }

        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Shipping";
            LoadGrid();
            ButtonColorChanges();
        }

        private void ButtonColorChanges()
        {
            if (Current_Tab.Equals("Shipping"))
            {
                Shipping.Background = System.Windows.Media.Brushes.DarkBlue;
                Production.Background = System.Windows.Media.Brushes.Blue;
                MIComplete.Background = System.Windows.Media.Brushes.Blue;
                Development.Background = System.Windows.Media.Brushes.Blue;
                SearchButton.Background = System.Windows.Media.Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Production"))
            {
                Shipping.Background = System.Windows.Media.Brushes.Blue;
                Production.Background = System.Windows.Media.Brushes.DarkBlue;
                MIComplete.Background = System.Windows.Media.Brushes.Blue;
                Development.Background = System.Windows.Media.Brushes.Blue;
                SearchButton.Background = System.Windows.Media.Brushes.LightGray;
            }
            else if (Current_Tab.Equals("MIComplete"))
            {
                Shipping.Background = System.Windows.Media.Brushes.Blue;
                Production.Background = System.Windows.Media.Brushes.Blue;
                MIComplete.Background = System.Windows.Media.Brushes.DarkBlue;
                Development.Background = System.Windows.Media.Brushes.Blue;
                SearchButton.Background = System.Windows.Media.Brushes.LightGray;
            }
            else if (Current_Tab.Equals("InDevelopment"))
            {
                Shipping.Background = System.Windows.Media.Brushes.Blue;
                Production.Background = System.Windows.Media.Brushes.Blue;
                MIComplete.Background = System.Windows.Media.Brushes.Blue;
                Development.Background = System.Windows.Media.Brushes.DarkBlue;
                SearchButton.Background = System.Windows.Media.Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Search"))
            {
                Shipping.Background = System.Windows.Media.Brushes.Blue;
                Production.Background = System.Windows.Media.Brushes.Blue;
                MIComplete.Background = System.Windows.Media.Brushes.Blue;
                Development.Background = System.Windows.Media.Brushes.Blue;
                SearchButton.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        private void UpdateStatus(string command)
        {
            Status.Content = command;
            Status.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void PRL123_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL123;
            ProductTable = "PRL123";
            PWL123.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.Blue;
            LoadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            ProductTable = "PRL4";
            PWL4.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.Blue;
            LoadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            ProductTable = "PRLCS";
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.DarkBlue;
            LoadGrid();
        }

        private void PRL123_Click(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
            Selected = -1;
        }

        private void PRL4_Click(object sender, RoutedEventArgs e)
        {
            PRL4_Set();
            Selected = -1;
        }

        private void PRLCS_Click(object sender, RoutedEventArgs e)
        {
            PRLCS_Set();
            Selected = -1;
        }


        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;

            if (gd.SelectedItems != null)
            {
                MultipleSelected = new List<int>();
                MultipleSelectedGO = new List<string>();
                for (int i = 0; i < gd.SelectedItems.Count; i++)
                {
                    dynamic row = gd.SelectedItems[i];
                    MultipleSelected.Add(row["ID"]);
                    MultipleSelectedGO.Add(row["GO_Item"]);
                }
            }

            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;
                Selected = row["ID"];
                SelectedGO = row["GO_Item"];
                GOnum = row["GO"];
                customer = row["Customer"];
                UpdateStatus(SelectedGO + " SELECTED");
            }
        }



        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                Edit sf = new Edit(SelectedGO, CurrentProduct);
                sf.Show();
            }
        }

        private void SRClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Selected != -1)
                {
                    SR sf = new SR(Selected, CurrentProduct, ProductTable);
                    sf.Show();
                }
            }
            catch
            {
                MessageBox.Show("Unable To Print The Shortage Report, Please Try Again");
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Selected != -1)
                {
                    if (Utility.HaveLabelsPrinted(SelectedGO,ProductTable))
                    {
                        if (MessageBox.Show("The labels for the GO Item have already been printed.\nWould you like to print them again?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    if (CurrentProduct == Utility.ProductGroup.PRL123 || CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        PrintPage sf = new PrintPage(SelectedGO, CurrentProduct, ProductTable);
                        sf.Show();
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS) 
                    {
                        PrintPageCS sf = new PrintPageCS(SelectedGO, customer);       //only one input of SelectedGO indicates only a single line item needs to be printed
                        //sf.Show();        Don't need to show the xaml page to print label for CS
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable To View Label, Please Try Again");
            }
        }


        private void Labels_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Selected != -1)
                {                    
                    if (Utility.HaveAllLabelsPrinted(GOnum, ProductTable))
                    {
                        if (MessageBox.Show("All labels for the GO Number have already been printed.\nWould you like to print them again?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    List<string> termsList = GetGOs(GOnum);

                    if (termsList.Count > 0)
                    {
                        if (CurrentProduct == Utility.ProductGroup.PRL123 || CurrentProduct == Utility.ProductGroup.PRL4)
                        {
                            PrintLabels sf = new PrintLabels(termsList, CurrentProduct, ProductTable);
                            //sf.Show();      Don't need to show the xaml page to print labels
                        }
                        else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                        {
                            PrintPageCS sf = new PrintPageCS(termsList, customer);   //different input parameter indicates all line items in production need to be printed
                            //sf.Show();        Don't need to show the xaml page to print multiple labels for CS
                        }
                    }
                    else
                    {
                        MessageBox.Show("Approve Jobs For Production Before Printing All Labels");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable To Print Labels, Please Try Again");
            }
        }

        private List<string> GetGOs(string GO)
        {
            List<string> termsList = new List<string>();

            DataTable dt;
            string query;

            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                query = "select [GO_Item] from [PRL123] where [GO]='" + GO + "' and [Tracking]='Production'";       //can print labels after supervisor approves
            }
            else
            {
                query = "select [GO_Item] from [" + ProductTable + "] where [GO]='" + GO + "' and [Tracking]='Production' and [PageNumber] = 0";
            }
            dt = Utility.SearchLP(query);
            using (DataTableReader dtr = new DataTableReader(dt))
            {
                while (dtr.Read())
                {
                    termsList.Add(dtr[0].ToString());
                }
            } //end using reader
            return termsList;
        }



        private void Click_Approve(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (int id in MultipleSelected)
                        {
                            string query = "update [PRL123] set [Tracking]='Production' where [ID]=" + id.ToString();
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='Production' where [ID]=" + Selected.ToString();
                        Utility.ExecuteNonQueryLP(query);

                        LoadGrid();
                        UpdateStatus(SelectedGO + " SUCCESSFULLY APPROVED");
                    }
                }
                else
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (string goItem in MultipleSelectedGO)
                        {
                            string query = "update [" + ProductTable + "] set [Tracking]='Production' where [GO_Item]='" + goItem + "'";
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='Production' where [GO_Item]='" + SelectedGO + "'";
                        Utility.ExecuteNonQueryLP(query);

                        LoadGrid();
                        UpdateStatus(SelectedGO + " SUCCESSFULLY APPROVED");
                    }
                }
            }
        }




        private void Click_ProductRecall(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if (CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (int id in MultipleSelected)
                        {
                            string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + id.ToString();
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + Selected.ToString();
                        Utility.ExecuteNonQueryLP(query);

                        LoadGrid();
                        UpdateStatus(SelectedGO + " RECALLED");
                    }
                }
                else
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (string goItem in MultipleSelectedGO)
                        {
                            string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + goItem + "'";
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + SelectedGO + "'";
                        Utility.ExecuteNonQueryLP(query);

                        LoadGrid();
                        UpdateStatus(SelectedGO + " RECALLED");
                    }
                }
            }
        }



        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search();
            }
            catch
            {
                MessageBox.Show("Unable To Search, Please Try Again");
            }
        }

        private void KeyDownClick(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void Search()
        {
            Current_Tab = "Search";
            DataTable dt;
            string query = Utility.SearchQueryGenerator(CurrentProduct, Field.Text, SearchBox.Text);
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            HideFullNotesColoumn();
            ButtonColorChanges();
        }


        private void AddToShopPack_Click(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if (CurrentProduct != Utility.ProductGroup.PRL123)
                {
                    int check = Utility.ExecuteAddToShopPack(SelectedGO, CurrentProduct);

                    if (check == 1)
                    {
                        UpdateStatus(SelectedGO + " PAGE ADDED");
                    }
                    else if (check == -2)
                    {
                        UpdateStatus(SelectedGO + " PAGE ADD ERROR");
                    }
                }
                else
                {
                    MessageBox.Show("This Feature Is Not Available For PRL123");
                }
            }
        }


        private void BLT_Insert(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                string pdfDirectory = Utility.GetDirectoryForOrderFiles(SelectedGO, CurrentProduct);
                int check = Utility.ExecuteBLTinsert(SelectedGO, pdfDirectory, CurrentProduct);

                if (check == 1)
                {
                    UpdateStatus(SelectedGO + " BLT ADDED");
                }
                else if (check == -2)
                {
                    UpdateStatus(SelectedGO + " BLT ERROR");
                }
            }
        }


        private void Constr_Insert(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                string pdfDirectory = Utility.GetDirectoryForOrderFiles(SelectedGO, CurrentProduct);
                int check = Utility.ExecuteCONSTRinsert(SelectedGO, pdfDirectory, CurrentProduct);

                if (check == 1)
                {
                    UpdateStatus(SelectedGO + " CONSTR ADDED");
                }
                else if (check == -2)
                {
                    UpdateStatus(SelectedGO + " CONSTR ERROR");
                }
            }
        }

        private void Click_FindFolder(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                Utility.AccessFolderGO(GOnum);
            }
        }


        private void Open_Records(object sender, RoutedEventArgs e)
        {
            CSArecords sf = new CSArecords();
            sf.Show();
        }

        private void HideFullNotesColoumn()
        {
            //Get the columns collection
            ObservableCollection<DataGridColumn> columns = dg.Columns;

            //set the visibility for end Notes coloumn to hidden
            foreach (DataGridColumn col in columns)
            {
                switch (col.Header.ToString())
                {
                    case "Notes":
                        col.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        private void DG_Loaded(object sender, RoutedEventArgs e)
        {
            HideFullNotesColoumn();
        }

    }
}

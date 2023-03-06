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

namespace PRL123_Final.Views
{
    /// <summary>
    /// Interaction logic for Supervisor.xaml
    /// </summary>
    public partial class Supervisor : UserControl
    {
        int Selected = -1;      //autonumber ID of job item + used as a checking variable
        string SelectedGO;
        string GOnum;

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


        private void loadGrid()
        {
            try
            {
                DataTable dt;
                string query = "";
                if (Current_Tab == "Search")
                {
                    search();
                    return;
                }
                else
                {
                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short] from [PRL123] where [Tracking]='" + Current_Tab + "'";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short] from [PRL4] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short] from [PRLCS] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                }

                dt = Utility.SearchLP(query);
                dg.ItemsSource = dt.DefaultView;
            }
            catch
            {
                MessageBox.Show("Unable To Load Grid");
            }
        }


        private void Development_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "InDevelopment";
            loadGrid();
            ButtonColorChanges();
        }

        private void MI_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "MIComplete";
            loadGrid();
            ButtonColorChanges();
        }

        private void Production_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Production";
            loadGrid();
            ButtonColorChanges();
        }

        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Shipping";
            loadGrid();
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

        private void updateStatus(string command)
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
            loadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            ProductTable = "PRL4";
            PWL4.Background = System.Windows.Media.Brushes.DarkBlue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.Blue;
            loadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            ProductTable = "PRLCS";
            PWL4.Background = System.Windows.Media.Brushes.Blue;
            PWL123.Background = System.Windows.Media.Brushes.Blue;
            PWLCS.Background = System.Windows.Media.Brushes.DarkBlue;
            loadGrid();
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
                updateStatus(SelectedGO + " SELECTED");
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
                    if(CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        PrintPageCS sf = new PrintPageCS(SelectedGO, CurrentProduct);
                        sf.Show();
                    }
                    else
                    {
                        PrintPage sf = new PrintPage(SelectedGO, CurrentProduct);
                        sf.Show();
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
                    List<string> termsList = getGOs(GOnum);

                    if (termsList.Count > 0)
                    {
                        if(CurrentProduct == Utility.ProductGroup.PRLCS)
                        {
                            PrintLabelsCS sf = new PrintLabelsCS(termsList, CurrentProduct);
                            sf.Show();
                        }
                        else
                        {
                            PrintLabels sf = new PrintLabels(termsList, CurrentProduct);

                            // sf.Show();      Don't need to show the xaml page to print labels
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

        private List<string> getGOs(string GO)
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
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='Production' where [ID]=" + Selected.ToString();
                        Utility.executeNonQueryLP(query);

                        loadGrid();
                        updateStatus(SelectedGO + " SUCCESSFULLY APPROVED");
                    }
                }
                else
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (string goItem in MultipleSelectedGO)
                        {
                            string query = "update [" + ProductTable + "] set [Tracking]='Production' where [GO_Item]='" + goItem + "'";
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='Production' where [GO_Item]='" + SelectedGO + "'";
                        Utility.executeNonQueryLP(query);

                        loadGrid();
                        updateStatus(SelectedGO + " SUCCESSFULLY APPROVED");
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
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + Selected.ToString();
                        Utility.executeNonQueryLP(query);

                        loadGrid();
                        updateStatus(SelectedGO + " RECALLED");
                    }
                }
                else
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (string goItem in MultipleSelectedGO)
                        {
                            string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + goItem + "'";
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + SelectedGO + "'";
                        Utility.executeNonQueryLP(query);

                        loadGrid();
                        updateStatus(SelectedGO + " RECALLED");
                    }
                }
            }
        }



        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                search();
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
                search();
            }
        }

        private void search()
        {
            Current_Tab = "Search";
            DataTable dt;
            string query = Utility.searchQueryGenerator(CurrentProduct, Field.Text, Search.Text);
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            ButtonColorChanges();
        }


        private void AddToShopPack_Click(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if (CurrentProduct != Utility.ProductGroup.PRL123)
                {
                    int check = Utility.executeAddToShopPack(SelectedGO, CurrentProduct);

                    if (check == 1)
                    {
                        updateStatus(SelectedGO + " PAGE ADDED");
                    }
                    else if (check == -2)
                    {
                        updateStatus(SelectedGO + " PAGE ADD ERROR");
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
                string pdfDirectory = Utility.getDirectoryForOrderFiles(SelectedGO, CurrentProduct);
                int check = Utility.executeBLTinsert(SelectedGO, pdfDirectory, CurrentProduct);

                if (check == 1)
                {
                    updateStatus(SelectedGO + " BLT ADDED");
                }
                else if (check == -2)
                {
                    updateStatus(SelectedGO + " BLT ERROR");
                }
            }
        }


        private void Constr_Insert(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                string pdfDirectory = Utility.getDirectoryForOrderFiles(SelectedGO, CurrentProduct);
                int check = Utility.executeCONSTRinsert(SelectedGO, pdfDirectory, CurrentProduct);

                if (check == 1)
                {
                    updateStatus(SelectedGO + " CONSTR ADDED");
                }
                else if (check == -2)
                {
                    updateStatus(SelectedGO + " CONSTR ERROR");
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

    }
}

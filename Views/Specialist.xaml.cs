﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
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

namespace PRL123_Final.Views
{
    /// <summary>
    /// Interaction logic for Specialist.xaml
    /// </summary>
    public partial class Specialist : UserControl
    {
        int Selected = -1;      //autonumber ID of job item + used as a checking variable
        string SelectedGO;

        List<int> MultipleSelected;             //autonumber ID's of job items
        List<string> MultipleSelectedGO;

        string Current_Tab;

        Utility.ProductGroup CurrentProduct;
        string ProductTable;

        public Specialist()
        {
            InitializeComponent();
            Field.Text = "GO_Item";
            Current_Tab = "InDevelopment";
            intializeProduct();
            //ButtonColorChanges();
        }

        public void intializeProduct()
        {
            if (MainWindow.ProductGroup.Equals(Utility.ProductGroup.PRLCS))
            {
                PRLCS_Set();
            }
            else if (MainWindow.ProductGroup.Equals(Utility.ProductGroup.PRL4))
            {
                PRL4_Set();
            }
            else if (MainWindow.ProductGroup.Equals(Utility.ProductGroup.PRL123))
            {
                PRL123_Set();
            }

        }


        private void loadGrid()
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
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRL123] where [Tracking]='" + Current_Tab + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [LabelsPrinted], [BoxEarly], [BoxSent] from [PRL4] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRLCS] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                }
            }
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
        }

        private void Development_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "InDevelopment";
            loadGrid();
            //ButtonColorChanges();
        }

        private void MI_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "MIComplete";
            loadGrid();
            //ButtonColorChanges();
        }

        private void Production_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Production";
            loadGrid();
            //ButtonColorChanges();
        }

        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Shipping";
            loadGrid();
            //ButtonColorChanges();
        }


        //private void ButtonColorChanges()
        //{
        //if (Current_Tab.Equals("Shipping"))
        //{
        //  Shipping.Background = Brushes.DarkBlue;
        // Production.Background = Brushes.Blue;
        //MIComplete.Background = Brushes.Blue;
        //Development.Background = Brushes.Blue;
        //SearchButton.Background = Brushes.LightGray;
        //}
        //else if (Current_Tab.Equals("Production"))
        //{
        // Shipping.Background = Brushes.Blue;
        // Production.Background = Brushes.DarkBlue;
        // MIComplete.Background = Brushes.Blue;
        //Development.Background = Brushes.Blue;
        //SearchButton.Background = Brushes.LightGray;
        //}
        //else if (Current_Tab.Equals("MIComplete"))
        //{
        //Shipping.Background = Brushes.Blue;
        //Production.Background = Brushes.Blue;
        //  MIComplete.Background = Brushes.DarkBlue;
        //  Development.Background = Brushes.Blue;
        //   SearchButton.Background = Brushes.LightGray;
        //}
        //else if (Current_Tab.Equals("InDevelopment"))
        //{
        //Shipping.Background = Brushes.Blue;
        //Production.Background = Brushes.Blue;
        //MIComplete.Background = Brushes.Blue;
        //Development.Background = Brushes.DarkBlue;
        //SearchButton.Background = Brushes.LightGray;
        //}
        //else if (Current_Tab.Equals("Search"))
        //{
        //Shipping.Background = Brushes.Blue;
        //Production.Background = Brushes.Blue;
        //MIComplete.Background = Brushes.Blue;
        //Development.Background = Brushes.Blue;
        //  SearchButton.Background = Brushes.Gray;
        //}
        //}



        public void Filter_click(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox selectedItem)
            {
                if (selectedItem.SelectedItem is ComboBoxItem selectedComboBoxItem)
                {
                    string selectedValue = selectedComboBoxItem.Content.ToString();
                    Current_Tab = selectedValue;  // Assuming Current_Tab is a string
                    loadGrid();
                }
            }
        }
        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the visual element that was clicked
            var hitTestResult = VisualTreeHelper.HitTest(dg, e.GetPosition(dg));
            var clickedElement = hitTestResult.VisualHit;

            // Traverse up the visual tree to find the DataGridRow that contains the clicked element
            DataGridRow clickedRow = null;
            while (clickedElement != null)
            {
                if (clickedElement is DataGridRow row)
                {
                    clickedRow = row;
                    break;
                }
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            // If a DataGridRow was found, select it
            if (clickedRow != null)
            {
                DataGrid gd = (DataGrid)sender;
                int rowIndex = clickedRow.GetIndex();

                // Override the current selected index
                dg.SelectedIndex = rowIndex;

                // Trigger the SelectionChanged event
                dg.SelectedItem = dg.Items[rowIndex];
            }
        }

        private void updateStatus(string command)
        {
            Status.Content = command;
            Status.Foreground = Brushes.Green;
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


        private void PRL123_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL123;
            MainWindow.ProductGroup = Utility.ProductGroup.PRL123;
            ProductTable = "PRL123";
            //PWL123.Background = Brushes.DarkBlue;
            //PWL4.Background = Brushes.Blue;
            //PWLCS.Background = Brushes.Blue;
            loadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            MainWindow.ProductGroup = Utility.ProductGroup.PRL4;
            ProductTable = "PRL4";
            //PWL4.Background = Brushes.DarkBlue;
            //PWL123.Background = Brushes.Blue;
            //PWLCS.Background = Brushes.Blue;
            loadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            MainWindow.ProductGroup = Utility.ProductGroup.PRLCS;
            ProductTable = "PRLCS";
            //PWL4.Background = Brushes.Blue;
            //PWL123.Background = Brushes.Blue;
            //PWLCS.Background = Brushes.DarkBlue;
            loadGrid();
        }


        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentProduct == Utility.ProductGroup.PRL123)
            {
                Insert sf = new Insert();
                sf.Show();
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                InsertPRL4 sf = new InsertPRL4();
                sf.Show();
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                InsertPRLCS sf = new InsertPRLCS();
                sf.Show();
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


        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
           
            if(gd.SelectedItems != null)
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
                updateStatus(SelectedGO + " SELECTED");
            }
        }

        private void Click_Approve(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if(CurrentProduct == Utility.ProductGroup.PRL123)
                {
                    if (MultipleSelected.Count > 1)
                    {
                        foreach (int id in MultipleSelected)
                        {
                            string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + id.ToString();
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + Selected.ToString();
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
                            string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + goItem + "'";
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + SelectedGO + "'";
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
                            string query = "update [PRL123] set [Tracking]='InDevelopment' where [ID]=" + id.ToString();
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='InDevelopment' where [ID]=" + Selected.ToString();
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
                            string query = "update [" + ProductTable + "] set [Tracking]='InDevelopment' where [GO_Item]='" + goItem + "'";
                            Utility.executeNonQueryLP(query);
                        }
                        loadGrid();
                        updateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='InDevelopment' where [GO_Item]='" + SelectedGO + "'";
                        Utility.executeNonQueryLP(query);

                        loadGrid();
                        updateStatus(SelectedGO + " RECALLED");
                    }
                }
            }
        }


        private void Click_Delete(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                if (MessageBox.Show("Are you sure you would like to delete the selected entry?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string query = "delete * from [" + ProductTable + "] where [GO_Item]='" + SelectedGO + "'";
                    Utility.executeNonQueryLP(query);
                    Utility.DeleteCSAValues(SelectedGO, CurrentProduct);

                    loadGrid();
                    updateStatus(SelectedGO + " SUCCESSFULLY DELETED");
                }
            }
        }

                    
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            search();
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
            //ButtonColorChanges();
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


        private void Open_Records(object sender, RoutedEventArgs e)
        {
            CSArecords sf = new CSArecords();
            sf.Show();
        }


        private void Click_FindFolder(object sender, RoutedEventArgs e)
        {
            if (Selected != -1)
            {
                Utility.AccessFolderGO(SelectedGO.Substring(0,10));
            }
        }

        private void Click_MassUpdateGO(object sender, RoutedEventArgs e)
        {
            MassUpdateGO window = new MassUpdateGO(SelectedGO.Substring(0,10));
            window.ShowDialog(); // Show the window modally

            //Edit sf = new Edit(SelectedGO, CurrentProduct);
            //sf.Show();
        }
    }
}

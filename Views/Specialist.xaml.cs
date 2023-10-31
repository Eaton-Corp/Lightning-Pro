using System;
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
using System.Collections.ObjectModel;

namespace LightningPRO.Views
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
        }


        private void Specialist_OnLoaded(object sender, RoutedEventArgs e)
        {
            Field.Text = "GO_Item";
            Current_Tab = "InDevelopment";
            IntializeProduct();
            ButtonColorChanges();
        }

        public void IntializeProduct()
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

        private void Click_Refresh(object sender, RoutedEventArgs e)
        {
            Selected = -1;
            LoadGrid();
            UpdateStatus("PAGE REFRESHED");
        }

        private void LoadGrid()
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
                    query = "select Distinct PRL123.[ID], PRL123.[GO_Item], PRL123.[GO], CSALabel.[ProductID], PRL123.[ShopOrderInterior], PRL123.[ShopOrderBox], PRL123.[ShopOrderTrim], PRL123.[SchedulingGroup], PRL123.[JobName], PRL123.[Customer], PRL123.[Quantity], PRL123.[EnteredDate], PRL123.[ReleaseDate], PRL123.[CommitDate], PRL123.[Tracking], PRL123.[Urgency], PRL123.[AMO], PRL123.[BoxEarly], PRL123.[Box Sent], PRL123.[SpecialCustomer], PRL123.[ServiceEntrance], PRL123.[DoubleSection], PRL123.[PaintedBox], PRL123.[RatedNeutral200], PRL123.[DNSB], PRL123.[Complete], PRL123.[Short], PRL123.[NameplateRequired], PRL123.[NameplateOrdered], PRL123.[LabelsPrinted], PRL123.[Notes] from [PRL123] inner join [CSALabel] on PRL123.[GO_Item] = CSALabel.[GO_Item] where PRL123.[Tracking]='" + Current_Tab + "'";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRL4)
                {
                    query = "select Distinct PRL4.[ID], PRL4.[GO_Item], PRL4.[GO], CSALabel.[ProductID], PRL4.[ShopOrderInterior], PRL4.[ShopOrderBox], PRL4.[ShopOrderTrim], PRL4.[SchedulingGroup], PRL4.[JobName], PRL4.[Customer], PRL4.[Quantity], PRL4.[EnteredDate], PRL4.[ReleaseDate], PRL4.[CommitDate], PRL4.[Tracking], PRL4.[Urgency], PRL4.[AMO], PRL4.[SpecialCustomer], PRL4.[ServiceEntrance], PRL4.[PaintedBox], PRL4.[RatedNeutral200], PRL4.[DoorOverDist], PRL4.[DoorInDoor], PRL4.[DNSB], PRL4.[Complete], PRL4.[Short], PRL4.[NameplateRequired], PRL4.[NameplateOrdered], PRL4.[LabelsPrinted], PRL4.[BoxEarly], PRL4.[BoxSent], PRL4.[Notes] from [PRL4] inner join [CSALabel] on PRL4.[GO_Item] = CSALabel.[GO_Item] where PRL4.[Tracking]='" + Current_Tab + "' and PRL4.[PageNumber] = 0";
                }
                else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                {
                    query = "select Distinct PRLCS.[ID], PRLCS.[GO_Item], PRLCS.[GO], CSALabelPRLCS.[ProductID], PRLCS.[ShopOrderInterior], PRLCS.[ShopOrderBox], PRLCS.[ShopOrderTrim], PRLCS.[SchedulingGroup], PRLCS.[JobName], PRLCS.[Customer], PRLCS.[Quantity], PRLCS.[EnteredDate], PRLCS.[ReleaseDate], PRLCS.[CommitDate], PRLCS.[Tracking], PRLCS.[Urgency], PRLCS.[AMO], PRLCS.[SpecialCustomer], PRLCS.[IncLocLeft], PRLCS.[IncLocRight], PRLCS.[CrossBus], PRLCS.[OpenBottom], PRLCS.[ExtendedTop], PRLCS.[PaintedBox], PRLCS.[ThirtyDeepEnclosure], PRLCS.[DNSB], PRLCS.[Complete], PRLCS.[Short], PRLCS.[NameplateRequired], PRLCS.[NameplateOrdered], PRLCS.[LabelsPrinted], PRLCS.[Notes] from [PRLCS] inner join [CSALabelPRLCS] on PRLCS.[GO_Item] = CSALabelPRLCS.[GO_Item] where PRLCS.[Tracking]='" + Current_Tab + "' and PRLCS.[PageNumber] = 0";
                }
            }
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            HideFullNotesColoumn();
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
                Shipping.Background = Brushes.DarkBlue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Production"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.DarkBlue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("MIComplete"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.DarkBlue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("InDevelopment"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.DarkBlue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Search"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.Gray;
            }
        }



        public void Filter_click(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox selectedItem)
            {
                if (selectedItem.SelectedItem is ComboBoxItem selectedComboBoxItem)
                {
                    string selectedValue = selectedComboBoxItem.Content.ToString();
                    Current_Tab = selectedValue;  // Assuming Current_Tab is a string
                    LoadGrid();
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
                int rowIndex = clickedRow.GetIndex();

                // Override the current selected index
                dg.SelectedIndex = rowIndex;

                // Trigger the SelectionChanged event
                dg.SelectedItem = dg.Items[rowIndex];
            }
        }

        private void UpdateStatus(string command)
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
            PWL123.Background = Brushes.DarkBlue;
            PWL4.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            MainWindow.ProductGroup = Utility.ProductGroup.PRL4;
            ProductTable = "PRL4";
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            MainWindow.ProductGroup = Utility.ProductGroup.PRLCS;
            ProductTable = "PRLCS";
            PWL4.Background = Brushes.Blue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.DarkBlue;
            LoadGrid();
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
                UpdateStatus(SelectedGO + " SELECTED");
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
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='MIComplete' where [ID]=" + Selected.ToString();
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
                            string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + goItem + "'";
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY APPROVED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='MIComplete' where [GO_Item]='" + SelectedGO + "'";
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
                            string query = "update [PRL123] set [Tracking]='InDevelopment' where [ID]=" + id.ToString();
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [PRL123] set [Tracking]='InDevelopment' where [ID]=" + Selected.ToString();
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
                            string query = "update [" + ProductTable + "] set [Tracking]='InDevelopment' where [GO_Item]='" + goItem + "'";
                            Utility.ExecuteNonQueryLP(query);
                        }
                        LoadGrid();
                        UpdateStatus("MULTIPLE SUCCESSFULLY RECALLED");
                    }
                    else
                    {
                        string query = "update [" + ProductTable + "] set [Tracking]='InDevelopment' where [GO_Item]='" + SelectedGO + "'";
                        Utility.ExecuteNonQueryLP(query);

                        LoadGrid();
                        UpdateStatus(SelectedGO + " RECALLED");
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
                    Utility.ExecuteNonQueryLP(query);
                    Utility.DeleteCSAValues(SelectedGO, CurrentProduct);

                    LoadGrid();
                    UpdateStatus(SelectedGO + " SUCCESSFULLY DELETED");
                }
            }
        }

                    
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Search();
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

        private void Click_MassUpdateGO(object sender, RoutedEventArgs e)
        {
            if(Selected != -1)
            {
                MassUpdateGO window = new MassUpdateGO(SelectedGO.Substring(0, 10), CurrentProduct);
                window.ShowDialog(); // Show the window modally
            }
        }

        
    }
}

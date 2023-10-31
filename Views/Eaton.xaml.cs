using System;
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
    /// Interaction logic for Eaton.xaml
    /// </summary>
    public partial class Eaton : UserControl
    {     
        string Current_Tab;
        Utility.ProductGroup CurrentProduct;

        public Eaton()
        {
            InitializeComponent();
        }


        private void Eaton_OnLoaded(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
            Field.Text = "GO_Item";
            Current_Tab = "InDevelopment";
            ButtonColorChanges();
            LoadGrid();
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
                        query =
                        "SELECT DISTINCT " +
                        "PRL123.[ID], " +
                        "PRL123.[GO_Item], " +
                        "PRL123.[GO], " +
                        "CSALabel.[ProductID], " +
                        "PRL123.[ShopOrderInterior], " +
                        "PRL123.[ShopOrderBox], " +
                        "PRL123.[ShopOrderTrim], " +
                        "PRL123.[SchedulingGroup], " +
                        "PRL123.[Customer], " +
                        "PRL123.[JobName], " +
                        "PRL123.[Quantity], " +
                        "PRL123.[EnteredDate], " +
                        "PRL123.[ReleaseDate], " +
                        "PRL123.[CommitDate], " +
                        "PRL123.[Tracking], " +
                        "PRL123.[Urgency], " +
                        "PRL123.[AMO], " +
                        "PRL123.[BoxEarly], " +
                        "PRL123.[Box Sent], " +
                        "PRL123.[SpecialCustomer], " +
                        "PRL123.[ServiceEntrance], " +
                        "PRL123.[DoubleSection], " +
                        "PRL123.[PaintedBox], " +
                        "PRL123.[RatedNeutral200], " +
                        "PRL123.[DNSB], " +
                        "PRL123.[Complete], " +
                        "PRL123.[Short], " +
                        "PRL123.[NameplateRequired], " +
                        "PRL123.[NameplateOrdered], " +
                        "PRL123.[LabelsPrinted], " +
                        "PRL123.[Notes] " +
                        "FROM [PRL123] " +
                        "INNER JOIN [CSALabel] " +
                        "ON PRL123.[GO_Item] = CSALabel.[GO_Item] " +
                        "WHERE PRL123.[Tracking]='" + Current_Tab + "' " +
                        "AND PRL123.[PageNumber] = 0";


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
            catch
            {
               MessageBox.Show("An Error Occurred Trying To Access LPdatabase");
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

        // changes the button colour to a darker color allowing the user to know which tab they are currently on 
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


        private void Open_Records(object sender, RoutedEventArgs e)
        {
            CSArecords sf = new CSArecords();
            sf.Show();
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

        private void PRL123_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL123;
            PWL123.Background = Brushes.DarkBlue;
            PWL4.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            PWL4.Background = Brushes.Blue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.DarkBlue;
            LoadGrid();
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

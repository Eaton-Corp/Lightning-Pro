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

namespace PRL123_Final.Views
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

            PRL123_Set();
            Field.Text = "GO_Item"; 
            Current_Tab = "InDevelopment";  
            ButtonColorChanges();
            loadGrid();
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
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRL123] where [Tracking]='" + Current_Tab + "'";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRL4] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRLCS] where [Tracking]='" + Current_Tab + "' and [PageNumber] = 0";
                    }
                }

                dt = Utility.SearchLP(query);
                dg.ItemsSource = dt.DefaultView;
            }
            catch
            {
               MessageBox.Show("An Error Occurred Trying To Access LPdatabase");
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


        private void search()
        {
            Current_Tab = "Search";
            DataTable dt;
            string query = Utility.searchQueryGenerator(CurrentProduct, Field.Text, Search.Text);
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            ButtonColorChanges();
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
            loadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            loadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            PWL4.Background = Brushes.Blue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.DarkBlue;
            loadGrid();
        }

        
    }
}

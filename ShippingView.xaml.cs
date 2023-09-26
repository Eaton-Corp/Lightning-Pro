using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
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
using System.Windows.Shapes;
using QRCoder;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for ShippingView.xaml
    /// </summary>
    public partial class ShippingView : Window
    {
        readonly string GO_Item;

        string[] GOs;
        string[] ShopOrderBoxArr;
        string[] CustomerArr;
        string[] QuantityArr;
        string[] UrgencyArr;
        string[] CatalogueArr;
        string[] imageFilePaths;

        BitmapImage[] BidmanArr;
        BitmapImage[] BoxArr;

        int pg;
        Boolean isLoaded;
        readonly Views.Shipping CurrentShippingWindow;

        
        public ShippingView(string GOI, Views.Shipping ShippingWindow)
        {
            InitializeComponent();
            GO_Item = GOI;
            CurrentShippingWindow = ShippingWindow;

            GetGOs("select [GO_Item], [ShopOrderBox], [Customer], [Quantity], [Urgency], [Bidman], [Catalogue], [ImageFilePath] from [PRL123] where [GO]='" + GO_Item.Substring(0, 10) + "' and [BoxEarly]=True and [Box Sent]=False");         
        }

        private void GetGOs(string query)
        {
            DataTableReader rd = Utility.LoadData(query);
            DataTableReader rb = Utility.LoadData(query);
            
            var counter = 0;
            int pages = 0;
            
            using (rd)
            {
                while (rd.Read())
                {
                    pages++;
                }
            }

            GOs = new string[pages];
            ShopOrderBoxArr = new string[pages];
            CustomerArr = new string[pages];
            QuantityArr = new string[pages];
            UrgencyArr = new string[pages];
            BidmanArr = new BitmapImage[pages];
            BoxArr = new BitmapImage[pages];
            CatalogueArr = new string[pages];
            imageFilePaths = new string[pages];

            using (rb)
            {
                while (rb.Read())
                {
                    GOs[counter] = rb[0].ToString();
                    ShopOrderBoxArr[counter] = rb[1].ToString();
                    CustomerArr[counter] = rb[2].ToString();
                    QuantityArr[counter] = rb[3].ToString();
                    UrgencyArr[counter] = rb[4].ToString();

                    imageFilePaths[counter] = rb[7].ToString();

                    if (string.IsNullOrEmpty(imageFilePaths[counter]))
                    {
                        BidmanArr[counter] = Utility.ToImage((byte[])rb[5]); 
                    }
                    else 
                    {
                        BidmanArr[counter] = Utility.PNGtoBitmap(imageFilePaths[counter]);
                    }
                        
                    BoxArr[counter] = Utility.GenerateQRCode(CC_code(ShopOrderBoxArr[counter], QuantityArr[counter]));
                    CatalogueArr[counter] = rb[6].ToString();

                    counter++;
                }
            }
            pg = 0;
            MountData();
        }

        private string CC_code(string Order, string Qty)
        {
            string output;
            string Tab = "\t";
            output = Order + Tab + Views.Configuration.addressLocation[3] + "-FG" + Tab + Tab + "XX.XX.FG.FG" + Tab + Qty;
            return output;
        }

        private void MountData()
        {
            GOI.Text = GOs[pg];
            ShopOrderBox.Text = ShopOrderBoxArr[pg];
            Customer.Text = CustomerArr[pg];
            Quantity.Text = QuantityArr[pg];
            Urgency.Text = UrgencyArr[pg];
            if (string.IsNullOrEmpty(ShopOrderBoxArr[pg]))
            {
                Box.Source = null;
            }
            else
            {
                Box.Source = BoxArr[pg];
            }
            Bidman.Source = BidmanArr[pg];
            page.Text = (pg + 1).ToString() + "/" + GOs.Length.ToString();
            isLoaded = true;
        }


        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (isLoaded == true)
            {
                if (pg < GOs.Length - 1)
                {
                    pg += 1;

                    MountData();
                    Status.Content = GOI.Text + " SELECTED";
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (isLoaded == true)
            {
                if (pg != 0)
                {
                    pg -= 1;

                    MountData();
                    Status.Content = GOI.Text + " SELECTED";
                }
            }
        }


        private void Shipped_Click(object sender, RoutedEventArgs e)
        {
            if (isLoaded == true)
            {
                string command = "update [PRL123] set [Box Sent]=True where [GO_Item]='" + GOI.Text + "'";
                Utility.ExecuteNonQueryLP(command);
                Status.Content = GOI.Text + " TUBS SUCCESSFULLY SHIPPED";

                CurrentShippingWindow.PRL123_Set();
                string query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [SchedulingGroup], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [Catalogue], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted], [Notes] from [PRL123] where [BoxEarly]=True and [Box Sent]=False";
                CurrentShippingWindow.LoadGrid(query);
                CurrentShippingWindow.Current_Tab = "Ship Early";
                CurrentShippingWindow.ButtonColorChanges();
            }
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (isLoaded == true)
            {
                Status.Content = "Printed Box Early List";
                PrintTracking ss = new PrintTracking(CatalogueArr, GOs);
                ss.Show();
            }
        }

    }
}

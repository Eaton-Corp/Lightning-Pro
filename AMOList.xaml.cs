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
using System.Windows.Shapes;

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for AMOList.xaml
    /// </summary>
    public partial class AMOList : Window
    {
        string GON;
        string LineItem;

        public AMOList(string GO)
        {
            InitializeComponent();
            GON = GO.Substring(0, 10);
            LineItem = GO;

            var dataTrigger = (DataTrigger)dg.CellStyle.Triggers[1];
            dataTrigger.Value = LineItem;

            loadGrid();
        }


        private void loadGrid()
        {
            string query = "select c.[Project Number], c.[GO Item] as [GoItem], c.[Part Number], [Locator], c.[Date Required], c.[Shortage Message], c.[BOH], c.[Qty Required], [Item] from (SELECT * FROM tblMaterialStatus WHERE [GO Item] like '%" + GON + "%') AS c LEFT JOIN tblOnHand ON c.[Part Number]=tblOnHand.Item";
            DataTable dt = Utility.SearchMasterDB(query);
            dg.ItemsSource = dt.DefaultView;
        }

    }
}

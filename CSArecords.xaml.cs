using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for CSArecords.xaml
    /// </summary>
    public partial class CSArecords : Window
    {
        int selected = -1;
        
        public CSArecords()
        {
            InitializeComponent();

            loadGrid();
        }

        private void loadGrid() 
        {
            string query = "select * from [CSANamePlateRecord259P075H01]";
            DataTable dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
        }


        private void Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;
            if (gd.SelectedItem != null)
            {
                dynamic row = gd.SelectedItem;
                selected = row["ID"];

                status.Content = selected.ToString() + " Selected";
            }
        }


        private void SaveEdit() 
        {
            if (selected != -1) 
            {
                string commandStr = "update [CSANamePlateRecord259P075H01] set [Series Number]='" + seriesNumber.Text + "' where [ID]=" + selected.ToString();
                Utility.ExecuteNonQueryLP(commandStr);

                status.Content = selected.ToString() + ": Series # Saved";
                loadGrid();
            }
        }

        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            SaveEdit();
        }

        private void KeyDownClick(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveEdit();
            }
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for MassUpdateGO.xaml
    /// </summary>
    public partial class MassUpdateGO : Window
    {
        private string GO;


        public MassUpdateGO(string go)
        {
            InitializeComponent();
            GO = go;
            LoadData();
        }

        private void LoadData()
        {
            string queryPRL123 = "select [ID], [GO_Item],[AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRL123] where [GO] = '" + GO + "'";
            string queryPRL4 = "select [ID], [GO_Item], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRL4] where [GO] = '" + GO + "' and [PageNumber] = 0";
            string queryPRLCS = "select [ID], [GO_Item], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [LabelsPrinted] from [PRLCS] where [GO] = '" + GO + "' and [PageNumber] = 0";


            DataTable dtPRL123 = Utility.SearchLP(queryPRL123);
            DataTable dtPRL4 = Utility.SearchLP(queryPRL4);
            DataTable dtPRLCS = Utility.SearchLP(queryPRLCS);

            if (dtPRL123 != null)
                PRL123DataGrid.ItemsSource = dtPRL123.DefaultView;
            else
                MessageBox.Show("Failed to load data for PRL123DataGrid");

            if (dtPRL4 != null)
                PRL4DataGrid.ItemsSource = dtPRL4.DefaultView;
            else
                MessageBox.Show("Failed to load data for PRL4DataGrid");

            if (dtPRLCS != null)
                PRLCSDataGrid.ItemsSource = dtPRLCS.DefaultView;
            else
                MessageBox.Show("Failed to load data for PRLCSDataGrid");


        }


        private void OnSaveChangesClick(object sender, RoutedEventArgs e)
        {
            SaveData(PRL123DataGrid, "PRL123");
            SaveData(PRL4DataGrid, "PRL4");
            SaveData(PRLCSDataGrid, "PRLCS");
        
        }

        private void SaveData(DataGrid dataGrid, string tableName)
        {
            foreach (DataRowView rowView in dataGrid.ItemsSource)
            {
                var row = rowView.Row;
                if (row.RowState == DataRowState.Modified)
                {
                    StringBuilder queryBuilder = new StringBuilder($"UPDATE [{tableName}] SET ");
                    string id = row["ID"].ToString();
                    bool isFirst = true;
                    foreach (DataColumn col in row.Table.Columns)
                    {
                        // Assuming boolean fields are represented by checkboxes and are of type bool in the DataTable.
                        if (col.DataType == typeof(bool) && row[col, DataRowVersion.Original] != row[col, DataRowVersion.Current])
                        {
                            if (!isFirst) queryBuilder.Append(", ");
                            queryBuilder.Append("[" + col.ColumnName + "] = " + ((bool)row[col] ? "1" : "0"));

                            isFirst = false;
                        }
                    }
                    queryBuilder.Append($" WHERE [ID] = {id}");

                    // Assuming you have a method to execute the query
                    ExecuteQuery(queryBuilder.ToString());
                }
            }
        }

        private void ExecuteQuery(string query)
        {
            try
            {
                // Assuming Utility.ExecuteQuery executes the query.
                Utility.executeNonQueryLP(query);
            }
            catch
            {
                MessageBox.Show("An error occurred while updating the database.");
            }
        }
    }

}

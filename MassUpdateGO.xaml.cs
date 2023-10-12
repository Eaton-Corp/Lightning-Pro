using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for MassUpdateGO.xaml
    /// </summary>
    public partial class MassUpdateGO : Window
    {
        private readonly string GO;
        readonly Utility.ProductGroup CurrProd;

        public MassUpdateGO(string go, Utility.ProductGroup CurrentProduct)
        {
            InitializeComponent();
            GO = go;
            CurrProd = CurrentProduct;
            LoadData();
        }


        private void LoadData()
        {
            string queryPRL123 = "select [ID], [GO_Item], [Notes], [ProductSpecialist], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short], [LabelsPrinted], [NameplateRequired], [NameplateOrdered] from [PRL123] where [GO] = '" + GO + "'";
            string queryPRL4 = "select [ID], [GO_Item], [Notes], [ProductSpecialist], [AMO], [BoxEarly], [BoxSent], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short], [LabelsPrinted], [NameplateRequired], [NameplateOrdered] from [PRL4] where [GO] = '" + GO + "' and [PageNumber] = 0";
            string queryPRLCS = "select [ID], [GO_Item], [Notes], [ProductSpecialist], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short], [LabelsPrinted], [NameplateRequired], [NameplateOrdered] from [PRLCS] where [GO] = '" + GO + "' and [PageNumber] = 0";


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


            switch (CurrProd)
            {
                case Utility.ProductGroup.PRL4:
                    TabItemControl.SelectedIndex = 1;
                    break;
                case Utility.ProductGroup.PRLCS:
                    TabItemControl.SelectedIndex = 2;
                    break;
                default:            //Utility.ProductGroup.PRL123
                    TabItemControl.SelectedIndex = 0;
                    break;
            }
        }



        private void OnSaveChangesClick(object sender, RoutedEventArgs e)
        {
            SaveData(PRL123DataGrid, "PRL123");
            SaveData(PRL4DataGrid, "PRL4");
            SaveData(PRLCSDataGrid, "PRLCS");

            StatusTextBlock.Text = $"{GO} Updated";
        }



        private void SaveData(DataGrid dataGrid, string tableName)
        {
            foreach (DataRowView rowView in dataGrid.ItemsSource)
            {
                var row = rowView.Row;
                Boolean ExecuteQueryCheck = false;
                if (row.RowState == DataRowState.Modified)
                {
                    StringBuilder queryBuilder = new StringBuilder($"UPDATE [{tableName}] SET ");
                    string GOI = row["GO_Item"].ToString();
                    bool isFirst = true;
                    foreach (DataColumn col in row.Table.Columns)
                    {
                        if (col.ColumnName != "ID" && row[col, DataRowVersion.Original] != row[col, DataRowVersion.Current])
                        {
                            if (!isFirst) queryBuilder.Append(", ");
                            if (col.DataType == typeof(bool))
                            {
                                queryBuilder.Append("[" + col.ColumnName + "] = " + ((bool)row[col] ? "1" : "0"));
                            }
                            else
                            {
                                queryBuilder.Append("[" + col.ColumnName + "] = '" + row[col].ToString().Replace("'", "''") + "'");
                            }
                            isFirst = false;
                            ExecuteQueryCheck = true; //a valid query will be passed for sure
                        }
                    }
                    queryBuilder.Append($" WHERE [GO_Item] = '{GOI}'");


                    if (ExecuteQueryCheck) 
                    {
                        ExecuteQuery(queryBuilder.ToString());
                    }
                }
            }
        }



        private void OnCreateNoteClick(object sender, RoutedEventArgs e)
        {
            string note = NotesTextBox.Text.Replace("'", "''"); // Escape single quotes
            string updateQuery = $"UPDATE [PRL123] SET [Notes] = '{note}' WHERE [GO] = '{GO}'";
            ExecuteQuery(updateQuery);

            updateQuery = $"UPDATE [PRL4] SET [Notes] = '{note}' WHERE [GO] = '{GO}'";
            ExecuteQuery(updateQuery);

            updateQuery = $"UPDATE [PRLCS] SET [Notes] = '{note}' WHERE [GO] = '{GO}'";
            ExecuteQuery(updateQuery);

            // Reload data to reflect changes
            LoadData();

            StatusTextBlock.Text = $"Notes For All {GO} Updated";
        }




        private void ExecuteQuery(string query)
        {
            try
            {
                // Assuming Utility.ExecuteQuery executes the query.
                Utility.ExecuteNonQueryLP(query);
            }
            catch
            {
                MessageBox.Show("An error occurred while updating the database.");
            }
        }


    }
}

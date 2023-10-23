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
            string queryPRL123 = "select Distinct PRL123.[ID], PRL123.[GO_Item], PRL123.[Notes], CSALabel.[ProductID], PRL123.[ProductSpecialist], PRL123.[AMO], PRL123.[BoxEarly], PRL123.[Box Sent], PRL123.[SpecialCustomer], PRL123.[ServiceEntrance], PRL123.[DoubleSection], PRL123.[PaintedBox], PRL123.[RatedNeutral200], PRL123.[DNSB], PRL123.[Complete], PRL123.[Short], PRL123.[LabelsPrinted], PRL123.[NameplateRequired], PRL123.[NameplateOrdered] from [PRL123] inner join [CSALabel] on PRL123.[GO_Item] = CSALabel.[GO_Item] where PRL123.[GO] = '" + GO + "'";
            string queryPRL4 = "select Distinct PRL4.[ID], PRL4.[GO_Item], PRL4.[Notes], CSALabel.[ProductID], PRL4.[ProductSpecialist], PRL4.[AMO], PRL4.[BoxEarly], PRL4.[BoxSent], PRL4.[SpecialCustomer], PRL4.[ServiceEntrance], PRL4.[PaintedBox], PRL4.[RatedNeutral200], PRL4.[DoorOverDist], PRL4.[DoorInDoor], PRL4.[DNSB], PRL4.[Complete], PRL4.[Short], PRL4.[LabelsPrinted], PRL4.[NameplateRequired], PRL4.[NameplateOrdered] from [PRL4] inner join [CSALabel] on PRL4.[GO_Item] = CSALabel.[GO_Item] where PRL4.[GO] = '" + GO + "' and PRL4.[PageNumber] = 0";
            string queryPRLCS = "select Distinct PRLCS.[ID], PRLCS.[GO_Item], PRLCS.[Notes], CSALabelPRLCS.[ProductID], PRLCS.[ProductSpecialist], PRLCS.[AMO], PRLCS.[SpecialCustomer], PRLCS.[IncLocLeft], PRLCS.[IncLocRight], PRLCS.[CrossBus], PRLCS.[OpenBottom], PRLCS.[ExtendedTop], PRLCS.[PaintedBox], PRLCS.[ThirtyDeepEnclosure], PRLCS.[DNSB], PRLCS.[Complete], PRLCS.[Short], PRLCS.[LabelsPrinted], PRLCS.[NameplateRequired], PRLCS.[NameplateOrdered] from [PRLCS] inner join [CSALabelPRLCS] on PRLCS.[GO_Item] = CSALabelPRLCS.[GO_Item] where PRLCS.[GO] = '" + GO + "' and PRLCS.[PageNumber] = 0";


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
                        if ((col.ColumnName != "ID" && col.ColumnName != "ProductID") && row[col, DataRowVersion.Original] != row[col, DataRowVersion.Current])
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

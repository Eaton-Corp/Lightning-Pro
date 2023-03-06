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

namespace PRL123_Final
{
    /// <summary>
    /// Interaction logic for CSArecords.xaml
    /// </summary>
    public partial class CSArecords : Window
    {
        public CSArecords()
        {
            InitializeComponent();

            string query = "select * from [CSANamePlateRecord259P075H01]";
            DataTable dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;

            //loadGrid3();
        }


        //private void loadGrid3() 
        //{
        //    DataSet ds = new DataSet();
        //    OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [CSANamePlateRecord259P075H01]", MainWindow.LPcon);
        //    adapter.Fill(ds, "CSANamePlateRecord259P075H01");

        //}



        //public CustomerDataProvider()
        //{
        //    NorthwindDataSet dataset = new NorthwindDataSet();

        //    adapter = new CustomersTableAdapter();
        //    adapter.Fill(dataset.Customers);

        //    dataset.Customers.CustomersRowChanged +=
        //        new NorthwindDataSet.CustomersRowChangeEventHandler(CustomersRowModified);
        //    dataset.Customers.CustomersRowDeleted +=
        //        new NorthwindDataSet.CustomersRowChangeEventHandler(CustomersRowModified);
        //}

        //void CustomersRowModified(object sender, NorthwindDataSet.CustomersRowChangeEvent e)
        //{
        //    adapter.Update(dataset.Customers);
        //}






        //private void loadGrid()
        //{
        //    string query = "select * from [CSANamePlateRecord259P075H01]";
        //    DataTable dt = Utility.SearchLP(query);
        //    dg.ItemsSource = dt.DefaultView;
        //}


        ////string query = "select * from [CSANamePlateRecord259P075H01]";
        //DataTable dt = Utility.SearchLP("select * from [CSANamePlateRecord259P075H01]");

        //OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [CSANamePlateRecord259P075H01]", MainWindow.LPcon);
        //DataSet ds = new DataSet();
        //private void loadGrid2()
        //{
            
        //    //DataSet ds = new DataSet();
        //    adapter.Fill(ds, "myPath");
        //    dg.DataContext = ds;

           


        //    //OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
        //    //adapter.UpdateCommand = builder.GetUpdateCommand();
        //    //adapter.Update(ds);

        //}


        //private void Click_Save(object sender, RoutedEventArgs e)
        //{
           
        //    OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
        //    adapter.UpdateCommand = builder.GetUpdateCommand();
        //    adapter.Update(ds, "dt");
        //}





        ////public CustomerDataProvider()
        ////{
        ////    NorthwindDataSet dataset = new NorthwindDataSet();

        ////    adapter = new CustomersTableAdapter();
        ////    adapter.Fill(dataset.Customers);

        ////    dataset.Customers.CustomersRowChanged +=
        ////        new NorthwindDataSet.CustomersRowChangeEventHandler(CustomersRowModified);
        ////    dataset.Customers.CustomersRowDeleted +=
        ////        new NorthwindDataSet.CustomersRowChangeEventHandler(CustomersRowModified);
        ////}

        ////void CustomersRowModified(object sender, NorthwindDataSet.CustomersRowChangeEvent e)
        ////{
        ////    adapter.Update(dataset.Customers);
        ////}


    }
}

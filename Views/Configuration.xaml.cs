using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
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
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        
        //access these fields globally in the app
        public static string[] addressLocation = ConfigurationManager.ConnectionStrings["locationAddress"].ToString().Split('/');          //each index has an address line for TEST Report and last index has location abbreviation 
        public static string[] specialCustomer = ConfigurationManager.ConnectionStrings["specialCustomersList"].ToString().Split('/');         //each index has a special customer - or index[0] has "NONE"


        //product names as they appear under [Prod Group] in [tblOrderStatus] 
        public static string[] PRL123names;
        public static string[] PRL4names;
        public static string[] PRLCSnames;
        public static string[] ECnames;

        public static void SetProductNames() 
        { 
            string[] productlines = ConfigurationManager.ConnectionStrings["productNameList"].ToString().Split('/');
            PRL123names = productlines[0].Split(',');
            PRL4names = productlines[1].Split(',');
            PRLCSnames = productlines[2].Split(',');
            ECnames = productlines[3].Split(',');
        }


        public Configuration()
        {
            InitializeComponent();
            populateLabels();
            Status.Visibility = Visibility.Hidden;
        }

        private void upload_txtClicked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
            ofg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            bool? response = ofg.ShowDialog();

            if (response == true)           //if the user selects a file and clicks OK
            {
                string filepath = ofg.FileName;
                string[] lines = System.IO.File.ReadAllLines(filepath);
                File.Copy(filepath, MainWindow.localConfigFilePath, true);

                string locationSatellite = lines[0];
                string masterDatabase = lines[1];
                string lightningProDatabase = lines[2];
                string locationOrderFiles = lines[3];
                string locationImagesFolder = lines[4];
                string locationReleaseFolder = lines[5];
                string addressLine = lines[6];
                string productNamesLine = lines[7];
                string specialCustomersLine = lines[8];

                MainWindow.UpdateConnectionStrings("Location", locationSatellite);
                MainWindow.UpdateConnectionStrings("MasterDB", masterDatabase);
                MainWindow.UpdateConnectionStrings("LPdatabase", lightningProDatabase);
                MainWindow.UpdateConnectionStrings("orderFiles", locationOrderFiles);
                MainWindow.UpdateConnectionStrings("imagesFolder", locationImagesFolder);
                MainWindow.UpdateConnectionStrings("releaseFolder", locationReleaseFolder);
                MainWindow.UpdateConnectionStrings("locationAddress", addressLine);
                MainWindow.UpdateConnectionStrings("productNameList", productNamesLine);
                MainWindow.UpdateConnectionStrings("specialCustomersList", specialCustomersLine);
                Status.Visibility = Visibility.Visible;
                MessageBox.Show("Please restart the application to allow for the updates to take place.", "Successfully Configured", MessageBoxButton.OK, MessageBoxImage.Information);
                Status.Visibility = Visibility.Hidden;
            }
        }

      
        private void populateLabels() 
        {
            Location.Content = ConfigurationManager.ConnectionStrings["Location"].ToString();
            MasterDB.Text = ConfigurationManager.ConnectionStrings["MasterDB"].ToString();
            LPdatabase.Text = ConfigurationManager.ConnectionStrings["LPdatabase"].ToString();
            orderFiles.Content = ConfigurationManager.ConnectionStrings["orderFiles"].ToString();
            imagesFolder.Content = ConfigurationManager.ConnectionStrings["imagesFolder"].ToString();
            releaseFolder.Content = ConfigurationManager.ConnectionStrings["releaseFolder"].ToString();
            address.Text = ConfigurationManager.ConnectionStrings["locationAddress"].ToString();
            productNames.Content = ConfigurationManager.ConnectionStrings["productNameList"].ToString();
            specialCustomers.Content = ConfigurationManager.ConnectionStrings["specialCustomersList"].ToString();
        }

    }
}

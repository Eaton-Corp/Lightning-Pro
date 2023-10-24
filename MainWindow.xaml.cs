using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
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
using LightningPRO.ViewModels;
using Squirrel;
using static LightningPRO.Utility;
using LightningPRO.Models;
using LinqToDB.Data;
using LinqToDB;

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly bool isConfigured;
        public static string localConfigFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "..\\Config.txt";

        //single instance of connection to Lightning Pro Database
        public static OleDbConnection LPcon = new OleDbConnection(ConfigurationManager.ConnectionStrings["LPdatabase"].ToString());

        //single instance of connection to Master Database
        public static OleDbConnection Mcon = new OleDbConnection(ConfigurationManager.ConnectionStrings["MasterDB"].ToString());

        public static Utility.ProductGroup ProductGroup { get; set; } = Utility.ProductGroup.PRL123;

        private void ConnectLPdatabase()
        {
            LPcon.Open();
            OleDbCommand cmd = new OleDbCommand("SELECT * FROM Z_DummyTable", LPcon);
            cmd.ExecuteReader();
        }

        private void ConnectMasterDatabase()
        {
            Mcon.Open();
            OleDbCommand cmd = new OleDbCommand("SELECT * FROM tblDepartments", Mcon);
            cmd.ExecuteReader();
        }


        public MainWindow()
        {
            InitializeComponent();

            //Command to delete the automatic configuration file on local computer - Normally should be commented out
            //if (File.Exists(localConfigFilePath)) File.Delete(localConfigFilePath);

            isConfigured = CheckConfigured();

            if (isConfigured)
            {
                ConfigurationStorage.SetProductNames();

                ConnectLPdatabase();
                ConnectMasterDatabase();
            }
        }

        private void ContentLoaded(object sender, EventArgs e)
        {
            if (!isConfigured)
            {
                if (File.Exists(localConfigFilePath))
                {
                    AutomaticConfiguration();
                }
                else
                {
                    DataContext = new ConfigurationViewModel();
                    MessageBox.Show("Please upload the provided configuration file for your location.", "Configuration Required", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            if (isConfigured)
            {
                DataContext = new EatonViewModel();
            }
        }

        private bool CheckConfigured()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["Location"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["MasterDB"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["LPdatabase"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["orderFiles"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["imagesFolder"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["releaseFolder"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["locationAddress"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["productNameList"].ToString()) || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["specialCustomersList"].ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AutomaticConfiguration()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(localConfigFilePath);
                string locationSatellite = lines[0];
                string masterDatabase = lines[1];
                string lightningProDatabase = lines[2];
                string locationOrderFiles = lines[3];
                string locationImagesFolder = lines[4];
                string locationReleaseFolder = lines[5];
                string addressLine = lines[6];
                string productNamesLine = lines[7];
                string specialCustomersLine = lines[8];

                UpdateConnectionStrings("Location", locationSatellite);
                UpdateConnectionStrings("MasterDB", masterDatabase);
                UpdateConnectionStrings("LPdatabase", lightningProDatabase);
                UpdateConnectionStrings("orderFiles", locationOrderFiles);
                UpdateConnectionStrings("imagesFolder", locationImagesFolder);
                UpdateConnectionStrings("releaseFolder", locationReleaseFolder);
                UpdateConnectionStrings("locationAddress", addressLine);
                UpdateConnectionStrings("productNameList", productNamesLine);
                UpdateConnectionStrings("specialCustomersList", specialCustomersLine);

                MessageBox.Show("Please restart the application to allow for the updates to take place.", "Successfully Configured", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed trying to automatically configure application using Config.txt saved locally.", "Unable to Complete Automatic Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Eaton_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new EatonViewModel();
            }
        }


        private void Specialist_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new SpecialistViewModel();
            }
        }

        private void Supervisor_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new SupervisorViewModel();
            }
        }

        private void MaterialPlanning_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                //DataContext = new MaterialPlanningViewModel();
            }
        }


        private void Assembler_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new AssemblerViewModel();
            }
        }


        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new ShippingViewModel();
            }
        }


        private void Analytics_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new AnalyticsViewModel();
            }
        }


        private void Settings_Clicked(object sender, RoutedEventArgs e)
        {
            if (isConfigured)
            {
                DataContext = new SettingsViewModel();
            }
        }

        private void Configuration_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new ConfigurationViewModel();
        }

        private void Credits_Clicked(object sender, RoutedEventArgs e)
        {
            DataContext = new CreditsViewModel();
        }



        public static void UpdateConnectionStrings(string name, string connectionString)
        {
            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + "LightningPRO.exe.config"
                };
                Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var settings = configFile.ConnectionStrings.ConnectionStrings;
                settings[name].ConnectionString = connectionString;
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.ConnectionStrings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error Updating ConnectionStrings");
            }
        }



        //Uncomment this code for Releases

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*
             if (isConfigured)
             {
                 string releaseFolder = ConfigurationManager.ConnectionStrings["releaseFolder"].ToString();
                 using (var updateManager = new UpdateManager(releaseFolder))
                 {
                     CurrentVersion.Text = $"Current version: {updateManager.CurrentlyInstalledVersion()}";
                     var releaseEntry = await updateManager.UpdateApp();
                     NewVersion.Text = $"Update Version: {releaseEntry?.Version.ToString() ?? " "}";
                 }
             }
            */
        }


    }

}
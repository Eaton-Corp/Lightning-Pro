using System;
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
using System.Collections.ObjectModel;


namespace LightningPRO
{
    public partial class ModernDataGrid : UserControl
    {
        public static readonly DependencyProperty DataItemsProperty = DependencyProperty.Register(
            "DataItems", typeof(ObservableCollection<FakeEntry>), typeof(ModernDataGrid), new PropertyMetadata(null));

        public ObservableCollection<FakeEntry> DataItems
        {
            get { return (ObservableCollection<FakeEntry>)GetValue(DataItemsProperty); }
            set { SetValue(DataItemsProperty, value); }
        }

        public ModernDataGrid()
        {
            InitializeComponent();
            // Initialize the DataItems with fake data
            DataItems = GetFakeData();
            // Set the DataContext for data binding
            this.DataContext = this;
        }

        private ObservableCollection<FakeEntry> GetFakeData()
        {
            return new ObservableCollection<FakeEntry>
            {
                new FakeEntry { Name = "Test User 1", Email = "testuser1@example.com", Mobile = "555-0100", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 2", Email = "testuser2@example.com", Mobile = "555-0101", Role = "User", Status = "Inactive" },
                new FakeEntry { Name = "Test User 3", Email = "testuser3@example.com", Mobile = "555-0102", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 4", Email = "testuser4@example.com", Mobile = "555-0103", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 5", Email = "testuser5@example.com", Mobile = "555-0104", Role = "User", Status = "Inactive" },
                new FakeEntry { Name = "Test User 6", Email = "testuser6@example.com", Mobile = "555-0105", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 7", Email = "testuser7@example.com", Mobile = "555-0106", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 8", Email = "testuser8@example.com", Mobile = "555-0107", Role = "User", Status = "Inactive" },
                new FakeEntry { Name = "Test User 9", Email = "testuser9@example.com", Mobile = "555-0108", Role = "User", Status = "Active" },
                new FakeEntry { Name = "Test User 10", Email = "testuser10@example.com", Mobile = "555-0109", Role = "User", Status = "Active" }
            };
        }

    }

    // Define the PartItem class if not already defined
    public class FakeEntry
    {
        // Properties corresponding to the data you want to display
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        // Include any additional properties as needed
    }
}


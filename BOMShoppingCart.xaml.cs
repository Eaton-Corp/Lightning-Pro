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
    public partial class BOMShoppingCart : UserControl
    {
        public ObservableCollection<PartItem> PartItems { get; set; }

        public ObservableCollection<AlternatePartItem> AlternatePartItems { get; set; }


        public BOMShoppingCart()
        {
            InitializeComponent();
            PartItems = new ObservableCollection<PartItem>
            {
                new PartItem { PartName = "Part1", Quantity = 10, Locator = "A1" },
                new PartItem { PartName = "Part2", Quantity = 5, Locator = "B2" },
                new PartItem { PartName = "Part3", Quantity = 7, Locator = "C3" },
                // Add more items as needed
            };

            AlternatePartItems = new ObservableCollection<AlternatePartItem>
        {
            new AlternatePartItem { GOItem = "Item1", TimesUsed = 5, DateCreated = DateTime.Now.AddDays(-10) },
            new AlternatePartItem { GOItem = "Item2", TimesUsed = 3, DateCreated = DateTime.Now.AddDays(-20) },
            new AlternatePartItem { GOItem = "Item3", TimesUsed = 8, DateCreated = DateTime.Now.AddDays(-30) },
            // Add more fake items as needed
        };

            // Bind the ObservableCollection to the AlternateDataGrid
            string imagePath = "Assets/check.png"; // Replace with the actual path to your image

            myEatonDataGrid.AddButtonColumn("Action", "Delete", DeleteButton_Click, Colors.Red, 75, 20);
            myEatonDataGrid.AddButtonColumn("Action", "View", ViewButton_Click, Colors.Orange, 75, 20);
            myEatonDataGrid.AddButtonColumn("Action", "View", ViewButton_Click, Colors.Blue, 75, 20);



            AlternateDataGrid.ItemsSource = AlternatePartItems;

            // Bind the ObservableCollection to the DataGrid
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click event
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                FakeEntry dataItem = clickedButton.DataContext as FakeEntry;
                if (dataItem != null)
                {
                    // Perform actions using dataItem, e.g., show a message
                    MessageBox.Show($"Button clicked for: {dataItem.Name}");
                }
            }
        }


        private void AddToBOMButton_Click(object sender, RoutedEventArgs e)
        {
            if (PartsTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                string partName = selectedItem.Header.ToString();
                string locator = GenerateRandomLocator();

                PartItem newItem = new PartItem { PartName = partName, Quantity = 1, Locator = locator };
                PartItems.Add(newItem);
            }
        }

        private string GenerateRandomLocator()
        {
            Random random = new Random();
            return $"{random.Next(10):00}.{random.Next(1000000000):000000000}.{random.Next(10):0}X";
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
                return;

            SelectTreeViewItem(PartsTreeView, searchText);
        }

        private void SelectTreeViewItem(ItemsControl parent, string searchText)
        {
            foreach (var item in parent.Items)
            {
                TreeViewItem treeItem = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem == null)
                    continue;

                if (treeItem.Header.ToString().ToLower().Contains(searchText))
                {
                    treeItem.IsSelected = true;
                    treeItem.BringIntoView();
                    return;
                }

                if (treeItem.Items.Count > 0)
                {
                    SelectTreeViewItem(treeItem, searchText);  // Recursive call
                }
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (myEatonDataGrid.SelectedItem is PartItem selectedPart)
            {
                string message = $"Part Name: {selectedPart.PartName}\nQuantity: {selectedPart.Quantity}\nLocator: {selectedPart.Locator}";
                MessageBox.Show(message, "Part Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (myEatonDataGrid.SelectedItem is PartItem selectedPart)
            {
                PartItems.Remove(selectedPart);
            }
        }

        private void ViewComprehensiveBOM_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            AlternateBOMView.Visibility = Visibility.Collapsed;
            AlternateBomBreakdown.Visibility = Visibility.Visible;
        }

        private void AlertButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            AlternateBOMView.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlternateBOMView.Visibility == Visibility.Visible)
            {
                AlternateBOMView.Visibility = Visibility.Collapsed;
                MainView.Visibility = Visibility.Visible;
            }
            else if (AlternateBomBreakdown.Visibility == Visibility.Visible)
            {
                AlternateBomBreakdown.Visibility = Visibility.Collapsed;
                AlternateBOMView.Visibility = Visibility.Visible;
            }
            
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            AlternateBOMView.Visibility = Visibility.Collapsed;
            AlternateBOMBreakdown.Visibility = Visibility.Collapsed;
        }

    }

    // Define a simple part item model
    public class PartItem
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public string Locator { get; set; }
    }

    public class AlternatePartItem
    {
        public string GOItem { get; set; }
        public int TimesUsed { get; set; }
        public DateTime DateCreated { get; set; }
    }

}

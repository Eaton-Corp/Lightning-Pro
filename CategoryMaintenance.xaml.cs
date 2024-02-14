using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace LightningPRO
{
    /// <summary>
    /// Interaction logic for CategoryMaintenance.xaml
    /// </summary>
    public partial class CategoryMaintenance : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<PartItem> PartItems { get; set; }



        public ObservableCollection<AlternatePartItem> AlternatePartItems { get; set; }

        private Stack<CatalogItem> navigationStack = new Stack<CatalogItem>();
        public ObservableCollection<CatalogItem> CurrentItems { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<CatalogItem> rootCatalogItems { get; set; }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Update your properties to call OnPropertyChanged
        private ObservableCollection<string> _navigationPath;
        public ObservableCollection<string> NavigationPath
        {
            get => _navigationPath;
            set
            {
                _navigationPath = value;
                OnPropertyChanged(nameof(NavigationPath));
            }
        }

        public CategoryMaintenance()
        {
            InitializeComponent();
            
            //LoadComboBoxes();
            //InitializeCatalog();
            NavigationPath = new ObservableCollection<string>();
            this.DataContext = this;
        }

        private void CategorySets_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateComboBoxes();
            MaterialCatalogControl.CatalogMaintenanceMode();
        }


        /*private void LoadComboBoxes()
        {
            // Assuming FetchUniqueFamilyValues returns a List<string> of unique values for the specified column
            Family1ComboBox.ItemsSource = FetchUniqueFamilyValues("FAMILY1");
            Family2ComboBox.ItemsSource = FetchUniqueFamilyValues("FAMILY2");
            Family3ComboBox.ItemsSource = FetchUniqueFamilyValues("FAMILY3");
        }*/

        /*private List<string> FetchUniqueFamilyValues(string familyColumn)
        {
            List<string> uniqueValues = new List<string>();
            string query = $"SELECT DISTINCT {familyColumn} FROM TBL_CATEGORY_SETS";

            DataTable dt = Utility.SearchLP(query);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    uniqueValues.Add(row[familyColumn].ToString());
                }
            }
            return uniqueValues;
        }

        private void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            string family1 = Family1ComboBox.SelectedValue?.ToString();
            string family2 = Family2ComboBox.SelectedValue?.ToString();
            string family3 = Family3ComboBox.SelectedValue?.ToString();
            FetchAndDisplayCatalogItems(family1, family2, family3);


        }*/

        private void FetchAndDisplayCatalogItems(string family1, string family2, string family3)
        {
            // Construct query with selected values
            string query = $"SELECT * FROM TBL_CATEGORY_SETS WHERE FAMILY1 = '{family1}' AND FAMILY2 = '{family2}' AND FAMILY3 = '{family3}'";
            DataTable allCategories = Utility.SearchLP(query);

            var catalogItems = ProcessCategories(allCategories);

            // Update CurrentItems and reset NavigationPath
            CurrentItems = new ObservableCollection<CatalogItem>(catalogItems);
            NavigationPath.Clear();
            NavigationPath.Add("Root");
            OnPropertyChanged(nameof(CurrentItems)); // Notify UI to update
            OnPropertyChanged(nameof(NavigationPath));
        }

        /*private List<CatalogItem> ProcessCategories(DataTable categories)
        {
            var catalogItems = new List<CatalogItem>();

            foreach (DataRow row in categories.Rows)
            {
                // Parse the row to create a CatalogItem
                var catSetName = row["CAT_SET_NAME"].ToString();
                var subCatIds = ParseSubCatIds(row["SUBCAT_IDS"].ToString());
                var setValues = ParseSetValues(row["SET_VALUES"].ToString());

                var catalogItem = new CatalogItem(catSetName)
                {
                    SubItems = FindSubItems(subCatIds, categories),
                    SetValues = setValues
                };

                if (Convert.ToBoolean(row["INITIALIZED"]))
                {
                    catalogItems.Add(catalogItem);
                }
            }

            return catalogItems;
        }*/

        private List<CatalogItem> ProcessCategories(DataTable categories)
        {
            var catalogItems = new List<CatalogItem>();

            foreach (DataRow row in categories.Rows)
            {
                var catSetName = row["CAT_SET_NAME"].ToString();
                var subCatIds = ParseSubCatIds(row["SUBCAT_IDS"].ToString());
                var setValues = ParseSetValues(row["SET_VALUES"].ToString());

                var catalogItem = new CatalogItem(catSetName)
                {
                    CatSetId = Convert.ToInt32(row["CAT_SET_ID"]),
                    Family1 = row["FAMILY1"].ToString(),
                    Family2 = row["FAMILY2"].ToString(),
                    Family3 = row["FAMILY3"].ToString(),
                    SubItems = FindSubItems(subCatIds, categories),
                    SetValues = setValues
                };

                if (Convert.ToBoolean(row["INITIALIZED"]))
                {
                    catalogItems.Add(catalogItem);
                }
            }

            return catalogItems;
        }


        private List<int> ParseSubCatIds(string subCatIdsStr)
        {
            if (string.IsNullOrEmpty(subCatIdsStr))
                return new List<int>();

            return subCatIdsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(int.Parse)
                               .ToList();
        }


        private List<string> ParseSetValues(string setValuesStr)
        {
            if (string.IsNullOrEmpty(setValuesStr))
                return new List<string>();

            return setValuesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .ToList();
        }


        private List<CatalogItem> FindSubItems(List<int> subCatIds, DataTable categories)
        {
            var subItems = new List<CatalogItem>();

            foreach (var id in subCatIds)
            {
                // Find the row with the matching CAT_SET_ID
                var matchingRows = categories.Select($"CAT_SET_ID = {id}");
                foreach (DataRow row in matchingRows)
                {
                    var catSetName = row["CAT_SET_NAME"].ToString();
                    var newSubCatIds = ParseSubCatIds(row["SUBCAT_IDS"].ToString());
                    var setValues = ParseSetValues(row["SET_VALUES"].ToString());

                    var catalogItem = new CatalogItem(catSetName)
                    {
                        SubItems = FindSubItems(newSubCatIds, categories),
                        SetValues = setValues
                    };

                    subItems.Add(catalogItem);
                }
            }

            return subItems;
        }

        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CatalogItem selected)
            {
                await Task.Delay(200); // 200 milliseconds delay
                // If the category has subcategories, navigate to them
                if (selected.SubItems.Count > 0)
                {
                    CurrentItems.Clear();
                    foreach (var item in selected.SubItems)
                    {
                        CurrentItems.Add(item);
                    }

                    navigationStack.Push(selected);
                    UpdateNavigationPath();
                }
                else if (selected.SetValues.Any())
                {
                    // If selected item has set values, display them
                    CurrentItems.Clear();
                    foreach (var setValue in selected.SetValues)
                    {
                        CurrentItems.Add(new CatalogItem(setValue));
                    }

                    navigationStack.Push(selected);
                    UpdateNavigationPath();
                }
                else
                {
                    // Handle selection of individual set value from search
                    // Optionally, directly add to BOM or prompt user
                }
            }
        }

        private void UpdateNavigationPath()
        {
            NavigationPath.Clear();
            var breadcrumbStack = navigationStack.Reverse().ToList();

            for (int i = 0; i < breadcrumbStack.Count; i++)
            {
                var item = breadcrumbStack[i];
                NavigationPath.Add(item.Name);

                if (i < breadcrumbStack.Count - 1)
                {
                    NavigationPath.Add(">");
                }
            }
        }



        public void NavigateBack(string toItem)
        {
            while (navigationStack.Count > 0 && navigationStack.Peek().Name != toItem)
            {
                navigationStack.Pop();
            }
            if (navigationStack.Count > 0)
            {
                // Re-populate the ListBox based on the new top of the stack
            }
        }

        public void BreadcrumbClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Text != ">")
            {
                NavigateToBreadcrumbItem(textBlock.Text);
            }
        }

        private void NavigateToBreadcrumbItem(string itemName)
        {
            while (navigationStack.Any() && navigationStack.Peek().Name != itemName)
            {
                navigationStack.Pop();
            }

            if (navigationStack.Any())
            {
                var currentItem = navigationStack.Peek();
                if (currentItem.SubItems.Count > 0)
                {
                    // Populate with subcategories
                    CurrentItems.Clear();
                    foreach (var item in currentItem.SubItems)
                    {
                        CurrentItems.Add(item);
                    }
                }
                else if (currentItem.SetValues.Any())
                {
                    // Populate with set values if no subcategories
                    CurrentItems.Clear();
                    foreach (var setValue in currentItem.SetValues)
                    {
                        CurrentItems.Add(new CatalogItem(setValue));
                    }
                }
            }
            else
            {
                // Handle navigation to the root level
                // You can decide what to do in this case,
                // e.g., repopulating the list with the root items or clearing it.
            }
            UpdateNavigationPath();
        }

       
        

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComboBoxes();
        }

        private void UpdateComboBoxes()
        {
            // Check the current selected values
            var selectedFamily1 = Family1ComboBox.SelectedItem?.ToString();
            var selectedFamily2 = Family2ComboBox.SelectedItem?.ToString();
            var selectedFamily3 = Family3ComboBox.SelectedItem?.ToString();

            // Update the items for Family1ComboBox
            Family1ComboBox.ItemsSource = GetFilteredColumnValues("FAMILY1", "FAMILY2", selectedFamily2, "FAMILY3", selectedFamily3);

            // Update the items for Family2ComboBox
            Family2ComboBox.ItemsSource = GetFilteredColumnValues("FAMILY2", "FAMILY1", selectedFamily1, "FAMILY3", selectedFamily3);

            // Update the items for Family3ComboBox
            Family3ComboBox.ItemsSource = GetFilteredColumnValues("FAMILY3", "FAMILY1", selectedFamily1, "FAMILY2", selectedFamily2);

            // Restore the selected items if they are still valid
            Family1ComboBox.SelectedItem = selectedFamily1;
            Family2ComboBox.SelectedItem = selectedFamily2;
            Family3ComboBox.SelectedItem = selectedFamily3;
        }

        private IList<string> GetFilteredColumnValues(string targetColumn, string filterColumn1, string filterValue1, string filterColumn2, string filterValue2)
        {
            // Construct the query with the necessary WHERE clauses based on filter values
            var whereClauses = new List<string>();
            if (!string.IsNullOrEmpty(filterValue1))
            {
                whereClauses.Add($"[{filterColumn1}] = '{filterValue1}'");
            }
            if (!string.IsNullOrEmpty(filterValue2))
            {
                whereClauses.Add($"[{filterColumn2}] = '{filterValue2}'");
            }

            string whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            string query = $"SELECT DISTINCT [{targetColumn}] FROM [TBL_CATEGORY_SETS] {whereClause} ORDER BY [{targetColumn}]";
            DataTable dt = Utility.SearchLP(query);

            if (dt != null)
            {
                return dt.AsEnumerable().Select(row => row[targetColumn].ToString()).ToList();
            }
            else
            {
                return new List<string>();
            }
        }


        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFamilyComboBoxes();
        }

        private void ClearFamilyComboBoxes()
        {
            Family1ComboBox.SelectedItem = null;
            Family2ComboBox.SelectedItem = null;
            Family3ComboBox.SelectedItem = null;

            Family1ComboBox.Text = string.Empty;
            Family2ComboBox.Text = string.Empty;
            Family3ComboBox.Text = string.Empty;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            
        }


        private int GetLatestCategorySetId()
        {
            string query = "SELECT MAX(CAT_SET_ID) AS LatestID FROM TBL_CATEGORY_SETS";
            DataTable dt = Utility.SearchLP(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0]["LatestID"]);
            }
            return -1; // Indicate an error or that no ID was found
        }

        private string GetUpdatedSubCatIds(int parentCatSetId, int newSubCatId)
        {
            string query = $"SELECT SUBCAT_IDS FROM TBL_CATEGORY_SETS WHERE CAT_SET_ID = {parentCatSetId}";
            DataTable dt = Utility.SearchLP(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                string existingSubCatIds = dt.Rows[0]["SUBCAT_IDS"].ToString();
                return existingSubCatIds.Length > 0 ? $"{existingSubCatIds},{newSubCatId}" : newSubCatId.ToString();
            }
            return newSubCatId.ToString(); // Return only the new ID if no existing subcategories were found
        }

        
    }
}

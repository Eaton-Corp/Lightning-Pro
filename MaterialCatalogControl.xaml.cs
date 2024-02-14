using Aspose.CAD.FileFormats.Ifc.IFC4.Entities;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
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
    /// Interaction logic for MaterialCatalogControl.xaml
    /// </summary>
    public partial class MaterialCatalogControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<PartItem> PartItems { get; set; }

        public ObservableCollection<PartItem> SetValueItems { get; set; }

        public ObservableCollection<PartItem> ReplacementItems { get; set; }

        public void setReplacementItems(ObservableCollection<PartItem> items) {
            this.ReplacementItems = items;
            myEatonDataGrid.Items.Refresh();
        }


        private bool _isDeleteButtonVisible = false; // Default to hidden
        public bool IsDeleteButtonVisible
        {
            get => _isDeleteButtonVisible;
            set
            {
                _isDeleteButtonVisible = value;
                OnPropertyChanged(nameof(IsDeleteButtonVisible));
            }
        }


        private Visibility _deleteButtonVisibility = Visibility.Collapsed;

        public Visibility DeleteButtonVisibility
        {
            get { return _deleteButtonVisibility; }
            set
            {
                _deleteButtonVisibility = value;
                OnPropertyChanged(nameof(DeleteButtonVisibility)); // Notify the UI about the change
            }
        }

        public void ToggleDeleteButtonVisibility(bool isVisible)
        {
            DeleteButtonVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private Stack<CatalogItem> navigationStack = new Stack<CatalogItem>();

        public ObservableCollection<CatalogItem> LastSearchResults { get; set; }

        public ObservableCollection<PartItem> LastSearchPartItems { get; set; }

        public ObservableCollection<CatalogItem> CurrentItems { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<CatalogItem> rootCatalogItems { get; set; }

        private CatalogItem selectedItemForReplacement;


        private bool _isReplaceMode;
        public bool IsReplaceMode
        {
            get => _isReplaceMode;
            set
            {
                _isReplaceMode = value;
                OnPropertyChanged(nameof(IsReplaceMode));
            }
        }

        public event EventHandler ExternalCancelReplaceButtonClickHandler;
        public event EventHandler ExternalFinishReplaceButtonClickHandler;


        private string _currentCategoryName;

        public string CurrentCategoryName
        {
            get => _currentCategoryName;
            set
            {
                _currentCategoryName = value;
                OnPropertyChanged(nameof(CurrentCategoryName));
            }
        }
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

        
        
        public MaterialCatalogControl()
        {
            InitializeComponent();
            InitializeCatalog();
            NavigationPath = new ObservableCollection<string>();
            this.DataContext = this;
            SetValueItems = new ObservableCollection<PartItem>
            {
                // Add more items as needed --> Used for testing and building out UI
            };
            PartItems = new ObservableCollection<PartItem>
            {
                // Add more items as needed --> Used for testing
            };
            var quantityOptions = Enumerable.Range(1, 1000).ToList(); // Range from 1 to 1000
            LastSearchResults = new ObservableCollection<CatalogItem>();
            LastSearchPartItems = new ObservableCollection<PartItem>();

            SetValuesDataGrid.ItemsSource = SetValueItems;
            InitializeShoppingCartWithParts();
            this.DataContext = this;
            UpdateNavigationPath();

            ReplacementItems =  new ObservableCollection<PartItem>
            {
                new PartItem { PartNumber = "part1", Quantity = 1, Locator = "A1", Description = "Description for part1" },
                new PartItem { PartNumber = "part2", Quantity = 1, Locator = "A1", Description = "Description for part2" },
            };


        }

        public void setCartContent(string content) {
            ViewCart.Content = content;
        }


        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CatalogItem catalogItem)
            {
                string catSetName = catalogItem.Name; // Retrieve the CAT_SET_NAME from the clicked DataGrid item
                bool newInitializedValue = catalogItem.Initialized; // Toggle the initialized state

                try
                {
                    // SQL command to update the category as a favorite
                    string sqlUpdateCommand = $"UPDATE TBL_CATEGORY_SETS SET INITIALIZED = true WHERE CAT_SET_NAME = '{catSetName}'";
                    if (newInitializedValue == true) {
                        sqlUpdateCommand = $"UPDATE TBL_CATEGORY_SETS SET INITIALIZED = false WHERE CAT_SET_NAME = '{catSetName}'";
                    }
                    // Execute the update operation
                    Utility.ExecuteNonQueryLP(sqlUpdateCommand);
                    if (newInitializedValue == true)
                    {
                        // Show a success message
                        MessageBox.Show($"Category '{catSetName}' has been unmarked as favorite.");
                    }
                    else {
                        // Show a success message
                        MessageBox.Show($"Category '{catSetName}' has been marked as favorite.");
                    }
                 

                    // Refresh the UI if needed
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occurred while updating the category: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Could not identify the category to update as favorite.");
            }
        }

        public void AddComboBoxColumnToDataGrid(string header, string bindingProperty, List<int> itemsSource, string displayMemberPath, string selectedValuePath, int width, SelectionChangedEventHandler selectionChangedEventHandler)
        {
            SetValuesDataGrid.AddComboBoxColumn(header, bindingProperty, itemsSource, displayMemberPath, selectedValuePath, width, selectionChangedEventHandler);
        }

        public void AddButtonToDataGrid(string header, string buttonText, RoutedEventHandler handler, Color color, double width, double height)
        {
            SetValuesDataGrid.AddButtonColumn(header, buttonText, handler, color, width, height);
        }

        public void RemoveButtonFromDataGrid(string header)
        {
            SetValuesDataGrid.RemoveButtonColumn(header);
        }

        public void ChangeBackgroundColor(Color color)
        {
            this.Background = new SolidColorBrush(color);
        }

        public void SetCancelReplaceButtonVisibility(Visibility visibility)
        {
            CancelReplaceButton.Visibility = visibility;
        }

        public void SetCartButtonVisibility(Visibility visibility)
        {
            ViewCart.Visibility = visibility;
        }

        public void SetFinishReplaceButtonVisibility(Visibility visibility)
        {
            FinishReplaceButton.Visibility = visibility;
        }

        public void ChangeBackgroundColorToDefault()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);
            MaterialCatalogGroupBox.Header = "Material Catalog";
        }

        private void CancelReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExternalCancelReplaceButtonClickHandler != null)
            {
                ExternalCancelReplaceButtonClickHandler?.Invoke(this, EventArgs.Empty);
            }

            IsReplaceMode = false;

            // Use MaterialCatalogControl's methods to update UI
            ChangeBackgroundColorToDefault();
            SetCancelReplaceButtonVisibility(Visibility.Collapsed);
            SetFinishReplaceButtonVisibility(Visibility.Collapsed);
            SetCartButtonVisibility(Visibility.Collapsed);
            SetValuesDataGrid.RemoveButtonColumn("Replace");
        }

        private void FinishReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExternalCancelReplaceButtonClickHandler != null)
            {
                ExternalFinishReplaceButtonClickHandler.Invoke(sender, e);
            }

            IsReplaceMode = false;

            // Use MaterialCatalogControl's methods to update UI
            ChangeBackgroundColorToDefault();
            SetCancelReplaceButtonVisibility(Visibility.Collapsed);
            SetFinishReplaceButtonVisibility(Visibility.Collapsed);
            SetCartButtonVisibility(Visibility.Collapsed);
            SetValuesDataGrid.RemoveButtonColumn("Replace");

        }


        public void disableReplaceMode()
        {
            ChangeBackgroundColorToDefault();
            SetCancelReplaceButtonVisibility(Visibility.Collapsed);
            SetFinishReplaceButtonVisibility(Visibility.Collapsed);
            SetCartButtonVisibility(Visibility.Collapsed);
        }

        private void InitializeShoppingCartWithParts()
        {
            string query = "SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID IN (1, 3, 5)";
            DataTable partsTable = Utility.SearchLP(query);

            if (partsTable != null)
            {
                foreach (DataRow row in partsTable.Rows)
                {
                    PartItem partItem = new PartItem
                    {
                        ItemId = Convert.ToInt32(row["ITEM_ID"]),
                        OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                        PartNumber = row["PART_NUMBER"].ToString(),
                        Quantity = 1, // Set quantity to 1
                        Locator = row["DEFAULT_LOCATOR"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        ReceivingSubinv = row["RECEIVING_SUBINV"].ToString(),

                        Pegging = row["PEGGING"].ToString(),
                        LeadTime = Convert.ToInt32(row["LEAD_TIME"]),
                        AltDescription = row["ALT_DESCRIPTION"].ToString(),
                        // Add additional fields as needed
                    };
                    PartItems.Add(partItem);
                }
            }
            else
            {
                MessageBox.Show("Failed to initialize parts from the database.");
            }
        }

        private void InitializeCatalog()
        {
            string query = "SELECT * FROM TBL_CATEGORY_SETS WHERE FAMILY1 = 'EC' AND FAMILY2 = 'MAT' AND FAMILY3 = 'NA'";
            DataTable allCategories = Utility.SearchLP(query);

            var rootItems = ProcessCategories(allCategories);
            rootCatalogItems = new ObservableCollection<CatalogItem>(rootItems);

            // Create a root item
            CatalogItem root = new CatalogItem("Root", true)
            {
                SubItems = rootCatalogItems.Where(item => item.Initialized).ToList()
            };

            // Initialize navigation stack with root
            navigationStack.Clear();
            navigationStack.Push(root);
            LastSearchPartItems = new ObservableCollection<PartItem>();
            CurrentItems = new ObservableCollection<CatalogItem>(root.SubItems);
            LastSearchResults = new ObservableCollection<CatalogItem>();

            // Update DataGrid if root has set values
            if (root.SetValues.Any())
            {
                LoadSetValuesIntoDataGrid(root.SetValues);
            }

            UpdateNavigationPath();
            DataContext = this;
        }


        private void LoadSetValuesFromDatabase(string setId)
        {
            string partQuery = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID = {setId}";
            DataTable partInfo = Utility.SearchLP(partQuery);

            if (partInfo != null && partInfo.Rows.Count > 0)
            {
                DataRow row = partInfo.Rows[0];
                SetValueItems.Add(new PartItem
                {
                    ItemId = Convert.ToInt32(row["ITEM_ID"]),
                    OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                    PartNumber = row["PART_NUMBER"].ToString(),
                    Locator = row["DEFAULT_LOCATOR"].ToString(),
                    Description = row["DESCRIPTION"].ToString(),
                });
            }
        }


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
                    SubItems = FindSubItems(subCatIds, categories),
                    SetValues = setValues,
                    Initialized = Convert.ToBoolean(row["INITIALIZED"])
                };

                catalogItems.Add(catalogItem);
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


        public void BreadcrumbClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                if (textBlock.Text == "Root")
                {
                    NavigateToRoot();
                }
                else if (textBlock.Text == "Search Results")
                {
                    NavigateToSearchResults();
                }
                else
                {
                    NavigateToBreadcrumbItem(textBlock.Text);
                }
            }
        }

        private void NavigateToSearchResults()
        {
            // Clear the navigation stack up to the root
            while (navigationStack.Count > 1)
            {
                navigationStack.Pop();
            }

            // Push the "Search Results" item onto the stack again
            var searchResultsItem = new CatalogItem("Search Results");
            navigationStack.Push(searchResultsItem);

            // Populate the CurrentItems and SetValueItems from LastSearchResults
            CurrentItems.Clear();
            foreach (var item in LastSearchResults)
            {
                CurrentItems.Add(item);
            }

            SetValueItems.Clear();
            foreach (var item in LastSearchResults)
            {
                foreach (var setValueId in item.SetValues)
                {
                    LoadSetValuesFromDatabase(setValueId);
                }
            }

            // Repopulate the CurrentItems from LastSearchResults
            CurrentItems = new ObservableCollection<CatalogItem>(LastSearchResults);

            // Repopulate the SetValueItems from LastSearchPartItems
            SetValueItems = new ObservableCollection<PartItem>(LastSearchPartItems);
            SetValuesDataGrid.ItemsSource = SetValueItems; // Ensure the DataGrid's ItemsSource is updated

            SetValuesDataGrid.Items.Refresh();
            UpdateNavigationPath(); // This will update the breadcrumb
        }

        


        public void NavigateToRoot()
        {
            // Clear the navigation stack and add a new root item
            navigationStack.Clear();

            var root = new CatalogItem("Root", true)
            {
                SubItems = rootCatalogItems.Where(item => item.Initialized).ToList()
            };

            navigationStack.Push(root);

            // Update CurrentItems to show all categories where Initialized is true
            CurrentItems = new ObservableCollection<CatalogItem>(root.SubItems);

            // Refresh the ListBox with the new CurrentItems
            PartsListBox.ItemsSource = CurrentItems;

            // Clear the DataGrid by clearing the SetValueItems collection
            SetValueItems.Clear();
            SetValuesDataGrid.ItemsSource = SetValueItems;

            // Update Breadcrumb
            UpdateNavigationPath();
        }


        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selected = e.AddedItems[0] as CatalogItem;
                if (selected != null)
                {
                    await Task.Delay(200); // Delay for better UI response

                    // Update CurrentItems for ListBox navigation
                    CurrentItems.Clear();

                    if (selected.SubItems.Count > 0)
                    {
                        foreach (var item in selected.SubItems)
                        {
                            CurrentItems.Add(item);
                        }
                    }
                    //if (selected.SetValues.Any())
                    //{
                        // Load set values into DataGrid for the selected category
                        LoadSetValuesIntoDataGrid(selected.SetValues);
                    //}

                    navigationStack.Push(selected);
                    UpdateNavigationPath();
                }
            }
        }


        private void UpdateNavigationPath()
        {
            if (NavigationPath == null)
            {
                NavigationPath = new ObservableCollection<string>();
            }

            NavigationPath.Clear();
            var breadcrumbStack = navigationStack.Reverse().ToList();

            foreach (var item in breadcrumbStack)
            {
                NavigationPath.Add(item.Name);

                if (breadcrumbStack.Last() != item)
                {
                    NavigationPath.Add(">");
                }
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

                // Populate with subcategories or set values
                CurrentItems.Clear();
                if (currentItem.SubItems.Count > 0)
                {
                    foreach (var item in currentItem.SubItems)
                    {
                        CurrentItems.Add(item);
                    }
                }
                else if (currentItem.SetValues.Any())
                {
                    foreach (var setValue in currentItem.SetValues)
                    {
                        CurrentItems.Add(new CatalogItem(setValue));
                    }
                }

                // Load set values into DataGrid for the navigated category
                LoadSetValuesIntoDataGrid(currentItem.SetValues);
            }
            else
            {
                // Handle navigation to the root level
            }
            UpdateNavigationPath();
        }

        private void LoadSetValuesIntoDataGrid(List<string> setValues)
        {
            SetValueItems.Clear(); // Clearing existing items

            foreach (var setValue in setValues)
            {
                string query = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID = {setValue}";
                DataTable partInfo = Utility.SearchLP(query);

                if (partInfo != null && partInfo.Rows.Count > 0)
                {
                    DataRow row = partInfo.Rows[0];
                    SetValueItems.Add(new PartItem
                    {
                        ItemId = Convert.ToInt32(row["ITEM_ID"]), // Ensure ItemId is correctly set
                        OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                        PartNumber = row["PART_NUMBER"].ToString(),
                        Quantity = 1, // Default quantity
                        Locator = row["DEFAULT_LOCATOR"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        // ... other properties
                    });
                }
            }

            // Refresh the DataGrid
            SetValuesDataGrid.Items.Refresh();
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }

            // Perform the search in the global materials table
            var globalMaterialResults = SearchInGlobalMaterials(searchText);

            // Display the results in the SetValuesDataGrid
            DisplaySearchResultsInDataGrid(globalMaterialResults);

            // Find and update categories based on global materials search results
            UpdateCategoriesBasedOnGlobalMaterialResults(globalMaterialResults);

            // Update Breadcrumb for search results
            UpdateBreadcrumbForSearchResults();

            // Update the PartsListBox with the filtered categories
            PartsListBox.ItemsSource = CurrentItems;

            // Store the search results
            LastSearchResults = new ObservableCollection<CatalogItem>(CurrentItems);
            LastSearchPartItems = new ObservableCollection<PartItem>(globalMaterialResults);
        }

        private void UpdateCategoriesBasedOnGlobalMaterialResults(ObservableCollection<PartItem> globalMaterialResults)
        {
            var matchingItemIds = globalMaterialResults.Select(item => item.ItemId.ToString()).ToHashSet();

            CurrentItems.Clear();

            foreach (var category in rootCatalogItems)
            {
                if (category.SetValues.Any(setValueId => matchingItemIds.Contains(setValueId)))
                {
                    CurrentItems.Add(category);
                }
            }

            PartsListBox.ItemsSource = CurrentItems;
        }



        private ObservableCollection<PartItem> SearchInGlobalMaterials(string searchText)
        {
            var searchResults = new ObservableCollection<PartItem>();
            string query = $"SELECT DISTINCT ITEM_ID, INVENTORY_ITEM_ID, PART_NUMBER, DEFAULT_LOCATOR, DESCRIPTION " +
                           $"FROM SHOPPING_CART_MATERIAL_VIEW WHERE " +
                           $"INVENTORY_ITEM_ID LIKE '%{searchText}%' OR " +
                           $"PART_NUMBER LIKE '%{searchText}%' OR " +
                           $"DEFAULT_LOCATOR LIKE '%{searchText}%' OR " +
                           $"DESCRIPTION LIKE '%{searchText}%'";

            DataTable results = Utility.SearchLP(query);
            foreach (DataRow row in results.Rows)
            {
                searchResults.Add(new PartItem
                {
                    ItemId = Convert.ToInt32(row["ITEM_ID"]),
                    OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                    PartNumber = row["PART_NUMBER"].ToString(),
                    Locator = row["DEFAULT_LOCATOR"].ToString(),
                    Description = row["DESCRIPTION"].ToString()
                });
            }

            return searchResults;
        }

       

        private void DisplaySearchResultsInDataGrid(ObservableCollection<PartItem> searchResults)
        {
            SetValueItems.Clear();
            foreach (var result in searchResults)
            {
                SetValueItems.Add(result);
            }
            SetValuesDataGrid.Items.Refresh();
        }

        private void AddSetValueItems(string setValueId)
        {
            string query = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID = {setValueId}";
            DataTable partInfo = Utility.SearchLP(query);

            if (partInfo != null && partInfo.Rows.Count > 0)
            {
                DataRow row = partInfo.Rows[0];
                SetValueItems.Add(new PartItem
                {
                    OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                    PartNumber = row["PART_NUMBER"].ToString(),
                    Locator = row["DEFAULT_LOCATOR"].ToString(),
                    Description = row["DESCRIPTION"].ToString(),
                    // ... other properties if needed ...
                });
            }
        }

        
        private void UpdateBreadcrumbForSearchResults()
        {
            // Clear the navigation stack except for the root
            while (navigationStack.Count > 1)
            {
                navigationStack.Pop();
            }

            // Add a "Search Results" item to the navigation stack
            var searchResultsItem = new CatalogItem("Search Results");
            navigationStack.Push(searchResultsItem);

            UpdateNavigationPath();
        }


        private List<CatalogItem> SearchCatalogItemsIncludingSetValues(string searchText, ObservableCollection<CatalogItem> items)
        {
            var results = new HashSet<CatalogItem>(); // This will ensure uniqueness
            var lowerCaseSearchText = searchText.ToLower();

            foreach (var item in items)
            {
                bool hasMatchingSetValue = false;

                // Search within the item's set values
                foreach (var setValueId in item.SetValues)
                {
                    string query = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID = {setValueId}";
                    DataTable partInfo = Utility.SearchLP(query);

                    if (partInfo != null && partInfo.Rows.Count > 0)
                    {
                        DataRow row = partInfo.Rows[0];
                        if (row["INVENTORY_ITEM_ID"].ToString().ToLower().Contains(lowerCaseSearchText) ||
                            row["PART_NUMBER"].ToString().ToLower().Contains(lowerCaseSearchText) ||
                            row["DEFAULT_LOCATOR"].ToString().ToLower().Contains(lowerCaseSearchText) ||
                            row["DESCRIPTION"].ToString().ToLower().Contains(lowerCaseSearchText))
                        {
                            hasMatchingSetValue = true;
                            break; // Break once a match is found, no need to add duplicates
                        }
                    }
                }

                if (hasMatchingSetValue)
                {
                    results.Add(item);
                }

                // Recursively search in sub-items
                if (item.SubItems != null && item.SubItems.Any())
                {
                    var subItemsAsObservableCollection = new ObservableCollection<CatalogItem>(item.SubItems);
                    foreach (var subItem in SearchCatalogItemsIncludingSetValues(lowerCaseSearchText, subItemsAsObservableCollection))
                    {
                        results.Add(subItem);
                    }
                }
            }

            // Convert HashSet to List
            return results.ToList();
        }

        private List<CatalogItem> SearchCatalogItems(string searchText, List<CatalogItem> items)
        {
            var results = new List<CatalogItem>();

            foreach (var item in items)
            {
                // Check if the current item's name matches the search text
                if (item.Name.ToLower().Contains(searchText))
                {
                    results.Add(item);
                }

                // Recursively search in sub-items
                if (item.SubItems.Any())
                {
                    results.AddRange(SearchCatalogItems(searchText, item.SubItems));
                }
            }

            return results;
        }

        private void AddSubcategoryClick(object sender, RoutedEventArgs e)
        {
            // Show the CategoryTextBox and AddSubButtonFinal
            CategoryTextBox.Visibility = Visibility.Visible;
            AddSubButtonFinal.Visibility = Visibility.Visible;

            // Change button visibilities
            AddSubcategoryButton.Visibility = Visibility.Collapsed;
            CancelAddSubcategory.Visibility = Visibility.Visible;
        }

        private void CancelAddSubcategory_Click(object sender, RoutedEventArgs e)
        {
            // Hide the CategoryTextBox and AddSubButtonFinal
            CategoryTextBox.Visibility = Visibility.Collapsed;
            AddSubButtonFinal.Visibility = Visibility.Collapsed;

            // Revert visibility changes
            AddSubcategoryButton.Visibility = Visibility.Visible;
            CancelAddSubcategory.Visibility = Visibility.Collapsed;
        }

        private int GetCategoryIdByName(string categoryName)
        {
            string query = $"SELECT CAT_SET_ID FROM TBL_CATEGORY_SETS WHERE CAT_SET_NAME = '{categoryName}'";
            DataTable result = Utility.SearchLP(query);
            if (result != null && result.Rows.Count > 0)
            {
                return Convert.ToInt32(result.Rows[0]["CAT_SET_ID"]);
            }
            else
            {
                throw new Exception("Category not found.");
            }
        }

        private int AddSubcategoryToDatabase(string subcategoryName)
        {
            string insertQuery = $"INSERT INTO TBL_CATEGORY_SETS (CAT_SET_NAME, FAMILY1, FAMILY2, FAMILY3) VALUES ('{subcategoryName}', 'EC', 'MAT', 'NA')";
            Utility.ExecuteNonQueryLP(insertQuery);

            return GetCategoryIdByName(subcategoryName); // Retrieve the newly added category's ID
        }

        private void UpdateParentCategorySubcatIds(int parentId, int newSubcategoryId)
        {
            string getParentQuery = $"SELECT SUBCAT_IDS FROM TBL_CATEGORY_SETS WHERE CAT_SET_ID = {parentId}";
            DataTable parentData = Utility.SearchLP(getParentQuery);
            string existingSubcatIds = parentData.Rows[0]["SUBCAT_IDS"].ToString();
            string updatedSubcatIds = string.IsNullOrEmpty(existingSubcatIds) ? newSubcategoryId.ToString() : $"{existingSubcatIds},{newSubcategoryId}";

            string updateQuery = $"UPDATE TBL_CATEGORY_SETS SET SUBCAT_IDS = '{updatedSubcatIds}' WHERE CAT_SET_ID = {parentId}";
            Utility.ExecuteNonQueryLP(updateQuery);
        }

        private void RefreshUI()
        {
            InitializeCatalog();
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationPath.Count == 0)
            {
                MessageBox.Show("No category selected.");
                return;
            }
            CurrentCategoryName = NavigationPath.Last();

            // Make AddMaterialCopy TextBlock visible
            AddMaterialCopy.Visibility = Visibility.Visible;

            // Add 'Sub' button to DataGrid
            SetValuesDataGrid.AddButtonColumn("Sub", "Sub", AddPartToBOM_Click, Colors.Purple, 35, 20);

            // Change button visibilities
            AddMaterialButton.Visibility = Visibility.Collapsed;
            CancelMaterialButton.Visibility = Visibility.Visible;
        }

        private void CancelMaterial_Click(object sender, RoutedEventArgs e)
        {
            // Remove 'Sub' button from DataGrid
            SetValuesDataGrid.RemoveButtonColumn("Sub");

            // Revert visibility changes
            AddMaterialButton.Visibility = Visibility.Visible;
            CancelMaterialButton.Visibility = Visibility.Collapsed;

            // Hide the AddMaterialCopy TextBlock if it's visible
            AddMaterialCopy.Visibility = Visibility.Collapsed;
        }


        public void CatalogMaintenanceMode()
        {
            // Change the header text
            MaterialCatalogGroupBox.Header = "Material Catalog - Maintenance Mode";

            ToggleDeleteButtonVisibility(true);

            ColorZoneAssist.SetMode(MaterialCatalogGroupBox, ColorZoneMode.SecondaryMid);



            AddSubcategoryButton.Visibility = Visibility.Visible;
            AddMaterialButton.Visibility = Visibility.Visible;
            // Add 'Remove' button to the DataGrid

            SetValuesDataGrid.AddButtonColumn("Remove", "Remove", RemovePartFromCategory_Click, Colors.Red, 55, 20);
        }


        private void RemovePartFromCategory_Click(object sender, RoutedEventArgs e)
        {
            // Check if the current view is "Search Results"
            if (NavigationPath.LastOrDefault() == "Search Results")
            {
                MessageBox.Show("Cannot remove materials in search results view.");
                return;
            }

            // Retrieve the current category name from the last item in the navigation path
            string currentCategoryName = NavigationPath.LastOrDefault();
            if (string.IsNullOrEmpty(currentCategoryName))
            {
                MessageBox.Show("No category selected.");
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is PartItem partItem)
            {
                int materialId = partItem.ItemId;
                int categoryId = GetCategoryIdByName(currentCategoryName);
                if (RemoveMaterialFromCategory(categoryId, materialId))
                {
                    MessageBox.Show($"Material '{partItem.PartNumber}' removed from '{currentCategoryName}'.");

                    // Refresh UI to reflect changes
                    RefreshUI();
                }
                else
                {
                    MessageBox.Show($"Failed to remove material '{partItem.PartNumber}' from '{currentCategoryName}'.");
                }
            }
            else
            {
                MessageBox.Show("No material selected.");
            }
        }

        private bool RemoveMaterialFromCategory(int categoryId, int materialId)
        {
            // Fetch the current set values for the category
            string getCategoryQuery = $"SELECT SET_VALUES FROM TBL_CATEGORY_SETS WHERE CAT_SET_ID = {categoryId}";
            DataTable categoryData = Utility.SearchLP(getCategoryQuery);
            string existingSetValues = categoryData.Rows[0]["SET_VALUES"].ToString();

            var setValuesList = existingSetValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            if (setValuesList.Contains(materialId))
            {
                setValuesList.Remove(materialId);
                string updatedSetValues = string.Join(",", setValuesList);

                // Update the category with the new set values
                string updateQuery = $"UPDATE TBL_CATEGORY_SETS SET SET_VALUES = '{updatedSetValues}' WHERE CAT_SET_ID = {categoryId}";
                Utility.ExecuteNonQueryLP(updateQuery);
                return true;
            }

            return false; // Material not found in the category
        }


        private bool AddMaterialToCategory(int categoryId, int materialId)
        {
            string getCategoryQuery = $"SELECT SET_VALUES FROM TBL_CATEGORY_SETS WHERE CAT_SET_ID = {categoryId}";
            DataTable categoryData = Utility.SearchLP(getCategoryQuery);
            string existingSetValues = categoryData.Rows[0]["SET_VALUES"].ToString();

            // Safely parse SET_VALUES to a list of integers, handling empty and non-numeric cases
            var setValuesList = existingSetValues
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int n) ? n : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .ToList();

            if (setValuesList.Contains(materialId))
            {
                return false; // Material already exists
            }

            setValuesList.Add(materialId);
            string updatedSetValues = string.Join(",", setValuesList);

            string updateQuery = $"UPDATE TBL_CATEGORY_SETS SET SET_VALUES = '{updatedSetValues}' WHERE CAT_SET_ID = {categoryId}";
            Utility.ExecuteNonQueryLP(updateQuery);
            return true;
        }

        private void AddSubcategoryNameClick(object sender, RoutedEventArgs e)
        {
            if (NavigationPath.Count == 0)
            {
                MessageBox.Show("No category selected.");
                return;
            }

            string currentCategoryName = NavigationPath.Last();
            int currentCategoryId = GetCategoryIdByName(currentCategoryName);

            string newSubcategoryName = CategoryTextBox.Text; // Get the name from CategoryTextBox
            if (string.IsNullOrWhiteSpace(newSubcategoryName))
            {
                MessageBox.Show("Please enter a valid subcategory name.");
                return;
            }

            int newSubcategoryId = AddSubcategoryToDatabase(newSubcategoryName);
            UpdateParentCategorySubcatIds(currentCategoryId, newSubcategoryId);

            MessageBox.Show($"Subcategory '{newSubcategoryName}' added under '{currentCategoryName}'.");
            RefreshUI();

            // Hide the CategoryTextBox and AddSubButtonFinal after adding the subcategory
            CategoryTextBox.Visibility = Visibility.Collapsed;
            AddSubButtonFinal.Visibility = Visibility.Collapsed;
            CancelAddSubcategory.Visibility = Visibility.Collapsed;
            AddSubcategoryButton.Visibility = Visibility.Visible;
        }

        private void AddPartToBOM_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentCategoryName))
            {
                MessageBox.Show("No category selected.");
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is PartItem partItem)
            {
                int materialId = partItem.ItemId;
                int categoryId = GetCategoryIdByName(CurrentCategoryName);

                if (AddMaterialToCategory(categoryId, materialId))
                {
                    MessageBox.Show($"Material '{partItem.PartNumber}' added to '{CurrentCategoryName}'.");

                    // Remove the 'Sub' button from the DataGrid
                    SetValuesDataGrid.RemoveButtonColumn("Sub");

                    // Make the AddMaterialCopy TextBlock invisible
                    AddMaterialCopy.Visibility = Visibility.Collapsed;

                    RefreshUI();
                }
                else
                {
                    MessageBox.Show($"Material '{partItem.PartNumber}' could not be added to '{CurrentCategoryName}'. It might already exist in the category.");
                }
            }
            else
            {
                MessageBox.Show("No material selected.");
            }
        }

        private void RemoveCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CatalogItem catalogItem)
            {
                string catSetName = catalogItem.Name; // Retrieve the CAT_SET_NAME from the clicked ListBox entity

                try
                {
                    // SQL command to delete the category
                    string sqlDeleteCommand = "DELETE FROM TBL_CATEGORY_SETS WHERE CAT_SET_NAME = ?";

                    // Create a parameter for the command
                    var param = new OleDbParameter("@CAT_SET_NAME", catSetName);

                    // Execute the delete operation
                    Utility.ExecuteNonQueryLPWithParams(sqlDeleteCommand, param);

                    // Show a success message
                    MessageBox.Show($"Category '{catSetName}' successfully deleted.");

                    RefreshUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occurred while deleting the category: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Could not identify the category to delete.");
            }
        }


    }
}

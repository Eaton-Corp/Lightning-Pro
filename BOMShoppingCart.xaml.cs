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
using System.Globalization;
using System.ComponentModel;
using System.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Net.Http.Headers;

namespace LightningPRO
{
    public partial class BOMShoppingCart : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<PartItem> PartItems { get; set; }

        public ObservableCollection<PartItem> SetValueItems { get; set; }

        public MaterialCatalogControl myMaterialCatalogControl;
        public ObservableCollection<AlternatePartItem> AlternatePartItems { get; set; }

        private Stack<CatalogItem> navigationStack = new Stack<CatalogItem>();

        //public ObservableCollection<int> QuantityOptions { get; set; }

        private CatalogItem selectedItemForReplacement;

        public ObservableCollection<CatalogItem> CurrentItems { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<CatalogItem> rootCatalogItems { get; set; }

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

        private string orderSpecsJson;



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

        public static readonly DependencyProperty ProductGroupProperty =
        DependencyProperty.Register(
            "ProductGroup",
            typeof(string),
            typeof(BOMShoppingCart),
            new PropertyMetadata(null, OnProductGroupChanged));

        public string ProductGroup
        {
            get { return (string)GetValue(ProductGroupProperty); }
            set { SetValue(ProductGroupProperty, value); }
        }

        private static void OnProductGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BOMShoppingCart control = (BOMShoppingCart)d;
            control.HandleProductGroupChange((string)e.NewValue);
        }

        private void HandleProductGroupChange(string productGroup)
        {
            this.ProductGroup = productGroup;            
        }

        public BOMShoppingCart()
        {
            InitializeComponent();
        }

        private void BOMCart_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeDataContext();
            InitializeCatalog();
            InitializePartItemsCollections();
            ConfigureDataGrids();
            SetDataGridItemSources();
        }

        private void InitializeDataContext()
        {
            this.DataContext = this;
            NavigationPath = new ObservableCollection<string>();
        }

        

        private void InitializePartItemsCollections()
        {
            //PartItems = CreateDefaultPartItems();
            AlternatePartItems = CreateDefaultAlternatePartItems();
            // Set the ItemsSource for myEatonDataGrid
            //myEatonDataGrid.ItemsSource = PartItems;
            // Refresh the DataGrid to update the UI
            //myEatonDataGrid.Items.Refresh();
        }

        private ObservableCollection<PartItem> CreateDefaultPartItems()
        {
            return new ObservableCollection<PartItem>
            {
                //new PartItem { PartNumber = "part1", Quantity = 1, Locator = "A1", Description = "Description for part1" },
                //new PartItem { PartNumber = "part2", Quantity = 1, Locator = "A1", Description = "Description for part2" },
            };
        }

        private ObservableCollection<AlternatePartItem> CreateDefaultAlternatePartItems()
        {
            return new ObservableCollection<AlternatePartItem>
            {
                //new AlternatePartItem { GOItem = "Item1", TimesUsed = 5, DateCreated = DateTime.Now.AddDays(-10) },
                //new AlternatePartItem { GOItem = "Item2", TimesUsed = 3, DateCreated = DateTime.Now.AddDays(-20) },
                //new AlternatePartItem { GOItem = "Item3", TimesUsed = 8, DateCreated = DateTime.Now.AddDays(-30) },
            };
        }

        private void ConfigureDataGrids()
        {
            var quantityOptions = Enumerable.Range(1, 1000).ToList();
            ConfigureMaterialCatalogDataGrid(quantityOptions);
            ConfigureMyEatonDataGrid(quantityOptions);
            ConfigureAlternateDataGrid();
        }

        private void ConfigureMaterialCatalogDataGrid(List<int> quantityOptions)
        {
            MaterialCatalogControl.AddComboBoxColumnToDataGrid("Qty", "Quantity", quantityOptions, "DisplayMember", "ValueMember", 55, ComboBox_SelectionChanged);
            MaterialCatalogControl.AddButtonToDataGrid("", "Info", ViewSetValue_Click, Colors.Blue, 35, 20);
            MaterialCatalogControl.AddButtonToDataGrid("Add", "Add", AddPartToBOM_Click, Colors.Green, 35, 20);
        }

        private void ConfigureMyEatonDataGrid(List<int> quantityOptions)
        {
            myEatonDataGrid.AddComboBoxColumn("Qty", "Quantity", quantityOptions, "DisplayMember", "ValueMember", 55, ComboBox_SelectionChanged);
            myEatonDataGrid.AddButtonColumn("", "Delete", DeleteButton_Click, Colors.Red, 55, 20);
            myEatonDataGrid.AddButtonColumn("", "Info", ViewButton_Click, Colors.Blue, 35, 20);
            myEatonDataGrid.AddDropdownButtonColumn("More Options", new Dictionary<string, RoutedEventHandler> { 
                { "Replace Parts", ReplaceButton_Click }, 
                { "Find Usage", WhereUsedButton_Click }
            }, 55, 20);            
        }

        private void ConfigureAlternateDataGrid()
        {
            AlternateDataGrid.AddButtonColumn("", "View", ViewComprehensiveBOM_Click, Colors.Orange, 75, 20);
        }

        private void SetDataGridItemSources()
        {
            MaterialCatalogControl.ExternalCancelReplaceButtonClickHandler += MyCustomCancelReplaceHandler;
            MaterialCatalogControl.ExternalFinishReplaceButtonClickHandler += FinalizeReplacement_Click;
            AlternateDataGrid.ItemsSource = AlternatePartItems;
            AlternateBOMBreakdown.ItemsSource = PartItems;
        }


        public void FinalizeReplacement_Click(object sender, EventArgs e)
        {
            // Get the DataGrid's ItemsSource (PartItems)
            ObservableCollection<PartItem> partItems = myEatonDataGrid.ItemsSource as ObservableCollection<PartItem>;
            if (partItems != null)
            {
                // Find and remove the partItems where isSelectedForReplacement is true
                for (int i = partItems.Count - 1; i >= 0; i--)
                {
                    if (partItems[i].isSelectedForReplacement)
                    {
                        partItems.RemoveAt(i);
                    }
                }

                // Add the replacement parts to the DataGrid's ItemsSource (PartItems)
                for (int i = 0; i < replacementItems.Count; i++)
                {
                    var replacementPart = replacementItems[i];
                    partItems.Add(replacementPart);
                }
            }

            // Refresh the DataGrid
            myEatonDataGrid.Items.Refresh();

            // Reset the ReplaceMode and other UI elements
            IsReplaceMode = false;
            MaterialCatalogControl.disableReplaceMode();
            MaterialCatalogControl.RemoveButtonFromDataGrid("Replace");
            MaterialCatalogControl.ChangeBackgroundColorToDefault();
            myEatonDataGrid.RemoveButtonColumn("Replace");

            // Clear the selectedItemsForReplacement list
            selectedItemsForReplacement.Clear();
        }

        public void MyCustomCancelReplaceHandler(object sender, EventArgs e)
        {
            IsReplaceMode = false;
            myEatonDataGrid.RemoveButtonColumn("Replace");
        }



        private string ConstructImagePath(AlternatePartItem goItem)
        {
            string directory = Utility.GetDirectoryForOrderFiles(goItem.GOItem, Utility.ProductGroup.PRL123); // Adjust this method as needed                                                                                             // Construct the full path (you might need to adjust this)
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(directory, goItem.GOItem + ".png")); // Assuming the image is a .png file
        }

        public void LoadCustomPartItems(ObservableCollection<PartItem> customPartItems, string orderSpecs)
        {
            // Initialize PartItems if it's null
            if (PartItems == null)
            {
                PartItems = new ObservableCollection<PartItem>();
            }
            else
            {
                // Clear existing items if you don't want to keep them
                PartItems.Clear();
            }

            // Adding items from customPartItems to PartItems using a for loop
            for (int i = 0; i < customPartItems.Count; i++)
            {
                PartItems.Add(customPartItems[i]);
            }

            myEatonDataGrid.ItemsSource = PartItems;
            myEatonDataGrid.Items.Refresh();

            // Set value for catalog matching
            orderSpecsJson = orderSpecs;

            //update alternate data grid
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
                        OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                        PartNumber = row["PART_NUMBER"].ToString(),
                        Quantity = 1, // Set quantity to 1
                        Locator = row["DEFAULT_LOCATOR"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        ReceivingSubinv = row["RECEIVING_SUBINV"].ToString(),
                        
                        Pegging = row["PEGGING"].ToString(),
                        LeadTime = Convert.ToInt32(row["LEAD_TIME"]),
                        AltDescription = row["ALT_DESCRIPTION"].ToString(),
                    };
                    PartItems.Add(partItem);
                }
            }
            else
            {
                MessageBox.Show("Failed to initialize parts from the database.");
            }
        }



        private void ViewSetValue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is PartItem partItem)
            {
                MessageBox.Show($"Part Name: {partItem.PartNumber}\nQuantity: {partItem.Quantity}\nLocator: {partItem.Locator}\nDescription: {partItem.Description}", "Set Value Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is PartItem partItem)
            {
                // Update the quantity in partItem based on the selection
                if (comboBox.SelectedItem is int selectedQuantity)
                {
                    partItem.Quantity = selectedQuantity;
                    // You might want to refresh the DataGrid or perform other updates
                    myEatonDataGrid.Items.Refresh();

                }
            }
        }

        private void AddPartToBOM_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PartItem partToAdd)
            {
                // Add the part to the BOM
                PartItems.Add(new PartItem
                {
                    OracleNumber = partToAdd.OracleNumber,
                    PartNumber = partToAdd.PartNumber,
                    Quantity = partToAdd.Quantity,
                    Locator = partToAdd.Locator,
                    Description = partToAdd.Description
                });

                MessageBox.Show($"{partToAdd.PartNumber} added to BOM.");
            }
            myEatonDataGrid.ItemsSource = PartItems;
            myEatonDataGrid.Items.Refresh();
        }

        private void WhereUsedButton_Click(object sender, RoutedEventArgs e)
        {
            if (myEatonDataGrid.SelectedItem is PartItem selectedPart)
            {
                // Query to find all BOMs where the selected part is used
                string query = $"SELECT * FROM BOM_STORE WHERE BOM LIKE '%{selectedPart.PartNumber}%'";
                DataTable bomsWithPart = Utility.SearchLP(query);

                // Process the result and update the AlternatePartItems collection
                AlternatePartItems.Clear();
                foreach (DataRow bomRow in bomsWithPart.Rows)
                {
                    AlternatePartItems.Add(ConvertToAlternatePartItem(bomRow));
                }

                // Change view to show the results
                MainView.Visibility = Visibility.Collapsed;
                AlternateBOMView.Visibility = Visibility.Visible;

                // Bind the results to the DataGrid
                AlternateDataGrid.ItemsSource = AlternatePartItems;
            }
        }

        private List<PartItem> selectedItemsForReplacement = new List<PartItem>();
        //private List<PartItem> replacementItems = new List<PartItem>();
        private ObservableCollection<PartItem> replacementItems = new ObservableCollection<PartItem>();


        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            IsReplaceMode = true;
            MaterialCatalogControl.ChangeBackgroundColor(Colors.Orange);
            MaterialCatalogControl.SetCancelReplaceButtonVisibility(Visibility.Visible);
            MaterialCatalogControl.SetFinishReplaceButtonVisibility(Visibility.Visible);
            MaterialCatalogControl.SetCartButtonVisibility(Visibility.Visible);
            myEatonDataGrid.AddCheckBoxColumn("Replace", "isSelectedForReplacement", 50);
            MaterialCatalogControl.AddButtonToDataGrid("Replace", "Replace", AddReplacementItem_Click, Colors.Orange, 60, 20);
        }

        private void AddReplacementItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PartItem replacementPart)
            {
                //replacementItems.Add(replacementPart);
                //myMaterialCatalogControl.setCartContent();
                // Optionally, update UI to reflect selection
                
                replacementItems.Add(replacementPart);
                string cartContent = "(" + replacementItems.Count + ") Cart";
                MaterialCatalogControl.ReplacementItems.Add(replacementPart);
                MaterialCatalogControl.setCartContent(cartContent);
                MaterialCatalogControl.setReplacementItems(replacementItems);

            }
        }

        private void InitializeCatalog()
        {
            string query = "SELECT * FROM TBL_CATEGORY_SETS WHERE FAMILY1 = 'EC' AND FAMILY2 = 'MAT' AND FAMILY3 = 'NA'";
            DataTable allCategories = Utility.SearchLP(query);

            var rootItems = ProcessCategories(allCategories);
            rootCatalogItems = new ObservableCollection<CatalogItem>(rootItems);

            if (rootCatalogItems.Any(item => item.Initialized))
            {
                foreach (var item in rootCatalogItems.Where(item => item.Initialized))
                {
                    LoadSetValuesFromDatabase(item);
                }
                CurrentItems = new ObservableCollection<CatalogItem>(rootCatalogItems.FirstOrDefault(item => item.Initialized).SubItems);
                navigationStack.Push(rootCatalogItems.FirstOrDefault(item => item.Initialized));
                UpdateNavigationPath();
            }
            else
            {
                CurrentItems = new ObservableCollection<CatalogItem>();
            }

            DataContext = this;
        }

        private void LoadSetValuesFromDatabase(CatalogItem catalogItem)
        {
            foreach (var setId in catalogItem.SetValues)
            {
                string partQuery = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE ITEM_ID = {setId}";
                DataTable partInfo = Utility.SearchLP(partQuery);

                if (partInfo != null && partInfo.Rows.Count > 0)
                {
                    DataRow row = partInfo.Rows[0];
                    catalogItem.SubItems.Add(new CatalogItem(row["PART_NUMBER"].ToString())
                    {
                        OracleNumber = row["INVENTORY_ITEM_ID"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        Locator = row["DEFAULT_LOCATOR"].ToString(),
                    });
                }
            }
        }

        private List<CatalogItem> ProcessCategories(System.Data.DataTable categories)
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


        private void UpdateNavigationPath()
        {
            if (NavigationPath == null)
            {
                NavigationPath = new ObservableCollection<string>();
            }

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

        private string GenerateRandomLocator()
        {
            Random random = new Random();
            return $"{random.Next(10):00}.{random.Next(1000000000):000000000}.{random.Next(10):0}X";
        }

        private string GenerateRandomDescription()
        {
            Random random = new Random();
            return "Generic description";
        }



        private DataTable FetchAllBOMs()
        {
            string query = "SELECT * FROM BOM_STORE WHERE FAMILY1 = 'EC' AND FAMILY2 = 'MAT' AND FAMILY3 = 'NA'";
            DataTable allBOMs = Utility.SearchLP(query);
            return allBOMs;
        }

        private AlternatePartItem ConvertToAlternatePartItem(DataRow bom)
            {
            return new AlternatePartItem
            {
                GOItem = bom["GO_ITEM"].ToString(),
                TimesUsed = int.Parse(bom["TIMESIMPORTED"].ToString()),
                DateCreated = DateTime.Parse(bom["DATECREATED"].ToString()),
                Creator = bom["CREATOR"].ToString() // Add this line
            };
        }

        private List<CatalogItem> SearchCatalogItemsIncludingSetValues(string searchText, ObservableCollection<CatalogItem> items)
        {
            var results = new HashSet<CatalogItem>();
            var lowerCaseSearchText = searchText.ToLower();

            foreach (var item in items)
            {
                // Check if the item's name contains the search text
                if (item.Name.ToLower().Contains(lowerCaseSearchText))
                {
                    results.Add(item);
                }

                // Also search within the item's set values
                if (item.SetValues != null)
                {
                    foreach (var setValue in item.SetValues)
                    {
                        if (setValue.ToLower().Contains(lowerCaseSearchText))
                        {
                            // If a set value matches, add the parent item to the results
                            results.Add(item);
                            break; // Break to avoid adding the same item multiple times
                        }
                    }
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

        private double CalculateBOMSimilarityScore(string currentBOMJson, string alternateBOMJson)
        {
            var currentBOM = JsonConvert.DeserializeObject<Dictionary<string, int>>(currentBOMJson);
            var alternateBOM = JsonConvert.DeserializeObject<Dictionary<string, int>>(alternateBOMJson);

            double totalPossibleScore = 0;
            double achievedScore = 0;

            // All unique parts in both BOMs
            var allParts = new HashSet<string>(currentBOM.Keys.Concat(alternateBOM.Keys));

            foreach (var part in allParts)
            {
                int currentQty = currentBOM.ContainsKey(part) ? currentBOM[part] : 0;
                int alternateQty = alternateBOM.ContainsKey(part) ? alternateBOM[part] : 0;

                int maxQty = Math.Max(currentQty, alternateQty);
                totalPossibleScore += maxQty;

                // Minimum of the two quantities is the match score for that part
                int matchScore = Math.Min(currentQty, alternateQty);
                achievedScore += matchScore;
            }

            return totalPossibleScore > 0 ? (achievedScore / totalPossibleScore) * 100 : 0;
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
            if (sender is FrameworkElement element && element.DataContext is PartItem partItem)
            {
                StringBuilder message = new StringBuilder();
                message.AppendLine("Part Details:");
                message.AppendLine($"Part Number: {partItem.PartNumber}");
                message.AppendLine($"Quantity: {partItem.Quantity}");
                message.AppendLine($"Locator: {partItem.Locator}");
                message.AppendLine($"Description: {partItem.Description}");
                message.AppendLine($"Receiving Subinventory: {partItem.ReceivingSubinv}");
                message.AppendLine($"Make/Buy: {partItem.MakeBuy}");
                message.AppendLine($"Planning Method: {partItem.PlanningMethod}");
                message.AppendLine($"Pegging: {partItem.Pegging}");
                message.AppendLine($"Lead Time: {partItem.LeadTime}");
                message.AppendLine($"Frozen Cost: {partItem.FrozenCost}");
                message.AppendLine($"Alternate Description: {partItem.AltDescription}");
                message.AppendLine($"Source ID: {partItem.SourceId}");
                message.AppendLine($"Last Updated: {partItem.LastUpdated}");
                message.AppendLine($"Explodable: {partItem.Explodable}");
                MessageBox.Show(message.ToString(), "Part Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (AlternateDataGrid.SelectedItem is AlternatePartItem selectedAlternateBOM)
            {
                LoadAlternateBOM(selectedAlternateBOM.GOItem);
                string pdfDirectory = Utility.GetDirectoryForOrderFiles(selectedAlternateBOM.GOItem, Utility.ProductGroup.PRL123);

                string goItemFull = selectedAlternateBOM.GOItem; // "CSMU129570-00I"
                int hyphenPosition = goItemFull.IndexOf('-');
                string goItemShort;
                if (hyphenPosition != -1)
                {
                    goItemShort = goItemFull.Substring(0, hyphenPosition); // "CSMU129570"
                }
                else
                {
                    // Handle cases where '-' is not found
                    // For example, use the full GO Item
                    goItemShort = goItemFull;
                }

                string directoryPath = ConfigurationManager.ConnectionStrings["orderFiles"].ToString() +"\\" + goItemShort + "\\Images";
                string goItem = selectedAlternateBOM.GOItem; 

                List<BitmapImage> images = LoadImagesForGOItem(goItem, directoryPath);
                ImageView.LoadImages(images, 0);

            }
            MainView.Visibility = Visibility.Collapsed;
            AlternateBOMView.Visibility = Visibility.Collapsed;
            AlternateBomBreakdown.Visibility = Visibility.Visible;

        }

        public List<BitmapImage> LoadImagesForGOItem(string goItem, string directoryPath)
        {
            var images = new List<BitmapImage>();

            // Normalize the GO Item format
            goItem = goItem.Replace("-", "_"); // Assuming "-" should be replaced with "_"

            // Get all files in the directory
            string[] filePaths = Directory.GetFiles(directoryPath);

            // Filter and load images
            foreach (string filePath in filePaths)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(filePath).StartsWith(goItem))
                {
                    try
                    {
                        BitmapImage img = Utility.PNGtoBitmap(filePath);
                        images.Add(img);
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions (e.g., invalid image file)
                        // For example: Log the error or show a message
                    }
                }
            }

            return images;
        }


        private BitmapImage LoadBitmapFromUrl(string url)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // Optional, useful for performance in multi-threaded scenarios
            return bitmap;
        }

        private BitmapImage LoadBitmapFromFile(string filePath)
        {
            BitmapImage bitmap = new BitmapImage();

            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load image into memory to prevent file locking
                bitmap.EndInit();
                bitmap.Freeze(); // Optional: Makes the BitmapImage thread-safe
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file not found, invalid file format)
                MessageBox.Show("Error loading image: " + ex.Message);
            }

            return bitmap;
        }


        private BitmapImage LoadBitmapImage(string filePath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // Optional, for performance in multi-threaded scenarios
            return bitmap;
        }


        private void LoadAlternateBOM(string goItem)
        {
            // Fetch the BOM details from your data source
            var bomDetails = FetchBOMDetailsByGOItem(goItem);

            // Check if BOM details are found
            if (bomDetails != null)
            {
                // Deserialize and populate the AlternateBOMBreakdown DataGrid
                PopulateAlternateBOMBreakdown(bomDetails["BOM"].ToString());
            }
            else
            {
                // Handle the case where BOM details are not found
                MessageBox.Show("BOM details not found for the selected item.");
            }
        }

        private DataRow FetchBOMDetailsByGOItem(string goItem)
        {
            // Replace this with your actual database query logic
            string query = $"SELECT * FROM BOM_STORE WHERE GO_ITEM = '{goItem}'";
            DataTable bomDetailsTable = Utility.SearchLP(query);

            if (bomDetailsTable.Rows.Count > 0)
            {
                return bomDetailsTable.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private void AlertButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            AlternateBOMView.Visibility = Visibility.Visible;

            DataTable allBOMs = FetchAllBOMs();
            DataRow currentBOM = GetCurrentBOM();  // Fetch the current BOM
            string currentBOMJson = GetCurrentBOMJson();  // Get the current BOM in JSON formatpartItem.status

            // Check if the 'ORDERSPECS' column exists in the currentBOM DataRow
            if (!currentBOM.Table.Columns.Contains("ORDERSPECS"))
            {
                MessageBox.Show("ORDERSPECS column not found in current BOM.");
                return;
            }

            string currentOrderSpecs = currentBOM["ORDERSPECS"].ToString();

            AlternatePartItems.Clear();  // Clear existing items

            foreach (DataRow bom in allBOMs.Rows)
            {
                // Ensure the 'ORDERSPECS' and 'BOM' columns exist in the DataRow
                if (bom.Table.Columns.Contains("ORDERSPECS") && bom.Table.Columns.Contains("BOM"))
                {
                    string alternateOrderSpecs = bom["ORDERSPECS"].ToString();
                    if (alternateOrderSpecs == currentOrderSpecs) // Check for exact match in ORDERSPECS
                    {
                        string alternateBOMJson = bom["BOM"].ToString();
                        double similarityScore = CalculateBOMSimilarityScore(currentBOMJson, alternateBOMJson);

                        AlternatePartItem alternateItem = ConvertToAlternatePartItem(bom);
                        alternateItem.SimilarityScore = similarityScore; // Set the similarity score
                        AlternatePartItems.Add(alternateItem);
                    }
                }
            }

            // Sort the items based on similarity score
            AlternatePartItems = new ObservableCollection<AlternatePartItem>(AlternatePartItems.OrderByDescending(x => x.SimilarityScore));

            AlternateDataGrid.ItemsSource = AlternatePartItems;
        }


        private DataRow GetCurrentBOM()
        {
            var bomTable = new DataTable();
            bomTable.Columns.Add("ORDERSPECS", typeof(string));
            bomTable.Columns.Add("BOM", typeof(string));

            // Assuming GetOrderSpecsFromCurrentBOM fetches the correct order specs
            string currentOrderSpecs = orderSpecsJson;
            string currentBOMJson = GetCurrentBOMJson();

            bomTable.Rows.Add(currentOrderSpecs, currentBOMJson);

            return bomTable.Rows[0];
        }

        private string GetCurrentBOMJson()
        {
            var bomDict = new Dictionary<string, int>();

            foreach (var partItem in PartItems)
            {
                bomDict[partItem.PartNumber] = partItem.Quantity;
            }

            return JsonConvert.SerializeObject(bomDict);
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

        private void ReplacePartFromSetValue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PartItem setValuePartItem)
            {
                var partToReplace = PartItems.FirstOrDefault(p => p.PartNumber == selectedItemForReplacement.Name);
                if (partToReplace != null)
                {
                    partToReplace.PartNumber = setValuePartItem.PartNumber; // Use PartNumber from SetValueItems
                    partToReplace.Description = setValuePartItem.Description;
                    partToReplace.Locator = setValuePartItem.Locator;
              

                    myEatonDataGrid.Items.Refresh();

                    selectedItemForReplacement = null;
                    IsReplaceMode = false;
                    MaterialCatalogControl.disableReplaceMode();
                    MaterialCatalogControl.RemoveButtonFromDataGrid("Replace"); // Remove Add button column
                    MaterialCatalogControl.ChangeBackgroundColorToDefault();
                }
            }
        }

        private void PopulatePartItemsFromJson(string bomJson)
        {
            var partsDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(bomJson);
            PartItems.Clear();

            foreach (var kvp in partsDict)
            {
                PartItems.Add(new PartItem
                {
                    PartNumber = kvp.Key,
                    Quantity = kvp.Value,
                    Locator = GenerateRandomLocator(), // or fetch an appropriate locator
                    Description = GenerateRandomDescription()
                });
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlternateDataGrid.SelectedItem is AlternatePartItem selectedAlternateBOM)
            {
                DataRow bomDetails = FetchBOMDetailsByGOItem(selectedAlternateBOM.GOItem);
                if (bomDetails != null)
                {
                    string bomJson = bomDetails["BOM"].ToString();
                    PopulatePartItemsFromJson(bomJson);
                }
                else
                {
                    MessageBox.Show("Failed to fetch BOM details from the database.");
                }
            }
            else
            {
                MessageBox.Show("No alternate BOM selected.");
            }

            // Navigate back to the main view
            MainView.Visibility = Visibility.Visible;
            AlternateBOMView.Visibility = Visibility.Collapsed;
            AlternateBomBreakdown.Visibility = Visibility.Collapsed;
        }

        private void PopulateAlternateBOMBreakdown(string bomJson)
        {
            var alternatePartsDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(bomJson);
            var currentPartsDict = PartItems.ToDictionary(p => p.PartNumber, p => p.Quantity);

            var alternateBreakdownItems = new ObservableCollection<PartItem>();

            // For parts in alternate BOM
            foreach (var kvp in alternatePartsDict)
            {
                string partQuery = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE PART_NUMBER = '{kvp.Key}'";
                DataTable partInfo = Utility.SearchLP(partQuery);

                if (partInfo != null && partInfo.Rows.Count > 0)
                {
                    DataRow row = partInfo.Rows[0];

                    var partItem = new PartItem
                    {
                        PartNumber = kvp.Key,
                        Quantity = kvp.Value,
                        Locator = row["DEFAULT_LOCATOR"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        Status = currentPartsDict.ContainsKey(kvp.Key) ? PartStatus.Normal : PartStatus.Additional
                    };

                    alternateBreakdownItems.Add(partItem);
                }
            }

            // For parts in current BOM but not in alternate BOM
            foreach (var part in currentPartsDict)
            {
                if (!alternatePartsDict.ContainsKey(part.Key))
                {
                    string partQuery = $"SELECT * FROM SHOPPING_CART_MATERIAL_VIEW WHERE PART_NUMBER = '{part.Key}'";
                    DataTable partInfo = Utility.SearchLP(partQuery);

                    if (partInfo != null && partInfo.Rows.Count > 0)
                    {
                        DataRow row = partInfo.Rows[0];

                        var missingPartItem = new PartItem
                        {
                            PartNumber = part.Key,
                            Quantity = part.Value,
                            Locator = row["DEFAULT_LOCATOR"].ToString(),
                            Description = row["DESCRIPTION"].ToString(),
                            Status = PartStatus.Missing
                        };

                        alternateBreakdownItems.Add(missingPartItem);
                    }
                }
            }

            AlternateBOMBreakdown.ItemsSource = alternateBreakdownItems;
        }


    }

    public class PartItem
    {
        public int ItemId { get; set; }
        public string OracleNumber { get; set; }
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
        public Boolean isSelectedForReplacement { get; set; }
        public string Locator { get; set; }
        public string Description { get; set; }

        public string ReceivingSubinv { get; set; }
        public string MakeBuy { get; set; }
        public string PlanningMethod { get; set; }
        public string Pegging { get; set; }
        public int LeadTime { get; set; }
        public decimal FrozenCost { get; set; }
        public PartStatus Status { get; set; }
        public string AltDescription { get; set; }
        public int SourceId { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool Explodable { get; set; }

    }

    public enum PartStatus
    {
        Normal,
        Additional,
        Missing
    }

    public class AlternatePartItem
    {
        public string GOItem { get; set; }
        public int TimesUsed { get; set; }
        public DateTime DateCreated { get; set; }
        public string Creator { get; set; }
        public double SimilarityScore { get; set; } // New property
    }

    public class CatalogItem
    {
        public string Name { get; set; }
        public List<CatalogItem> SubItems { get; set; }

        public List<string> SetValues { get; set; }

        public bool Initialized { get; set; }

        public string OracleNumber { get; set; }
        public string Description { get; set; }
        public string Locator { get; set; }

        public bool IsCategory => SubItems.Any() || SetValues.Any();

        public bool IsSetItem => !SubItems.Any() && !SetValues.Any();



        // Add these properties
        public int CatSetId { get; set; } // Represents CAT_SET_ID
        public string Family1 { get; set; }
        public string Family2 { get; set; }
        public string Family3 { get; set; }

        public bool IsSetValue { get; private set; } // Add this property

        public CatalogItem(string name, bool isSetValue = false) // Add parameter with default value
        {
            Name = name;
            SubItems = new List<CatalogItem>();
            SetValues = new List<string>();
            IsSetValue = isSetValue; // Set the property
        }
    }

    public static class ScrollViewerExtensions
    {
        public static bool IsMouseOverScrollbar(this ScrollViewer scrollViewer)
        {
            var verticalScrollbar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as System.Windows.Controls.Primitives.ScrollBar;
            var horizontalScrollbar = scrollViewer.Template.FindName("PART_HorizontalScrollBar", scrollViewer) as System.Windows.Controls.Primitives.ScrollBar;

            if (verticalScrollbar != null && verticalScrollbar.IsMouseOver)
            {
                return true;
            }

            if (horizontalScrollbar != null && horizontalScrollbar.IsMouseOver)
            {
                return true;
            }

            return false;
        }
    }

}

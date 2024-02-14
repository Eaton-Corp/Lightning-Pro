using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
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
using System.IO;
using Aspose.CAD;
using Aspose.CAD.FileFormats.Cad;
using Aspose.CAD.FileFormats.Cad.CadTables;

using Aspose.CAD.FileFormats.Cad.CadObjects;
using SharpCompress.Common;
using Aspose.CAD.ImageOptions;
using System.Diagnostics;
using Image = Aspose.CAD.Image;
using System.Reflection;
using Aspose.CAD.FileFormats.Cad.CadConsts;
using iTextSharp.text.pdf.parser;
using System.ComponentModel;
using OfficeOpenXml;

namespace LightningPRO.Views
{
    /// <summary>
    /// Interaction logic for Eaton.xaml
    /// </summary>
    public partial class Eaton : UserControl
    {     
        string Current_Tab;
        Utility.ProductGroup CurrentProduct;

        public Eaton()
        {
            InitializeComponent();

            
        }


        private void Eaton_OnLoaded(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
            Field.Text = "GO_Item";
            Current_Tab = "InDevelopment";
            ButtonColorChanges();
            LoadGrid();
            string excelFilePath = @"C:\Users\e0637402\Downloads\Nomenclature.xlsx";
            //ImportExcelToAccessUsingUtility(excelFilePath);
            //ListCADLayers();
        }

        private void LoadGrid()
        {
            try
            {
                System.Data.DataTable dt;
                string query = "";

                if (Current_Tab == "Search")
                {
                    Search();
                    return;
                }
                else
                {
                    if (CurrentProduct == Utility.ProductGroup.PRL123)
                    {
                        query =
                        "SELECT DISTINCT " +
                        "PRL123.[ID], " +
                        "PRL123.[GO_Item], " +
                        "PRL123.[GO], " +
                        "CSALabel.[ProductID], " +
                        "PRL123.[ShopOrderInterior], " +
                        "PRL123.[ShopOrderBox], " +
                        "PRL123.[ShopOrderTrim], " +
                        "PRL123.[SchedulingGroup], " +
                        "PRL123.[Customer], " +
                        "PRL123.[JobName], " +
                        "PRL123.[Quantity], " +
                        "PRL123.[EnteredDate], " +
                        "PRL123.[ReleaseDate], " +
                        "PRL123.[CommitDate], " +
                        "PRL123.[Tracking], " +
                        "PRL123.[Urgency], " +
                        "PRL123.[AMO], " +
                        "PRL123.[BoxEarly], " +
                        "PRL123.[Box Sent], " +
                        "PRL123.[SpecialCustomer], " +
                        "PRL123.[ServiceEntrance], " +
                        "PRL123.[DoubleSection], " +
                        "PRL123.[PaintedBox], " +
                        "PRL123.[RatedNeutral200], " +
                        "PRL123.[DNSB], " +
                        "PRL123.[Complete], " +
                        "PRL123.[Short], " +
                        "PRL123.[NameplateRequired], " +
                        "PRL123.[NameplateOrdered], " +
                        "PRL123.[LabelsPrinted], " +
                        "PRL123.[Notes] " +
                        "FROM [PRL123] " +
                        "INNER JOIN [CSALabel] " +
                        "ON PRL123.[GO_Item] = CSALabel.[GO_Item] " +
                        "WHERE PRL123.[Tracking]='" + Current_Tab + "' " +
                        "AND PRL123.[PageNumber] = 0";


                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRL4)
                    {
                        query = "select Distinct PRL4.[ID], PRL4.[GO_Item], PRL4.[GO], CSALabel.[ProductID], PRL4.[ShopOrderInterior], PRL4.[ShopOrderBox], PRL4.[ShopOrderTrim], PRL4.[SchedulingGroup], PRL4.[JobName], PRL4.[Customer], PRL4.[Quantity], PRL4.[EnteredDate], PRL4.[ReleaseDate], PRL4.[CommitDate], PRL4.[Tracking], PRL4.[Urgency], PRL4.[AMO], PRL4.[SpecialCustomer], PRL4.[ServiceEntrance], PRL4.[PaintedBox], PRL4.[RatedNeutral200], PRL4.[DoorOverDist], PRL4.[DoorInDoor], PRL4.[DNSB], PRL4.[Complete], PRL4.[Short], PRL4.[NameplateRequired], PRL4.[NameplateOrdered], PRL4.[LabelsPrinted], PRL4.[BoxEarly], PRL4.[BoxSent], PRL4.[Notes] from [PRL4] inner join [CSALabel] on PRL4.[GO_Item] = CSALabel.[GO_Item] where PRL4.[Tracking]='" + Current_Tab + "' and PRL4.[PageNumber] = 0";
                    }
                    else if (CurrentProduct == Utility.ProductGroup.PRLCS)
                    {
                        query = "select Distinct PRLCS.[ID], PRLCS.[GO_Item], PRLCS.[GO], CSALabelPRLCS.[ProductID], PRLCS.[ShopOrderInterior], PRLCS.[ShopOrderBox], PRLCS.[ShopOrderTrim], PRLCS.[SchedulingGroup], PRLCS.[JobName], PRLCS.[Customer], PRLCS.[Quantity], PRLCS.[EnteredDate], PRLCS.[ReleaseDate], PRLCS.[CommitDate], PRLCS.[Tracking], PRLCS.[Urgency], PRLCS.[AMO], PRLCS.[SpecialCustomer], PRLCS.[IncLocLeft], PRLCS.[IncLocRight], PRLCS.[CrossBus], PRLCS.[OpenBottom], PRLCS.[ExtendedTop], PRLCS.[PaintedBox], PRLCS.[ThirtyDeepEnclosure], PRLCS.[DNSB], PRLCS.[Complete], PRLCS.[Short], PRLCS.[NameplateRequired], PRLCS.[NameplateOrdered], PRLCS.[LabelsPrinted], PRLCS.[Notes] from [PRLCS] inner join [CSALabelPRLCS] on PRLCS.[GO_Item] = CSALabelPRLCS.[GO_Item] where PRLCS.[Tracking]='" + Current_Tab + "' and PRLCS.[PageNumber] = 0";
                    }
                }
                dt = Utility.SearchLP(query);
                dg.ItemsSource = dt.DefaultView;
                HideFullNotesColoumn();
            }
            catch
            {
               MessageBox.Show("An Error Occurred Trying To Access LPdatabase");
            }
        }


        private void Development_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "InDevelopment";
            LoadGrid();
            ButtonColorChanges();
        }

        
        private void MI_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "MIComplete";
            LoadGrid();
            ButtonColorChanges();
        }

        
        private void Production_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Production";
            LoadGrid();
            ButtonColorChanges();
        }

        
        private void Shipping_Clicked(object sender, RoutedEventArgs e)
        {
            Current_Tab = "Shipping";
            LoadGrid();
            ButtonColorChanges();
        }


        public static void ImportExcelToAccessUsingUtility(string excelFilePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // Set the license context for EPPlus

            // Read the Excel file
            using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assumes data is in the first worksheet
                int totalRows = worksheet.Dimension.End.Row;

                for (int row = 2; row <= totalRows; row++) // Start at row 2 to skip headers
                {
                    // Assuming the Excel columns are structured as per your example
                    string classId = worksheet.Cells[row, 1].Text.Trim();
                    string className = worksheet.Cells[row, 2].Text.Trim();

                    // Remove the number and the hyphen (e.g., "04 - " becomes "")
                    className = System.Text.RegularExpressions.Regex.Replace(className, @"^\d+\s-\s", "");

                    string discCbType = worksheet.Cells[row, 4].Text.Trim();
                    string reversingNonReversing = worksheet.Cells[row, 5].Text.Trim();
                    string type = worksheet.Cells[row, 6].Text.Trim();
                    string classCode = worksheet.Cells[row, 7].Text.Trim();

                    string attrValue = $"{classId}: {className} (DISC/CB TYPE: {discCbType}, REVERSING/NON-REVERSING: {reversingNonReversing}, TYPE: {type}, CLASS CODE: {classCode})";

                    string query = $"INSERT INTO TBL_ATTRIBUTE_OPTIONS (ATTR_ID, ATTR_VALUE, Disabled) VALUES (64, '{attrValue.Replace("'", "''")}', False)";

                    MessageBox.Show(query);
                    Utility.ExecuteNonQueryLP(query);
                }
            }
        }



        public CadImage UpdateTextInCAD(CadImage cd, string searchText, string updateText)
        {
            // Load an existing DWG file
            using (CadImage cadImage = cd)
            {
                // Go through each entity in the CAD image
                foreach (var entity in cadImage.Entities)
                {
                    // Check if the entity is a CAD text
                    if (entity is CadText)
                    {
                        CadText textEntity = (CadText)entity;

                        // Check if the text value matches the search text
                        if (textEntity.DefaultValue == searchText)
                        {
                            // Update text value
                            textEntity.DefaultValue = updateText;
                        }
                    }
                    else if (entity is CadMText)
                    {
                        CadMText mTextEntity = (CadMText)entity;

                        // Check if the text value contains the search text
                        if (mTextEntity.Text.Contains(searchText))
                        {
                            // Update text value
                            mTextEntity.Text = mTextEntity.Text.Replace(searchText, updateText);
                        }
                    }
                }

            }

            return cd;
        }

        public void ConvertDwgToPng(string dwgFilePath, string outputDirectory)
        {
            try
            {
                // Load the DWG file
                using (Image image = Image.Load(dwgFilePath))
                {
                    // Create an instance of CadRasterizationOptions and set its properties
                    CadRasterizationOptions rasterizationOptions = new CadRasterizationOptions
                    {
                        // Set page width & height
                        PageWidth = 1200,
                        PageHeight = 1200,

                        // Additional options can be set here if needed
                        DrawType = CadDrawTypeMode.UseObjectColor
                    };

                    // Create an instance of PngOptions for the resultant image
                    PngOptions pngOptions = new PngOptions
                    {
                        VectorRasterizationOptions = rasterizationOptions
                    };

                    // Define the output file path
                    string outputFilePath = System.IO.Path.Combine(outputDirectory, "testpic1.png");

                    // Save the resultant image
                    image.Save(outputFilePath, pngOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during conversion: " + ex.Message);
            }
        }

        public void RemoveByLayerName(CadImage cadImage)
        {
            CadEntityBase[] entities = cadImage.Entities.ToArray();

            // List of layer names to keep
            List<string> layerNamesToKeep = new List<string>
            {
                "2POLE",
                "HOA WITHOUT OL",
                "RED RUN",
                "TRANSFORMER (L-L)",
                "1NO-1NC (C1)",
                "APPROVAL",
                "GENERAL INFORMATION",
                "_MULTI_WIRE",
                "0",
                "AM_4",
                "AM_VIEWS",
                "BLK_12_THHN",
                "CONSTRUCTION",
                "Customer Information",
                "Defpoints",
                "DESC",
                "DESCCHILD",
                "Dimension (ANSI)",
                "Disclaimer",
                "FMT_ATT",
                "FORMAT",
                "FUSE TABLE",
                "LINK",
                "LOC",
                "LOCBOX",
                "MISC",
                "PDF_Geometry",
                "PDF_Solid Fills",
                "PDF_Text",
                "POS",
                "RED_14_THHN",
                "Symbol (ANSI)",
                "SYMS",
                "TAGS",
                "TERMS",
                "Visible (ANSI)",
                "Visible Narrow (ANSI)",
                "WHT_12_THHN",
                "WIRECOPY",
                "WIREFIXED",
                "WIRENO",
                "WIREREF",
                "WIRES",
                "XREF",
                "XREFCHILD",
                "YEL_14_THHN"
            };

            // Temporary list to store layer names (for debugging or logging)
            List<string> layerNames = new List<string>();

            foreach (CadEntityBase baseEntity in entities)
            {
                string trimmedLayerName = baseEntity.LayerName.Trim();
                layerNames.Add(trimmedLayerName);

                if (baseEntity.TypeName != CadEntityTypeName.TEXT)
                {
                    // Check if the LayerName exactly matches any of the specified strings to keep
                    bool shouldKeep = layerNamesToKeep.Any(name => trimmedLayerName.Equals(name.Trim()));

                    if (!shouldKeep)
                    {
                        // If the LayerName does not exactly match any of the specified strings, remove the entity
                        cadImage.RemoveEntity(baseEntity);
                    }
                }
            }

            // Optionally, write the layer names to a file for reference
            File.AppendAllLines("C:\\Users\\e0637402\\Downloads\\names.txt", layerNames);
        }



        private void ListCADLayers()
        {
            string filePathDWG = @"C:\Users\e0637402\Downloads\BASICTEMPLATE-1.dwg";
            string filePathFinish2 = @"C:\Users\e0637402\Downloads\TestBigFile8.pdf";

            Stopwatch stopWatch = new Stopwatch();

            try
            {
                using (CadImage cadImage = (CadImage)Image.Load(filePathDWG))
                {

                    RemoveByLayerName(cadImage);
                   
                    CadRasterizationOptions rasterizationOptions = new CadRasterizationOptions();
                    rasterizationOptions.DrawType = CadDrawTypeMode.UseObjectColor;

                    CadLayersList newList = cadImage.Layers;

                    

                    PdfOptions pdfOptions = new PdfOptions();
                    pdfOptions.VectorRasterizationOptions = rasterizationOptions;

                    cadImage.Save(filePathFinish2, pdfOptions);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        // changes the button colour to a darker color allowing the user to know which tab they are currently on 
        private void ButtonColorChanges()
        {
            if (Current_Tab.Equals("Shipping"))
            {
                Shipping.Background = Brushes.DarkBlue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Production"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.DarkBlue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("MIComplete"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.DarkBlue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.LightGray;
                
            }
            else if (Current_Tab.Equals("InDevelopment"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.DarkBlue;
                SearchButton.Background = Brushes.LightGray;
            }
            else if (Current_Tab.Equals("Search"))
            {
                Shipping.Background = Brushes.Blue;
                Production.Background = Brushes.Blue;
                MIComplete.Background = Brushes.Blue;
                Development.Background = Brushes.Blue;
                SearchButton.Background = Brushes.Gray;
            }
        }


        private void Open_Records(object sender, RoutedEventArgs e)
        {
            CSArecords sf = new CSArecords();
            sf.Show();
        }


        private void Search()
        {
            Current_Tab = "Search";
            System.Data.DataTable dt;
            string query = Utility.SearchQueryGenerator(CurrentProduct, Field.Text, SearchBox.Text);
            dt = Utility.SearchLP(query);
            dg.ItemsSource = dt.DefaultView;
            HideFullNotesColoumn();
            ButtonColorChanges();
        }


        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void KeyDownClick(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void PRL123_Click(object sender, RoutedEventArgs e)
        {
            PRL123_Set();
        }

        private void PRL4_Click(object sender, RoutedEventArgs e)
        {
            PRL4_Set();   
        }

        private void PRLCS_Click(object sender, RoutedEventArgs e)
        {
            PRLCS_Set();
        }

        private void PRL123_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL123;
            PWL123.Background = Brushes.DarkBlue;
            PWL4.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRL4_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRL4;
            PWL4.Background = Brushes.DarkBlue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.Blue;
            LoadGrid();
        }

        private void PRLCS_Set()
        {
            CurrentProduct = Utility.ProductGroup.PRLCS;
            PWL4.Background = Brushes.Blue;
            PWL123.Background = Brushes.Blue;
            PWLCS.Background = Brushes.DarkBlue;
            LoadGrid();
        }

        private void HideFullNotesColoumn() 
        {
            //Get the columns collection
            ObservableCollection<DataGridColumn> columns = dg.Columns;

            //set the visibility for end Notes coloumn to hidden
            foreach (DataGridColumn col in columns)
            {
                switch (col.Header.ToString())
                {
                    case "Notes":
                        col.Visibility = System.Windows.Visibility.Hidden;
                        break;
                }
            }
        }

        private void DG_Loaded(object sender, RoutedEventArgs e)
        {
            HideFullNotesColoumn();
        }

    }
}

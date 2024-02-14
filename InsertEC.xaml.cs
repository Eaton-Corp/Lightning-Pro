using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Globalization;
using System.Collections.ObjectModel;
using Aspose.CAD.FileFormats.Cad;
using Aspose.CAD.ImageOptions;
using System.Diagnostics;
using Aspose.CAD.FileFormats.Cad.CadConsts;
using Aspose.CAD.FileFormats.Cad.CadObjects;
using Image = Aspose.CAD.Image;
using Aspose.CAD;
using SharpCompress.Common;
using MaterialDesignThemes.Wpf;
using System.Xml.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using System.Runtime.InteropServices;
using System.Threading;
using Aspose.CAD.FileFormats.Collada.FileParser.Elements;
using System.IO;
using Org.BouncyCastle.Asn1.Cms;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wpf.Ui.Mvvm.Interfaces;
using System.Xml.Serialization;
using Wpf.Ui.Animations;
using Syncfusion.UI.Xaml.Diagram.Serializer;
using Syncfusion.Windows.Shared;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.GraphicsInterface;
using Org.BouncyCastle.Crypto.Macs;

namespace LightningPRO
{
    public partial class InsertEC : Window
    {
        public List<string> MyComboBoxItems { get; set; }

        public List<string> CatalogNumber { get; set; }

        public string currentGO;

        public string currentCatalog;

        public ECNomenclature currentNomenclature;

        //public ECOrderInfo currentOrderInformation;

        public List<string> currentLayers;

        public List<ECRelevantXMLValues> parsedXML;

        private readonly ECInsertViewModel _viewModel;


        //CONSTANTS
        private const string GtoObjectElementName = "Object";
        private const string ConfigurationDocumentElementName = "ConfigurationDocument";

        public InsertEC()
        {
            InitializeComponent();
            this.Loaded += InsertEC_Loaded;
            DataContext = this;
            _viewModel = new ECInsertViewModel();
            DataContext = _viewModel;

        }

        private void InsertEC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxItems("EC_VERSION", "EC_Version_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_DESIGN", "EC_Design_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_CLASS", "EC_Class_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_SIZE", "EC_Size_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_ENCL", "EC_Encl_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_TRANS", "EC_Trans_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_CC", "EC_CC_Selector"); // Load items into the EC_DESIGN ComboBox
            LoadComboBoxItems("EC_DISC", "EC_Disc_Selector"); // Load items into the EC_DESIGN ComboBox



        var customPartItems = new ObservableCollection<PartItem>
            {
            };


            string orderSpecsJson = "{\"Type\": \"IEC\", \"Class\" : \"Combo\", \"Size\" : \"G\"}";
            BomCart.LoadCustomPartItems(customPartItems, orderSpecsJson);
        }


        [CommandMethod("ConnectToAcad")]
        public static void ConnectToAcad()
        {

            AcadApplication acAppComObj = null;
            const string strProgId = "AutoCAD.Application.22";

            // Get a running instance of AutoCAD
            try
            {
                acAppComObj = (AcadApplication)Marshal.GetActiveObject(strProgId);
            }
            catch // An error occurs if no instance is running
            {
                try
                {
                    // Create a new instance of AutoCAD
                    acAppComObj = (AcadApplication)Activator.CreateInstance(System.Type.GetTypeFromProgID(strProgId), true);
                }
                catch
                {
                    // If an instance of AutoCAD is not created then message and exit
                    MessageBox.Show("Instance of 'AutoCAD.Application'" +
                                                         " could not be created.");

                    return;
                }
            }

            // Display the application and return the name and version
            acAppComObj.Visible = true;//originally 

            // Get the active document
            AcadDocument acDocComObj;

            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            acAppComObj.ActiveDocument.SendCommand("(command \"NETLOAD\"" + @"""C:\\Users\\e0637402\\source\\repos\\ClassLibrary\\ClassLibrary\\bin\\Debug\\ClassLibrary.dll"") ");
            acAppComObj.ActiveDocument.SendCommand("OpenDrawing ");
            acAppComObj.ActiveDocument.SendCommand("TurnLayerOn ");
            acAppComObj.ActiveDocument.SendCommand("EditAttr ");
            acAppComObj.ActiveDocument.SendCommand("ChangeGeneralInformationAttribute ");
            acAppComObj.ActiveDocument.SendCommand("SaveActiveDrawing ");

            // Set the output file path
            string outputFilePath = @"""C:\\Users\\e0637402\\Downloads\\output45.pdf""";

            //string plotCommand = $"(command \"_.-PLOT\" \"\" \"Model\" \"\" \"DWG To PDF.pc3\" {outputFilePath} \"N\" \"Y\") ";

            string plotCommand = $"(command \"_.-PLOT\" \"Yes\" \"Model\" \"DWG To PDF.pc3\" \"ISO full bleed A4 (297.00 x 210.00 MM)\" \"Inches\" \"Landscape\" \"No\" \"Window\" \"\" \"\" \"Fit\" \"Center\" \"Yes\" \"monochrome.ctb\" \"No\" \"\" {outputFilePath} \"N\" \"Y\") ";

            acAppComObj.ActiveDocument.SendCommand(plotCommand);

        }
        // --> P1-N: What layers  PN+1 -> M: Fill in the layers
        //pn+1: CodnitionsSet = if layers contains "General information" --> [#: noOfUnits, NEMA/IEC: ]



        private async void generateClicked(object sender, RoutedEventArgs e)
        {

            List<attributeChanges> lightingContactorJob1 = new List<attributeChanges>
            {
                new attributeChanges
                {
                    blockName = "GENERAL INFORMATION",
                    attributes = new List<attributeValue>
                    {
                        new attributeValue { tag = "#", prompt = "QUANTITY", value = currentNomenclature.noOfUnits },
                        new attributeValue { tag = "NEMA/IEC", prompt = "SELECT NEMA OR IEC", value = currentNomenclature.EC_NOM_Type },
                        new attributeValue { tag = "SIZE", prompt = "CONTACTOR SIZE", value = currentNomenclature.EC_NOM_Size },
                        new attributeValue { tag = "MV", prompt = "MAIN VOLTS", value = currentNomenclature.mainVolts },
                        new attributeValue { tag = "CV", prompt = "CONTROL VOLTS", value = currentNomenclature.controlVolts },
                        new attributeValue { tag = "HP/A", prompt = "SELECT HP OR A", value = "HP/A"},
                        new attributeValue { tag = "HP/A_VALUE", prompt = "HORSEPOWER OR AMPERAGE", value = currentNomenclature.horsepower },
                        new attributeValue { tag = "KA", prompt = "FAULT CAPACITY", value = currentNomenclature.EC_Info_Fault_Cap },
                        new attributeValue { tag = "TYPE", prompt = "ENCLOSURE", value = currentNomenclature.enclosure }
                    }
                },
                new attributeChanges 
                {
                    blockName = "COEFtBL",
                    attributes = new List<attributeValue>
                    {
                        new attributeValue { tag = "[REF]", prompt = "DRAWING REVISION", value = "LP-REV-1" },
                        new attributeValue { tag = "[DWG_NO]", prompt = "DRAWING NUMBER", value = "420" },
                        new attributeValue { tag = "[ORIG_BY]", prompt = "ORIGINAL BY", value = "Lightning PRO" },
                        new attributeValue { tag = "[TITLE1]", prompt = "TITLE LINE 1", value = "" },
                        new attributeValue { tag = "[TITLE2]", prompt = "TITLE LINE 2", value = "" },
                        new attributeValue { tag = "[TAG]", prompt = "TAG", value = "TAGS" },
                        new attributeValue { tag = "[DWG_TYPE]", prompt = "DRAWING TYPE", value = "LP GEN" },
                        new attributeValue { tag = "[GO]", prompt = "GENERAL ORDER - ITEM", value = currentNomenclature.goItem },
                        new attributeValue { tag = "[SO]", prompt = "SHOP ORDER NUMBER", value = "**Shop Order number**" },
                        new attributeValue { tag = "[SHEET]", prompt = "SHEET", value = "1" },
                        new attributeValue { tag = "[PROD_LINE]", prompt = "PRODUCT LINE", value = "EC" },
                        new attributeValue { tag = "[PLOT_SIZE]", prompt = "DWG SIZE", value = "" },
                        new attributeValue { tag = "[APP_DATE]", prompt = "DATE APPROVED", value = "10/10/23" },
                        new attributeValue { tag = "[INFO5]", prompt = "LINE 5 CUSTOMER INFORMATION:", value = "18" },
                        new attributeValue { tag = "[INFO4]", prompt = "LINE 4 CUSTOMER INFORMATION:", value = "19" },
                        new attributeValue { tag = "[INFO3]", prompt = "LINE 3 CUSTOMER INFORMATION:", value = "20" },
                        new attributeValue { tag = "[INFO2]", prompt = "LINE 2 CUSTOMER INFORMATION:", value = "21" },
                        new attributeValue { tag = "[INFO1]", prompt = "LINE 1 CUSTOMER INFORMATION:", value = "22" },
                        new attributeValue { tag = "[FED_ID]", prompt = "CAGE NO", value = "Fed id" },
                        new attributeValue { tag = "[LOCATION]", prompt = "LOCATION", value = "Mississauga" }, //pull missiisauga location
                        new attributeValue { tag = "[SHEET_MAX]", prompt = "SHEET MAX", value = "1" },
                        new attributeValue { tag = "[CUST_REF]", prompt = "REFERENCE", value = "Customer" },
                        new attributeValue { tag = "[EQUIP_TYPE]", prompt = "EQUIPMENT TYPE", value = "Equip type" },

                    }
                },
                new attributeChanges
                {
                    blockName = "SPECS-COMBO",
                    attributes = new List<attributeValue>
                    {
                        new attributeValue { tag = "SIZE", prompt = "QUANTITY", value = "30 A" }, //need to obtain this from partnumber = C351KD71		
                        new attributeValue { tag = "CLASS", prompt = "SELECT NEMA OR IEC", value = "CLASS J" },
                    }
                },
            };

            string jsonData = JsonConvert.SerializeObject(lightingContactorJob1);
            File.WriteAllText(@"C:\\Users\\e0637402\\Downloads\textInfo.txt", jsonData);

            List<string> layerNamesToKeep = currentLayers;

            string filePath = @"C:\Users\e0637402\Downloads\Layers.txt";
            File.WriteAllLines(filePath, layerNamesToKeep);

            //Specs transformer and specs combo are only enabled when combo and transformer are on

            //string fileName = "OG" + Guid.NewGuid().ToString();
            //string filePathPNG = @"C:\\Users\\e0637402\\Downloads\\firstphoto.png";

            // Show the loading indicator
            ButtonProgressAssist.SetIsIndicatorVisible((Button)sender, true);
            await Task.Run(() =>
            {
                ConnectToAcad();
            });

            // Hide the loading indicator
            string outputFilePath = @"C:\\Users\\e0637402\\Downloads\\output45.pdf";
            ButtonProgressAssist.SetIsIndicatorVisible((Button)sender, false);
            BitmapSource NewImage = Utility.ConvertPDFToImage(outputFilePath)[0];     //first page only
            List<BitmapImage> imgs = new List<BitmapImage>
            {
                (BitmapImage)NewImage
            };
            ImageView.LoadImages(imgs, 0);


        }

        private void LoadComboBoxItems(string comboBoxName, string propertyName)
        {
            // Find the ComboBox by name
            ComboBox comboBox = this.FindName(comboBoxName) as ComboBox;
            if (comboBox == null)
            {
                // Handle the case where the ComboBox is not found
                return;
            }

            // Fetch items for the ComboBox
            List<string> comboBoxItems = new List<string>();

            string safeComboBoxName = comboBoxName.Replace("'", "''");
            string query = $@"
            SELECT ao.ATTR_VALUE 
            FROM TBL_ATTRIBUTES_LIST al
            INNER JOIN TBL_ATTRIBUTE_OPTIONS ao ON al.ATTR_ID = ao.ATTR_ID
            WHERE al.ATTR_NAME = '{propertyName}'";

            System.Data.DataTable results = Utility.SearchLP(query);

            foreach (DataRow row in results.Rows)
            {
                comboBoxItems.Add(row["ATTR_VALUE"].ToString());
            }

            // Set the ComboBox's ItemsSource
            comboBox.ItemsSource = comboBoxItems;
        }

        private void uploadXMLClicked(object sender, RoutedEventArgs e)
        {
            string xmlFilePath = Utility.SelectXMLFile();
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                parsedXML = SerializeEC(xmlFilePath);
            }
            else
            {
                MessageBox.Show("No file was selected. Please select a file.");
            }
            var XMLWithFLA = getFLAValues(parsedXML[0]);
            var XMLWithPowerWire = getPowerWireValues(XMLWithFLA);


            _viewModel.loadParsedXML(XMLWithPowerWire);
            ECNomenclature nom = getNomenclatureBreakdown(_viewModel.Nomenclature);



            MessageBox.Show("XML Parsing Finished");
        }

        private ECNomenclature getNomenclatureBreakdown(ECNomenclature ecValues)
        {
            var dataStructure = ConvertStructureToAnonymousObject(ecValues);
            var (setValue, setValueFlags, updatedStructure) = Utility.RunDataObjectLogic(dataStructure, "EC", "UI", "NOM", 0);
            UpdateNomenclatureObjectWithSetValues(updatedStructure, ecValues);
            return ecValues;

        }

        public void UpdateNomenclatureObjectWithSetValues(object anonymousObject, ECNomenclature structure)
        {
            var updatedValues = JObject.FromObject(anonymousObject);

            foreach (var property in structure.GetType().GetProperties())
            {
                if (updatedValues.TryGetValue(property.Name, out JToken value))
                {
                    property.SetValue(structure, value.ToObject(property.PropertyType));
                }
            }
        }

        public object ConvertStructureToAnonymousObject(ECNomenclature structure)
        {
            string json = JsonConvert.SerializeObject(structure);
            var anonymousObject = JObject.Parse(json);

            return anonymousObject;
        }


        public void UpdateStructureObjectWithSetValues(object anonymousObject, ECRelevantXMLValues structure)
        {
            var updatedValues = JObject.FromObject(anonymousObject);

            foreach (var property in structure.GetType().GetProperties())
            {
                if (updatedValues.TryGetValue(property.Name, out JToken value))
                {
                    property.SetValue(structure, value.ToObject(property.PropertyType));
                }
            }
        }

        private ECRelevantXMLValues getFLAValues(ECRelevantXMLValues ecValues)
        {
            
            var dataStructure = ConvertStructureToAnonymousObject(ecValues);
            var (setValue, setValueFlags, updatedStructure) = Utility.RunDataObjectLogic(dataStructure, "EC", "UI", "FLA", 0);
            UpdateStructureObjectWithSetValues(updatedStructure, ecValues);
            return ecValues;

        }

        public object ConvertStructureToAnonymousObject(ECRelevantXMLValues structure)
        {
            string json = JsonConvert.SerializeObject(structure);
            var anonymousObject = JObject.Parse(json);

            return anonymousObject;
        }

        private ECRelevantXMLValues getPowerWireValues(ECRelevantXMLValues ecValues)
        {

            var dataStructure = ConvertStructureToAnonymousObject(ecValues);
            var (setValue, setValueFlags, updatedStructure) = Utility.RunDataObjectLogic(dataStructure, "EC", "UI", "POWER_WIRE", 0);
            UpdateStructureObjectWithSetValues(updatedStructure, ecValues);
            return ecValues;

        }


        public static List<ECRelevantXMLValues> SerializeEC(string xmlFilePath)
        {
            string xml = File.ReadAllText(xmlFilePath);
            var root = XDocument.Parse(xml).Root.Name.LocalName;

            if (root == GtoObjectElementName) return ProcessGtoObject(xml);//Detailed XML is parsed
            else if (root == ConfigurationDocumentElementName) return ProcessConfigurationDocument(xml);//standard quickprint is parsed
            else throw new ArgumentException("Unknown XML document type.");
        }

        private static List<ECRelevantXMLValues> ProcessGtoObject(string xml)
        {
            var serializer = new XmlSerializer(typeof(GTOObject));
            using (var reader = new StringReader(xml))
            {
                var configDocument = (GTOObject)serializer.Deserialize(reader);
                var ecValues = ECRelevantXMLValues.ParseECXML(configDocument.Attributes);
                return new List<ECRelevantXMLValues> { ecValues };
            }
        }

        private static List<ECRelevantXMLValues> ProcessConfigurationDocument(string xml)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationDocument));
            using (var reader = new StringReader(xml))
            {
                var configDocument = (ConfigurationDocument)serializer.Deserialize(reader);
                var ecRelevantXMLValuesList = new List<ECRelevantXMLValues>();
                foreach (var bmConfiguredLineItem in configDocument.BMConfiguredItems.BMConfiguredLineItemList)
                {
                    var ecValues = ECRelevantXMLValues.ParseECXML(bmConfiguredLineItem);
                    ecRelevantXMLValuesList.Add(ecValues);
                }
                return ecRelevantXMLValuesList;
            }
        }

        /* Loads a fake job
         * 
         * 
         * public void loadJob(ECNomenclature ecNom, ECOrderInfo orderinfo, ObservableCollection<PartItem> listOfParts, List<string> modificationCodes, List<string> Layers) {
            setOrderInformation(orderinfo);
            setNomenclature(ecNom);
            SetModificationCodes(modificationCodes);
            string orderSpecsJson = "{\"Type\": \"IEC\", \"Class\" : \"Combo\", \"Size\" : \"G\"}";
            BomCart.LoadCustomPartItems(listOfParts, orderSpecsJson);
            currentGO = "CSMU129570";
            currentNomenclature = ecNom;
            currentOrderInformation = orderinfo;
            currentLayers = Layers;
            ViewMods.Content = currentCatalog.Substring(currentCatalog.IndexOf('-') + 1);
        }*/

        public void setNomenclature(ECNomenclature nomenclature)
        {
            _viewModel.SetNomenclature(nomenclature);
        }

    }
        public class ECInsertViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private ECNomenclature _nomenclature;

            
            public ECNomenclature Nomenclature
            {
                get { return _nomenclature; }
                set
                {
                    _nomenclature = value;
                    OnPropertyChanged(nameof(Nomenclature));
                }
            }


            public void loadParsedXML(ECRelevantXMLValues xml)
            {
                Nomenclature.goItem = xml.GoNumber + "-" + xml.ItemNumber;
                Nomenclature.catalogNumber = xml.CatalogNumber;
                Nomenclature.noOfUnits = xml.Quantity.ToString();
                Nomenclature.horsepower = xml.HorsePowerRating.ToString() + " HP";
                Nomenclature.mainVolts = xml.MotorVoltageText;
                Nomenclature.controlVolts = xml.CoilVoltageText;
                Nomenclature.phase = xml.PH.ToString();
                Nomenclature.enclosure = xml.BoxSize;
                Nomenclature.type = xml.EnclosureType;
                Nomenclature.inputFreq = "60 Hz";
                Nomenclature.controlWire = "16 AWG";
                Nomenclature.EC_Info_Resultant_Size = xml.ResultantSizeText;
                Nomenclature.powerWire = xml.PowerWire;
                Nomenclature.designation = xml.Designation;

                //TODO: Please add code to when the catalog does not have a "-"
                string catalogNum = Nomenclature.catalogNumber.Split('-')[0];
                string modificationCodes = Nomenclature.catalogNumber.Split('-')[1];
                Nomenclature.EC_Version_Selector = catalogNum.Substring(0, 2);
                Nomenclature.EC_Design_Selector = catalogNum.Substring(2, 1);
                Nomenclature.EC_Class_Selector = catalogNum.Substring(3, 2);
                Nomenclature.EC_Size_Selector = catalogNum.Substring(5, 1);
                Nomenclature.EC_Encl_Selector = catalogNum.Substring(6, 1);
                Nomenclature.EC_Trans_Selector = catalogNum.Length == 11 ? catalogNum.Substring(7, 2) : catalogNum.Substring(7, 1);
                Nomenclature.EC_CC_Selector = catalogNum.Length == 11 ? catalogNum.Substring(9, 1) : catalogNum.Substring(8, 1);
                Nomenclature.EC_Disc_Selector = catalogNum.Length == 11 ? catalogNum.Substring(10, 1) : catalogNum.Substring(9, 1);
                Nomenclature.EC_NOM_ModificationCodes = ParseModCodes(modificationCodes);
                Nomenclature.EC_NOM_Modification_Codes_Detailed = GetDisplayNames(Nomenclature.EC_NOM_ModificationCodes);
                Nomenclature.EC_Info_Modification_Codes = modificationCodes;

            }

            public ObservableCollection<string> GetDisplayNames(string modCodes)
            {
                // Split the mod codes into an array
                string[] codes = modCodes.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                // Initialize the collection to hold the display names
                ObservableCollection<string> displayNames = new ObservableCollection<string>();

                // Loop through each code
                foreach (string code in codes)
                {
                    // Formulate the query
                    string query = $"SELECT DisplayName FROM TBL_ATTRIBUTE_OPTIONS WHERE ATTR_VALUE = '{code}'";

                    // Query the database
                    System.Data.DataTable results = Utility.SearchLP(query);

                    // If a result is found, add the display name to the collection
                    if (results.Rows.Count > 0)
                    {
                        displayNames.Add(results.Rows[0]["DisplayName"].ToString());
                    }
                }

                // Return the collection of display names
                return displayNames;
            }

            public static string ParseModCodes(string modCodes)
            {
                string pattern = @"(L29[ABCDEFG]|([A-Z]\d+\/[A-Z])|[A-Z]\d+|[A-Z])";
                // Find matches
                MatchCollection matches = Regex.Matches(modCodes, pattern);
                // Build the result string
                string result = "";
                foreach (Match match in matches)
                {
                    result += "|" + match.Value + "|";
                }

                return result;
            }

            public void SetNomenclature(ECNomenclature nomenclature)
            {
                Nomenclature = nomenclature;
            }

            public ECInsertViewModel()
            {
                Nomenclature = new ECNomenclature();
            }

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    public class ECNomenclature : INotifyPropertyChanged
    {
            public event PropertyChangedEventHandler PropertyChanged;


            private string _catalogNumber;
            private string _goItem;
            private string _designation;
            private string _type;
            private string _mainVolts;
            private string _controlVolts;
            private string _phase;
            private string _controlWire;
            private string _horsepower;
            private string _enclosure;
            private string _powerWire;
            private string _noOfUnits;
            private string _wire;
            private string _EC_Info_Fault_Cap;
            private string _inputFreq;
            private ObservableCollection<string> _EC_NOM_Modification_Codes_Detailed;
            private string _EC_Info_Modification_Codes;
            public ObservableCollection<string> EC_NOM_Modification_Codes_Detailed
            {
                get { return _EC_NOM_Modification_Codes_Detailed; }
                set
                {
                    _EC_NOM_Modification_Codes_Detailed = value;
                    OnPropertyChanged(nameof(EC_NOM_Modification_Codes_Detailed));
                }
            }

            public string EC_Info_Modification_Codes
        {
                get { return _EC_Info_Modification_Codes; }
                set
                {
                    _EC_Info_Modification_Codes = value;
                    OnPropertyChanged(nameof(EC_Info_Modification_Codes));
                }
            }

            public string EC_Info_Resultant_Size { get; set; }
            

            private string _EC_NOM_Type;
            private string _EC_NOM_Design;
            private string _EC_NOM_Class;
            private string _EC_NOM_Size;
            private string _EC_NOM_EnclType;
            private string _EC_NOM_CtrlXfrmr;
            private string _EC_NOM_MainVoltage;
            private string _EC_NOM_CoilVoltage;
            private string _EC_NOM_CoverControl;
            private string _EC_NOM_NumberOfPoles;
            private string _EC_NOM_CbType;
            private string _EC_NOM_CbRating;
            private string _EC_NOM_ModificationCodes;
            private string _EC_Disc_Selector;
            private string _EC_CC_Selector;
            private string _EC_Trans_Selector;
            private string _EC_Encl_Selector;
            private string _EC_Size_Selector;
            private string _EC_Class_Selector;
            private string _EC_Design_Selector;
            private string _EC_Version_Selector;

            public int EC_NOM_Size_Value { get; set; }
            public int EC_NOM_MainVoltage_Value { get; set; }
            public int EC_NOM_CoilVoltage_Value { get; set; }
            public string EC_NOM_Operation { get; set; }
            public int EC_NOM_NumberOfPoles_Value { get; set; }
            public int EC_NOM_CbRating_Voltage_Value { get; set; }
            public int EC_NOM_CbRating_Amps_Value { get; set; }

            public string EC_Version_Selector
            {
                get { return _EC_Version_Selector; }
                set
                {
                    _EC_Version_Selector = value;
                    OnPropertyChanged(nameof(EC_Version_Selector));
                }
            }

            
            public string EC_Design_Selector
            {
                get { return _EC_Design_Selector; }
                set
                {
                _EC_Design_Selector = value;
                    OnPropertyChanged(nameof(EC_Design_Selector));
                }
            }

            
            public string EC_Class_Selector
            {
                get { return _EC_Class_Selector; }
                set
                {
                    _EC_Class_Selector = value;
                    OnPropertyChanged(nameof(EC_Class_Selector));
                }
            }

            public string EC_Size_Selector
            {
                get { return _EC_Size_Selector; }
                set
                {
                    _EC_Size_Selector = value;
                    OnPropertyChanged(nameof(EC_Size_Selector));
                }
            }

            public string EC_Encl_Selector
            {
                get { return _EC_Encl_Selector; }
                set
                {
                    _EC_Encl_Selector = value;
                    OnPropertyChanged(nameof(EC_Encl_Selector));
                }
            }

            
            public string EC_Trans_Selector
            {
                get { return _EC_Trans_Selector; }
                set
                {
                    _EC_Trans_Selector = value;
                    OnPropertyChanged(nameof(EC_Trans_Selector));
                }
            }

            public string EC_CC_Selector
            {
                get { return _EC_CC_Selector; }
                set
                {
                    _EC_CC_Selector = value;
                    OnPropertyChanged(nameof(EC_CC_Selector));
                }
            }

            public string EC_Disc_Selector
            {
                get { return _EC_Disc_Selector; }
                set
                {
                    _EC_Disc_Selector = value;
                    OnPropertyChanged(nameof(EC_Disc_Selector));
                }
            }

           

            public string EC_NOM_ModificationCodes
            {
                get { return _EC_NOM_ModificationCodes; }
                set
                {
                    _EC_NOM_ModificationCodes = value;
                    OnPropertyChanged(nameof(EC_NOM_ModificationCodes));
                }
            }

            public string EC_NOM_Type
            {
                get { return _EC_NOM_Type; }
                set
                {
                    _EC_NOM_Type = value;
                    OnPropertyChanged(nameof(EC_NOM_Type));
                }
            }

        
            public string EC_NOM_Design
            {
                get { return _EC_NOM_Design; }
                set
                {
                    _EC_NOM_Design = value;
                    OnPropertyChanged(nameof(EC_NOM_Design));
                }
            }

        

            public string EC_NOM_Class
        {
                get { return _EC_NOM_Class; }
                set
                {
                     _EC_NOM_Class = value;
                    OnPropertyChanged(nameof(EC_NOM_Class));
                }
            }


            public string EC_NOM_Size
            {
                get { return _EC_NOM_Size; }
                set
                {
                    _EC_NOM_Size = value;
                    OnPropertyChanged(nameof(EC_NOM_Size));
                }
            }

            public string EC_NOM_EnclType
            {
                get { return _EC_NOM_EnclType; }
                set
                {
                    _EC_NOM_EnclType = value;
                    OnPropertyChanged(nameof(EC_NOM_EnclType));
                }
            }

            public string EC_NOM_CtrlXfrmr
            {
                get { return _EC_NOM_CtrlXfrmr; }
                set
                {
                    _EC_NOM_CtrlXfrmr = value;
                    OnPropertyChanged(nameof(EC_NOM_CtrlXfrmr));
                }
            }

            public string EC_NOM_MainVoltage
            {
                get { return _EC_NOM_MainVoltage; }
                set
                {
                    _EC_NOM_MainVoltage = value;
                    OnPropertyChanged(nameof(EC_NOM_MainVoltage));
                }
            }

            public string EC_NOM_CoilVoltage
            {
                get { return _EC_NOM_CoilVoltage; }
                set
                {
                    _EC_NOM_CoilVoltage = value;
                    OnPropertyChanged(nameof(EC_NOM_CoilVoltage));
                }
            }

            public string EC_NOM_CoverControl
            {
                get { return _EC_NOM_CoverControl; }
                set
                {
                    _EC_NOM_CoverControl = value;
                    OnPropertyChanged(nameof(EC_NOM_CoverControl));
                }
            }

            public string EC_NOM_NumberOfPoles
            {
                get { return _EC_NOM_NumberOfPoles; }
                set
                {
                    _EC_NOM_NumberOfPoles = value;
                    OnPropertyChanged(nameof(EC_NOM_NumberOfPoles));
                }
            }

            public string EC_NOM_CbType
            {
                get { return _EC_NOM_CbType; }
                set
                {
                    _EC_NOM_CbType = value;
                    OnPropertyChanged(nameof(EC_NOM_CbType));
                }
            }

            public string EC_NOM_CbRating
            {
                get { return _EC_NOM_CbRating; }
                set
                {
                    _EC_NOM_CbRating = value;
                    OnPropertyChanged(nameof(EC_NOM_CbRating));
                }
            }


            //EC Order Info:

            public string catalogNumber
            {
                get { return _catalogNumber; }
                set
                {
                    _catalogNumber = value;
                    OnPropertyChanged(nameof(catalogNumber));
                }
            }

            public string goItem
            {
                get { return _goItem; }
                set
                {
                    _goItem = value;
                    OnPropertyChanged(nameof(goItem));
                }
            }

            public string designation
            {
                get { return _designation; }
                set
                {
                    _designation = value;
                    OnPropertyChanged(nameof(designation));
                }
            }

            public string type
            {
                get { return _type; }
                set
                {
                    _type = value;
                    OnPropertyChanged(nameof(type));
                }
            }

            public string mainVolts
            {
                get { return _mainVolts; }
                set
                {
                    _mainVolts = value;
                    OnPropertyChanged(nameof(mainVolts));
                }
            }

            public string controlVolts
            {
                get { return _controlVolts; }
                set
                {
                    _controlVolts = value;
                    OnPropertyChanged(nameof(controlVolts));
                }
            }

            public string phase
            {
                get { return _phase; }
                set
                {
                    _phase = value;
                    OnPropertyChanged(nameof(phase));
                }
            }

            public string controlWire
            {
                get { return _controlWire; }
                set
                {
                    _controlWire = value;
                    OnPropertyChanged(nameof(controlWire));
                }
            }

            public string horsepower
            {
                get { return _horsepower; }
                set
                {
                    _horsepower = value;
                    OnPropertyChanged(nameof(horsepower));
                }
            }

            public string enclosure
            {
                get { return _enclosure; }
                set
                {
                    _enclosure = value;
                    OnPropertyChanged(nameof(enclosure));
                }
            }

            public string powerWire
            {
                get { return _powerWire; }
                set
                {
                    _powerWire = value;
                    OnPropertyChanged(nameof(powerWire));
                }
            }

            public string noOfUnits
            {
                get { return _noOfUnits; }
                set
                {
                    _noOfUnits = value;
                    OnPropertyChanged(nameof(noOfUnits));
                }
            }

            public string wire
            {
                get { return _wire; }
                set
                {
                    _wire = value;
                    OnPropertyChanged(nameof(wire));
                }
            }

            public string EC_Info_Fault_Cap
        {
                get { return _EC_Info_Fault_Cap; }
                set
                {
                    _EC_Info_Fault_Cap = value;
                    OnPropertyChanged(nameof(EC_Info_Fault_Cap));
                }
            }

            public string inputFreq
            {
                get { return _inputFreq; }
                set
                {
                    _inputFreq = value;
                    OnPropertyChanged(nameof(inputFreq));
                }
            }




        protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class attributeChanges
        {
            public string blockName { get; set; }
            public List<attributeValue> attributes { get; set; }
        }

        public class attributeValue
        {
            public string tag;
            public string prompt;
            public string value;
        }

        public class NotEmptyValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                if (value is string str && !string.IsNullOrWhiteSpace(str))
                {
                    return ValidationResult.ValidResult;
                }
                return new ValidationResult(false, "Field cannot be empty.");
            }
        }

        public class ComboBoxSelectionConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string stringValue)
                {
                    // Find the index of the colon character
                    int colonIndex = stringValue.IndexOf(':');
                    if (colonIndex != -1)
                    {
                        // Return the substring before the colon
                        return stringValue.Substring(0, colonIndex).Trim();
                    }
                }

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return LookUpFullValue(value as string);
            }

            private string LookUpFullValue(string abbreviation)
            {
                return abbreviation;
            }
        }

        public class ECRelevantXMLValues
        {
            public string GoNumber { get; set; }
            public string ItemNumber { get; set; }
            public string Designation { get; set; }
            public string CatalogNumber { get; set; }
            public int MotorVoltage { get; set; } // keep integer and as a string but when check for seperate source may need to store as a boolean. When we see the following: 110V/120V(SEPARATE SOURCE) we need 120 and seperate source
            public Boolean SerperateSource { get; set; }
            public string MotorVoltageText { get; set; }
            public int CoilVoltageValue { get; set; } //Integer largest possible value and keep string for now 
            public string CoilVoltageText { get; set; } //full text of coil voltage
            public int PH { get; set; }//int --> if S29 modification code is present then we can convert from 3 phase to single phase //for now will set to 1 if s29 mod code is present
            public int Quantity { get; set; }//int 
            public bool Cpt { get; set; }//boolean //TODO: need to double check what CPT is and how to determine it
            public double? HorsePowerRating { get; set; }//int
            public double NAmpsFLA { get; set; } //int
            public int ResultantSizeValue { get; set; } //Convert to int
            public string ResultantSizeText { get; set; }//full text of resultant size
            public string OverloadType { get; set; } //String match BIDMAN
            public string OverloadRange { get; set; } //string
            public string Disconnect { get; set; }//string
            public int DisconnectRatingValue { get; set; }
            public string DisconnectRatingText { get; set; }
            public string EnclosureType { get; set; }//string
            public string BoxSize { get; set; }//string
            public string PowerWire { get; set; }

            public static ECRelevantXMLValues ParseECXML(Attributes item)
            {
                var values = new ECRelevantXMLValues();

                //getting major values
                foreach (var attr in item.AttrList)
                {
                    switch (attr.Name)
                    {
                        case "BM_GONumber":
                            values.GoNumber = attr.Value;
                            break;
                        case "BM_ItemNumber":
                            values.ItemNumber = attr.Value;
                            break;
                        case "BuildCatnum_cache":
                            values.CatalogNumber = attr.Value;
                            values.PH = attr.Value.Contains("S29") ? 1 : 3;
                            break;
                        case "Quantity":
                            values.Quantity = int.Parse(attr.Value);
                            break;
                        case "CPT_Enable":
                            values.Cpt = attr.Value == "Y" ? true : false;
                            break;
                        case "motorConvertion": 
                            values.MotorVoltage = int.Parse(attr.Value);
                            break;
                        case "motor_voltage":
                            values.MotorVoltageText = attr.Value;
                            values.SerperateSource = attr.Value.Contains("SEPARATE SOURCE") ? true : false;
                            break;
                        case "coil_voltage":
                            values.CoilVoltageValue = GetMaxNumber(attr.Value);
                            values.CoilVoltageText = attr.Value;
                            break;
                        case "hp":
                            values.HorsePowerRating = attr.Value == "" ? 0 : double.Parse(attr.Value);
                            break;
                        case "NAmps":
                            values.NAmpsFLA = GetNumber(attr.Value);
                            break;
                        case "resultant_size":
                            values.ResultantSizeText = "Size " + attr.Value;
                            //TODO: Need to add resultant size Value
                            break;
                        case "overload":
                            values.OverloadType = attr.Value;
                            break;
                        case "mfla":
                            values.OverloadRange = attr.Value;
                            break;
                        case "disconnect":
                            values.Disconnect = attr.Value;
                            break;
                        case "diconnect_rating":
                            values.DisconnectRatingText = attr.Value;
                            values.DisconnectRatingValue = attr.Value == "N/A" ? 0 : GetMaxNumber(attr.Value);//TODO: Need to check if the value is obtained correclty
                            break;
                        case "enclosure_type":
                            values.EnclosureType = attr.Value;
                            break;
                        case "BoxID":
                            values.BoxSize = attr.Value;
                            break;
                    }
                }
                return values;
            }

        public static int GetMaxNumber(string input)
        {
            var matches = Regex.Matches(input, @"\d+");
            var numbers = matches.Cast<Match>().Select(m => int.Parse(m.Value));
            return numbers.Max();
        }

        public static double GetNumber(string input)
        {
            var match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                return double.Parse(match.Value);
            }
            return 0; // Return a default value if no number is found
        }

        public static int GetLargestNumber(string input)
        {
            // Define the regex pattern
            string pattern = @"\d+";

            // Find matches
            MatchCollection matches = Regex.Matches(input, pattern);

            // Convert matches to integers and find the largest one
            int largestNumber = matches.Cast<Match>().Select(m => int.Parse(m.Value)).Max();

            return largestNumber;
        }

        public static ECRelevantXMLValues ParseECXML(BMConfiguredLineItem item)
            {
                var values = new ECRelevantXMLValues();

                //getting major values
                foreach (var attr in item.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "GONumber":
                            values.GoNumber = attr.Value;
                            break;
                        case "ItemNumber":
                            values.ItemNumber = attr.Value;
                            break;
                        case "Designations":
                            values.Designation = attr.Value;
                            break;
                    }
                }

                //getting leftover values
                foreach (var attr in item.BMLineItem.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "Description":
                            var description = attr.Value;
                            string[] parts = description.Split(new string[] { ", " }, StringSplitOptions.None);
                            if (description.Contains("w/CPT")) //Do not trust w/CPT --> look into C-C6 mod code
                            {
                                values.Cpt = true;
                            }
                            values.EnclosureType = parts[2];


                            string hpString = parts[3];
                            values.HorsePowerRating = null;

                            if (hpString.Trim() != "HP")
                            {
                                string[] p = hpString.Split(' ');
                                int number;
                                if (int.TryParse(p[0], out number))
                                {
                                    // If the string can be parsed to an integer, hpNumber now holds that value
                                    values.HorsePowerRating = number;
                                }
                            }
                            values.MotorVoltage = GetLargestNumber(parts[4]);
                            values.MotorVoltageText = parts[4]
;                           values.ResultantSizeText = parts[5];
                            values.CoilVoltageText = parts[6];
                            string[] disconnect = parts[7].Split(new string[] { " - " }, StringSplitOptions.None);
                            values.Disconnect = disconnect[0];
                            values.DisconnectRatingText = disconnect[1];
                            values.OverloadType = parts.Length > 12 ? parts[12] : "";
                            //TODO:
                            //- Box Size
                            break;
                        case "CatalogNumber":
                            values.CatalogNumber = attr.Value;
                            values.PH = attr.Value.Contains("S29") ? 1 : 3;
                            break;
                        case "Quantity":
                            values.Quantity = int.Parse(attr.Value);
                            break;
                    }
                }

            //FIX: Not always the first
            foreach (var attr in item.BMLineItems.BMLineItem.FirstOrDefault().Attributes)
            {
                switch (attr.Name)
                {
                    case "Description":
                        var description = attr.Value;
                        Regex regex = new Regex(@"\b\d+-\d+\b");
                        Match match = regex.Match(description);
                        if (match.Success)
                        {
                            values.OverloadRange = match.Value;
                        }
                        break;
                }
            }
                return values;
            }
        }
    }
    //=====================================================================EC DETAILED XML PARSING  CLASSES==============================================================

    [XmlRoot("Object")]
    public class GTOObject
    {
        [XmlAttribute("Class")]
        public string Class { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("clsID")]
        public string ClsID { get; set; }

        [XmlAttribute("InstanceID")]
        public string InstanceID { get; set; }

        [XmlElement("lpProduct")]
        public string LpProduct { get; set; }

        [XmlElement("Children")]
        public Children Children { get; set; }

        [XmlElement("Attributes")]
        public Attributes Attributes { get; set; }
    }

    public class Children
    {
        [XmlAttribute("Count")]
        public int Count { get; set; }

        [XmlElement("child")]
        public List<Child> ChildList { get; set; }
    }

    public class Child
    {
        [XmlAttribute("pc")]
        public string Pc { get; set; }

        [XmlElement("Object")]
        public GTOObject Object { get; set; }
    }

    public class Attributes
    {
        [XmlElement("Attr")]
        public List<Attr> AttrList { get; set; }
    }


    //=====================================================================EC XML PARSING CLASSES==============================================================
    // Note: Alot of values are left undefined because they are not needed for the XML parsing



    [XmlRoot("ConfigurationDocument")]
    public class ConfigurationDocument
    {
        [XmlElement("CalculatedPricing")]
        public CalculatedPricing CalculatedPricing { get; set; }

        [XmlElement("CustomInfo")]
        public CustomInfo CustomInfo { get; set; }

        [XmlElement("OrderInfo")]
        public OrderInfo OrderInfo { get; set; }

        [XmlElement("JobInfo")]
        public JobInfo JobInfo { get; set; }

        [XmlElement("JobNotes")]
        public string JobNotes { get; set; }

        [XmlElement("BMConfiguredItems")]
        public BMConfiguredItems BMConfiguredItems { get; set; }

    }

    public class BMConfiguredItems
    {
        [XmlElement("BMConfiguredLineItem")]
        public List<BMConfiguredLineItem> BMConfiguredLineItemList { get; set; }
    }

    public class BMConfiguredLineItem
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
        public BOMDescriptions BOMDescriptions { get; set; }
        public BMNotes BMNotes { get; set; }
        
        [XmlElement("BMLineItem")]
        public BMLineItem BMLineItem { get; set; }

        [XmlElement("BMLineItems")]
        public BMLineItems BMLineItems { get; set; }
        public PricingSummary PricingSummary { get; set; }
        public BMLayoutStructures BMLayoutStructures { get; set; }
    }

    public class BOMDescriptions
    {
        // Add properties if BOMDescriptions has attributes or elements
    }

    public class BMNotes
    {
        // Add properties if BMNotes has attributes or elements
    }

    public class BMLineItem
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class BMLineItems
    {

    [XmlElement("BMLineItem")]
    public List<BMLineItem> BMLineItem { get; set; }
    }

public class PricingSummary
    {
        [XmlElement("BMPricingItem")]
        public List<BMPricingItem> BMPricingItems { get; set; }
    }

    public class BMPricingItem
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class BMLayoutStructures
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class Attr
    {
        [XmlAttribute("N")]
        public string Name { get; set; }
        [XmlAttribute("V")]
        public string Value { get; set; }
    }

    public class CalculatedPricing
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class CustomInfo
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class OrderInfo
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }
    }

    public class JobInfo
    {
        [XmlElement("Attr")]
        public List<Attr> Attributes { get; set; }

    }




/*ECNomenclature Nom_ECL03B1D3A = new ECNomenclature
            {
                type = "LIGHTING",
                design = "L - CN35 or A202 Lighting Contactor",
                Class = "03 - Non-combo elec. held lighting contactor (CN35)",
                size = "B - 20A",
                enclType = "1 - Type 1 - General Purpose",
                ctrlXfrmr = "CPT",
                mainVoltage = "D - 600V/60HZ 550V/50HZ",
                coilVoltage = "D - 120V/60HZ 110V/50HZ",
                coverControl = "-",
                numberOfPoles = "3 - 3 POLES",
                cbType = "NONE",
                cbRating = "A - NONE"
            };

            ECOrderInfo OrderInfo_ECL03B1D3A = new ECOrderInfo
            {
                catalogNumber = "ECL03B1D3A-C1C1A29P23P25S3",
                goItem = "GO#",
                designation = "-",
                type = "LIGHTING CONTACTOR",
                mainVolts = "600VAC",
                controlVolts = "120VAC",
                phase = "3",
                controlWire = "16AWG",
                horsepower = "20",//AMPERAGE
                enclosure = "TYPE 1",
                powerWire = "N/A",
                noOfUnits = "1",
                wire = "-",
                faultCap = "5kA",
                inputFreq = "60HZ"
            };

            var ECL03B1D3A = new ObservableCollection<PartItem>
            {
                new PartItem {OracleNumber = "", PartNumber = "C799B14", Quantity = 1, Locator = "A.M.O.1.", Description = "BOX 3, TYPE 1 - 12.65W X 13.85H X 6.40D - C400 + RST" },
                new PartItem {OracleNumber = "", PartNumber = "C400GK36A", Quantity = 1, Locator = "CAR03.10L.06.RM..", Description = "COVER CONTROL KIT - HAND/OFF/AUTO W RED RUN & GREEN OFF LIGHT" },
                new PartItem {OracleNumber = "", PartNumber = "CN35BN3AB", Quantity = 1, Locator = "CAR03.08U.02.RM..", Description = "NEMA CN35 LIGHTING CONT., ELEC. HELD, 3P 20A, 1NO AUX, 110V/50HZ 120V/60HZ COIL" },
                new PartItem {OracleNumber = "", PartNumber = "C320KGT15", Quantity = 1, Locator = "CAR03.08L.13.RM..", Description = "TOP AUX, NEMA FREEDOM CONTACTOR SIZE 00-2, CN35 10-60A, 2NO2NC" },
                new PartItem {OracleNumber = "", PartNumber = "C0050E4CFB-BP", Quantity = 1, Locator = "A.M.O.1.", Description = "CTRL XFRMR PRI: 550/575/600V SEC: 110/115/120V, 50VA" },
            };

            var ECL03B1D3A_modifications = new List<string>
            {
                "C1 - CPT - 120V/60 Hz, 110V/50 Hz secondary with two primary and one secondary fuse",
                "C1 - CPT - 120V/60 Hz, 110V/50 Hz secondary with two primary and one secondary fuse",
                "A29 - Side-mounted auxiliary contacts, IEC 40A and up - 1NO-1NC",
                "P23 - Pilot lights - Pilot light—red RUN",
                "P25 - Pilot lights - Pilot light—green OFF",
                "S - Standard IEC Overload 25-35A",
            };

            List<string> ECL03B1D3A_layers = new List<string>
            {
                "1NO-1NC (C1)",
                "3POLE",
                "GREEN OFF (C1)",
                "TRANSFORMER (L-L)",
                "HOA-COVER CONTROL WITHOUT OL",
                "RED RUN",
                "SPECS TRANSFORMER",
                "TYPE 1 BOX 3"
            };

            ECNomenclature Nom_ECN1812DKC = new ECNomenclature
            {
                type = "NEMA",
                design = "N - Freedom NEMA",
                Class = "18 - Combo NEMA starter with CPT (fusible or non-fusible disc.)",
                size = "1 - Size 1",
                enclType = "2 - Type 3R - Rainproof",
                ctrlXfrmr = "CPT",
                mainVoltage = "D - 600V/60HZ 550V/50HZ",
                coilVoltage = "D - 120V/60HZ 110V/50HZ",
                coverControl = "K - HAND/OFF/AUTO selector switch w. red RUN/green OFF lights",
                numberOfPoles = "",
                cbType = "DISCONNECT",
                cbRating = "C - 30A/600V R"
            };

            ECOrderInfo OrderInfo_ECN1812DKC = new ECOrderInfo
            {
                catalogNumber = "ECN1812DKC-C1H5/D6A29",
                goItem = "-",
                designation = "-",
                type = "COMBINATION MOTOR CONTROLLER",
                mainVolts = "600VAC",
                controlVolts = "120VAC",
                phase = "3",
                controlWire = "16AWG",
                horsepower = "1.5",
                enclosure = "TYPE 3R",
                powerWire = "14 AWG",
                noOfUnits = "1",
                wire = "3",
                faultCap = "65kAIC",
                inputFreq = "60HZ"
            };


            var ECN1812DKC = new ObservableCollection<PartItem>
            {
                new PartItem {OracleNumber = "", PartNumber = "2A92907G71", Quantity = 1, Locator = "A.M.O.1.", Description = "BOX A, TYPE 3R - 10.50W X 28.98H X 6.66D - (5) 30MM + RST + C371" },
                new PartItem {OracleNumber = "", PartNumber = "10250T21LB", Quantity = 1, Locator = "POU", Description = "3-POSITION SELECTOR SWITCH" },
                new PartItem {OracleNumber = "", PartNumber = "10250T181N", Quantity = 2, Locator = "POU", Description = "PILOT LIGHT UNIT 120V TRANSFORMER TYPE" },
                new PartItem {OracleNumber = "", PartNumber = "10250TC2N", Quantity = 1, Locator = "POU", Description = "PILOT LIGHT LENS GREEN" },
                new PartItem {OracleNumber = "", PartNumber = "10250TC1N", Quantity = 1, Locator = "POU", Description = "PILOT LIGHT LENS RED" },
                new PartItem {OracleNumber = "", PartNumber = "AN16DN0AB", Quantity = 1, Locator = "CAR03.08U.12.RM..", Description = "NEMA FREEDOM STARTER, FVNR, 3P 27A, SIZE 1, 1NO AUX, 110V/50HZ 120V/60HZ COIL" },
                new PartItem {OracleNumber = "", PartNumber = "H2006B-3", Quantity = 1, Locator = "A.M.O.1.", Description = "HEATER PACK, SIZE 00-2, CLASS 20, 1.79-2.90A" },
                new PartItem {OracleNumber = "", PartNumber = "C320KGS5", Quantity = 1, Locator = "CAR03.08L.12.RM..", Description = "SIDE AUX, NEMA FREEDOM CONTACTOR SIZE 00-2, CN35 10-60A,  2NC" },
                new PartItem {OracleNumber = "", PartNumber = "C361SDM", Quantity = 1, Locator = "EC01.04.01.KB..", Description = "C361 SWITCH DISCONNECT SW 60A" },
                new PartItem {OracleNumber = "", PartNumber = "C351KD71", Quantity = 1, Locator = "CAR03.10L.11.RM..", Description = "FUSE CLIP KIT 30A, CLASS J" },
                new PartItem {OracleNumber = "", PartNumber = "2A92910G01", Quantity = 1, Locator = "EC01.03.01.KB..", Description = "FLANGE HANDLE OPER MECH, C361 DISC. SWITCH, 60A, TYPE 1/3R/12" },
                new PartItem {OracleNumber = "", PartNumber = "C0100E4CFB-BP", Quantity = 1, Locator = "A.M.O.1.", Description = "CTRL XFRMR PRI: 550/575/600V SEC: 110/115/120V, 100VA" },
            };

            var ECN1812DKC_modifications = new List<string>
            {
                "C1 - CPT - 120V/60 Hz, 110V/50 Hz secondary with two primary and one secondary fuse\r\n",
                "H5/D6 - Install heater packs (Freedom Series), Class 20 - H2006B-3 ",
                "A29 - Side-mounted auxiliary contacts, IEC 40A and up - 1NO-1NC"
            };

            List<string> ECN1812DKC_layers = new List<string>
            {
                "1NO-1NC (M1)",
                "3PHASE-COMBO",
                "GREEN OFF (M1)",
                "HOA WITH OL",
                "RED RUN",
                "SPECS-COMBO",
                "SPECS-TRANSFORMER",
                "TRANSFORMER (L-L)",
                "TYPE 3R BOX A"
            };*/





/* OLD PARSING LOGIC

ParseXML parse = new ParseXML(xmlFilePath, myKickOffFunction);//create an instance of this class
CatalogNumber = parse.Parse();
currentCatalog = CatalogNumber[0];
SelectCatalogNumberItems(CatalogNumber[0]);

if (currentCatalog == "ECL03B1D3A-C1C1A29P23P25S"){
    loadJob(Nom_ECL03B1D3A, OrderInfo_ECL03B1D3A, ECL03B1D3A, ECL03B1D3A_modifications, ECL03B1D3A_layers);
}
else if (currentCatalog == "ECN1812DKC-C1H5/D6A29"){
    loadJob(Nom_ECN1812DKC, OrderInfo_ECN1812DKC, ECN1812DKC, ECN1812DKC_modifications, ECN1812DKC_layers);
}
else {
    MessageBox.Show("Job logic not in place");
}*/
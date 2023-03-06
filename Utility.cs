using System;
using System.Data.OleDb;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using QRCoder;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace PRL123_Final
{
    //functions used multiple times throughout the program
    
    public class Utility
    {
        public enum ProductGroup
        {
            PRL123,
            PRL4,
            PRLCS,
            EC
        }


        // The following is used to get data from LPdatabase 
        public static DataTable SearchLP(string query)      //given a query for LPdatabase, returns a loaded DataTable
        {
            try
            {
                DataTable dt = new DataTable();
                using (OleDbCommand cmd = new OleDbCommand(query, MainWindow.LPcon))
                {
                    using (OleDbDataReader rd = cmd.ExecuteReader())
                    {
                        dt.Load(rd);
                    } //end using reader
                } //end using command
                return dt;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Get Data From LPdatabase");
                return null;
            }
        }


        // The following is used to get data from Master Database 
        public static DataTable SearchMasterDB(string query)      //given a query for MasterDB, returns a loaded DataTable
        {
            try
            {
                DataTable dt = new DataTable();
                using (OleDbCommand cmd = new OleDbCommand(query, MainWindow.Mcon))
                {
                    using (OleDbDataReader rd = cmd.ExecuteReader())
                    {
                        dt.Load(rd);
                    } //end using reader
                } //end using command
                return dt;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Get Data From MasterDB");
                return null;
            }
        }

        public static string searchQueryGenerator(ProductGroup CurrentProduct, string FieldText, string SearchText)
        {
            string query = "";
            if (CurrentProduct == ProductGroup.PRL123)
            {
                if (FieldText == "ShopOrder")
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short] from [PRL123] where ShopOrderInterior like '%" + SearchText + "%' OR ShopOrderTrim like '%" + SearchText + "%' OR ShopOrderBox like '%" + SearchText + "%'";
                }
                else
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [BoxEarly], [Box Sent], [SpecialCustomer], [ServiceEntrance], [DoubleSection], [PaintedBox], [RatedNeutral200], [DNSB], [Complete], [Short] from [PRL123] where " + FieldText + " like '%" + SearchText + "%'";
                }
            }
            else if (CurrentProduct == Utility.ProductGroup.PRL4)
            {
                if (FieldText == "ShopOrder")
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short] from [PRL4] where ShopOrderInterior like '%" + SearchText + "%' OR ShopOrderTrim like '%" + SearchText + "%' OR ShopOrderBox like '%" + SearchText + "%'";
                }
                else
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [ServiceEntrance], [PaintedBox], [RatedNeutral200], [DoorOverDist], [DoorInDoor], [DNSB], [Complete], [Short] from [PRL4] where [" + FieldText + "] like '%" + SearchText + "%' and [PageNumber] = 0";
                }
            }
            else if (CurrentProduct == Utility.ProductGroup.PRLCS)
            {
                if (FieldText == "ShopOrder")
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short] from [PRLCS] where ShopOrderInterior like '%" + SearchText + "%' OR ShopOrderTrim like '%" + SearchText + "%' OR ShopOrderBox like '%" + SearchText + "%'";
                }
                else
                {
                    query = "select [ID], [GO_Item], [GO], [ShopOrderInterior], [ShopOrderBox], [ShopOrderTrim], [Customer], [Quantity], [EnteredDate], [ReleaseDate], [CommitDate], [Tracking], [Urgency], [AMO], [SpecialCustomer], [IncLocLeft], [IncLocRight], [CrossBus], [OpenBottom], [ExtendedTop], [PaintedBox], [ThirtyDeepEnclosure], [DNSB], [Complete], [Short] from [PRLCS] where [" + FieldText + "] like '%" + SearchText + "%' and [PageNumber] = 0";
                }
            }
            return query;
        }

        // The following is used to ExecuteNonQueries from LPdatabase 
        public static void executeNonQueryLP(string command)      
        {
            try
            {
                using (OleDbCommand cmd = new OleDbCommand(command, MainWindow.LPcon))
                {
                    cmd.ExecuteNonQuery();
                } //end using command
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To ExecuteNonQuery With LPdatabase");
            }
        }


        // The following is used to return a DataTableReader for any query from LPdatabase
        public static DataTableReader loadData(string query)
        {
            DataTable dt;
            dt = SearchLP(query);
            DataTableReader dtr = new DataTableReader(dt);
            return dtr;
        }


        // The following is used to return a DataTableReader with the CSAValues for a GOItem
        public static DataTableReader getCSAValues(string GOItem, ProductGroup CurrentProduct)
        {
            string query = "";
            if (CurrentProduct == ProductGroup.PRL123 || CurrentProduct == ProductGroup.PRL4)
            {
                query = "select * from [CSALabel] where [GO_Item]='" + GOItem + "'";
            }
            else if(CurrentProduct == ProductGroup.PRLCS)
            {
                query = "select * from [CSALabelPRLCS] where [GO_Item]='" + GOItem + "'"; 
            }
            DataTableReader dtr = loadData(query);
            return dtr;
        }

        // The following is used to DeleteCSAValues from LPdatabase given a GoItem 
        public static void DeleteCSAValues(string GoItem, ProductGroup CurrentProduct)
        {
            string command = "";
            if (CurrentProduct == ProductGroup.PRL123 || CurrentProduct == ProductGroup.PRL4)
            {
                command = "delete * from [CSALabel] where [GO_Item]='" + GoItem + "'";
            }
            else if(CurrentProduct == ProductGroup.PRLCS)
            {
                command = "delete * from [CSALabelPRLCS] where [GO_Item]='" + GoItem + "'";
            }
            executeNonQueryLP(command);
        }



        // The following is used to convert RenderTargetBitmap to BitmapImage 
        public static BitmapImage TargetToBitmap(RenderTargetBitmap img)
        {
            var renderTargetBitmap = img;
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }


        // The following is used to generate QRcode BitmapImages given strings 
        public static BitmapImage GenerateQRCode(string GO)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(GO, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap bitmap = qrCode.GetGraphic(20);
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        // The following is used to convert pdf files to an array of BitmapSources  
        public static BitmapSource[] ConvertPDFToImage(string filepath)
        {
            PdfReader pdfReader = new PdfReader(filepath);
            BitmapSource[] output = new BitmapSource[(int)pdfReader.NumberOfPages];

            output = FreewareTest(output, filepath);

            return output;
        }

        // The following is an intermediate step --> converts pdf to byte[] array, then calls ToImage
        public static BitmapSource[] FreewareTest(BitmapSource[] input, string filepath)
        {
            for (int i = 1; i <= input.Length; i++)
            {
                using (FileStream fs = File.OpenRead(filepath))
                {
                    byte[] png = Freeware.Pdf2Png.Convert(fs, i, 144);
                    input[i - 1] = ToImage(png);
                }
            }
            return input;
        }

        // The following is used to convert a byte array to a BitmapImage
        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }




        // The following is used to save a BitmapImage as a PDF to dstFilename
        public static void SaveImageToPdf(string dstFilename, BitmapImage img)
        {
            iTextSharp.text.Rectangle pageSize = null;
            using (var srcImage = BitmapImage2Bitmap(img))
            {
                pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
            }
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
                document.Open();

                var image = iTextSharp.text.Image.GetInstance(ImageToByte(img));
                document.Add(image);
                document.Close();

                File.WriteAllBytes(dstFilename, ms.ToArray());
            }
        }

        // The following is used to convert BitmapImage to Bitmap
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                return new System.Drawing.Bitmap(outStream);
            }
        }

        // The following is used to convert Bitmap to BitmapImage
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        // The following is used to convert BitmapImage to byte[] array
        public static byte[] ImageToByte(BitmapImage imageSource)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            encoder.Frames.Add(BitmapFrame.Create(imageSource));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }



        // The following is used to save a BitmapImage to a filePath as a PNG
        public static void SaveBitmapAsPNGinImages(string filePath, BitmapImage img)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(fileStream);
                fileStream.Close();
            }
        }


        // The following is used to retrieve PNG images --> filestream PNG To Byte[] Array then ToImage 
        public static BitmapImage PNGtoBitmap(string filepath)
        {
            using (FileStream input = new FileStream(filepath, FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    input.CopyTo(ms);
                    return ToImage(ms.ToArray());
                }
            }
        }


        // The following is used to convert png/jpg sourceFile to pdf destinationFile 
        public static void ConvertImageToPdf(string srcFilename, string dstFilename)
        {
            try
            {
                iTextSharp.text.Rectangle pageSize = null;

                using (var srcImage = new System.Drawing.Bitmap(srcFilename))
                {
                    pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
                }
                using (var ms = new MemoryStream())
                {
                    var document = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms).SetFullCompression();
                    document.Open();
                    var image = iTextSharp.text.Image.GetInstance(srcFilename);
                    document.Add(image);
                    document.Close();

                    File.WriteAllBytes(dstFilename, ms.ToArray());
                }
            }
            catch 
            {
                MessageBox.Show("An Error Occurred Trying To Convert Image To PDF");
            }
        }


        // The following is used to check if OLD/NEW job
        // -> OLD = saves image as binary & saves to root in order files
        // -> New = saves ImageFilePath & saves to PDF Storage in order files
        public static Boolean hasImageFilePath(string GO_Item, ProductGroup currentProduct)
        {
            try
            {
                if (currentProduct != ProductGroup.PRL123)
                {
                    return true;        //every other product group will have imageFilePath
                }
                //else it must be a ProductGroup.123
                Boolean hasImageFilePath = !string.IsNullOrEmpty(getImageFilePath(GO_Item, ProductGroup.PRL123, 0)); //TODO: Need to figure out integration with CS --> Ask Ralla
                return hasImageFilePath;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Check For ImageFilePath");
                return false;
            }
        }


        // The following is used to update the LastSave of an image edit
        public static void updateLastSave(string GO_Item, ProductGroup currentProduct, int pgNumber) 
        {
            try
            {
                string updateTime = DateTime.Now.ToString();
                string command = "";
                if (currentProduct == ProductGroup.PRL123)
                {
                    command = "update [PRL123] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "'";
                }
                else if (currentProduct == ProductGroup.PRL4)
                {
                    command = "update [PRL4] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if (currentProduct == ProductGroup.PRLCS)
                {
                    command = "update [PRLCS] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                executeNonQueryLP(command);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To UpdateLastSave");
            }
        }

        // The following is used to get the time of the LastSave of an image edit
        public static string getLastSave(string GO_Item, ProductGroup currentProduct, int pgNumber)
        {
            string output = "";
            try
            {
                string query = "";
                if (currentProduct == ProductGroup.PRL123)
                {
                    query = "select [LastSave] from [PRL123] where [GO_Item]='" + GO_Item + "'";
                }
                else if (currentProduct == ProductGroup.PRL4)
                {
                    query = "select [LastSave] from [PRL4] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if(currentProduct == ProductGroup.PRLCS)
                {
                    query = "select [LastSave] from [PRLCS] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        output = dtr[0].ToString();
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Get The LastSave");
            }
            return output;
        }


        // The following is used to get the Notes related to a GOItem
        public static void updateNotes(string GO_Item, string ProductTable, string NewNote)
        {
            try
            {
                string command = "update [" + ProductTable + "] set [Notes]='" + NewNote + "' where [GO_Item]='" + GO_Item + "'";
                executeNonQueryLP(command);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Update GOI Notes");
            }
        }


        // The following is used to get the Notes related to a GOItem
        public static string getNotes(string GO_Item, string ProductTable)
        {
            string output = "";
            try
            {
                string query = "select [Notes] from [" + ProductTable + "] where [GO_Item]='" + GO_Item + "'";
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        output = dtr[0].ToString();
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Get GOI Notes");
            }
            return output;
        }




        // The following is used to get the ImageFilePath
        public static string getImageFilePath(string GO_Item, ProductGroup currentProduct, int pgNumber)
        {
            string output = "";
            try
            {
                DataTable dt;
                string query = "";
               
                if (currentProduct == ProductGroup.PRL123)
                {
                    query = "select [ImageFilePath] from [PRL123] where [GO_Item]='" + GO_Item + "'";
                }
                else if (currentProduct == ProductGroup.PRL4)
                {
                    query = "select [ImageFilePath] from [PRL4] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if(currentProduct == ProductGroup.PRLCS)
                {
                    query = "select [ImageFilePath] from [PRLCS] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                dt = SearchLP(query);
                using (DataTableReader dtr = new DataTableReader(dt))
                {
                    while (dtr.Read())
                    {
                        output = dtr[0].ToString();
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Get The ImageFilePath");
            }
            return output;
        }

        
        // The following is used to retrieve the binary Bidman --> ONLY EXISTS FOR PRL123
        public static BitmapImage retrieveBinaryBidman(string GO_Item)
        {
            BitmapImage img = new BitmapImage();
            try
            {
                DataTableReader rd = loadData("select [Bidman] from [PRL123] where [GO_Item]='" + GO_Item + "'");
                using (rd)
                {
                    while (rd.Read())
                    {
                        img = ToImage((byte[])rd[0]);
                    }
                }
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To RetrieveBinaryBidman");
            }
            return img;
        }


        // The following is used to get the Directory for the BLT & CONSTR feature
        public static string getDirectoryForOrderFiles(string SelectedGO, ProductGroup CurrentProduct)
        {
            DataTable dt;
            string query = "";
            string output = "";

            if (CurrentProduct == ProductGroup.PRL123)
            {
                query = "select [FilePath] from [PRL123] where [GO_Item]='" + SelectedGO + "'";
            }
            else if (CurrentProduct == ProductGroup.PRL4)
            {
                query = "select [FilePath] from [PRL4] where [GO_Item]='" + SelectedGO + "' AND [PageNumber]=0";
            }
            else if (CurrentProduct == ProductGroup.PRLCS)
            {
                query = "select [FilePath] from [PRLCS] where [GO_Item]='" + SelectedGO + "' AND [PageNumber]=0";
            }
            dt = SearchLP(query);
            using (DataTableReader dtr = new DataTableReader(dt))
            {
                while (dtr.Read())
                {
                    output = dtr[0].ToString();
                }
            } //end using reader

            output = output.Substring(0, output.Length - 11);
            return output;
        }



        // The following is used for the AddToShopPackage feature
        public static int executeAddToShopPack(string SelectedGO, ProductGroup CurrentProduct) 
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf";
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string insertGOI = SelectedGO;
                    string insertGO = SelectedGO.Substring(0, 10);
                    string SOInterior = "";
                    string SOBox = "";
                    string SOTrim = "";
                    string Customer = "";
                    string Quantity = "";
                    string EnterDate = "";
                    string ReleaseDate = "";
                    string CommitDate = "";
                    string Tracking = "";
                    string Urgency = "";

                    Boolean SpecialCustomer = false;
                    Boolean AMO = false;
                    Boolean PaintedBox = false;

                    //PRL
                    Boolean ServiceEntrance = false;
                    Boolean RatedNeutral200 = false;
                    Boolean DoorOverDist = false;
                    Boolean DoorInDoor = false;
                    
                    //PRLCS Checklist
                    Boolean IncLocLeft = false;
                    Boolean IncLocRight = false;
                    Boolean CrossBus = false;
                    Boolean OpenBottom = false;
                    Boolean ExtendedTop = false;
                    Boolean ThirtyDeepEncolsure = false;
                    
                    Boolean DNSB = false;
                    Boolean Complete = false;
                    Boolean Short = false;

                    string type = "";
                    string volts = "";
                    string amps = "";
                    string torque = "";
                    string appearance = "";
                    string bus = "";
                    string catalogue = "";
                    string prodSpecialist = "";

                    string pdfFilepath = "";
                    string imageFilepath = "";
                    int pageNumber = 0;

                    string query = "";
                    if (CurrentProduct == ProductGroup.PRL4)
                    {
                        query = "select [ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[ServiceEntrance],[PaintedBox],[RatedNeutral200],[DoorOverDist],[DoorInDoor],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber] from [PRL4] where [GO_Item]='" + SelectedGO + "' order by [GO_Item],[PageNumber]";
                    }
                    else if(CurrentProduct == ProductGroup.PRLCS)
                    {
                        query = "select [ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[IncLocLeft],[IncLocRight],[CrossBus],[OpenBottom],[ExtendedTop],[PaintedBox],[ThirtyDeepEnclosure],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber] from [PRLCS] where [GO_Item]='" + SelectedGO + "' order by [GO_Item],[PageNumber]";
                    }
                    using (DataTableReader dtr = loadData(query))
                    {
                        while (dtr.Read())
                        {
                           
                            if(CurrentProduct == ProductGroup.PRL4)
                            {
                                SOInterior = dtr[0].ToString();
                                SOBox = dtr[1].ToString();
                                SOTrim = dtr[2].ToString();
                                Customer = dtr[3].ToString();
                                Quantity = dtr[4].ToString();
                                EnterDate = dtr[5].ToString();
                                ReleaseDate = dtr[6].ToString();
                                CommitDate = dtr[7].ToString();
                                Tracking = dtr[8].ToString();
                                Urgency = dtr[9].ToString();

                                SpecialCustomer = (Boolean)dtr[10];
                                AMO = (Boolean)dtr[11];

                                ServiceEntrance = (Boolean)dtr[12];
                                PaintedBox = (Boolean)dtr[13];
                                RatedNeutral200 = (Boolean)dtr[14];
                                DoorOverDist = (Boolean)dtr[15];
                                DoorInDoor = (Boolean)dtr[16];
                                DNSB = (Boolean)dtr[17];
                                Complete = (Boolean)dtr[18];
                                Short = (Boolean)dtr[19];

                                type = dtr[20].ToString();
                                volts = dtr[21].ToString();
                                amps = dtr[22].ToString();
                                torque = dtr[23].ToString();
                                appearance = dtr[24].ToString();
                                bus = dtr[25].ToString();
                                catalogue = dtr[26].ToString();
                                prodSpecialist = dtr[27].ToString();

                                pdfFilepath = dtr[28].ToString();
                                imageFilepath = dtr[29].ToString();
                                pageNumber = (int)dtr[30];
                            }
                            else if(CurrentProduct == ProductGroup.PRLCS)
                            {
                                SOInterior = dtr[0].ToString();
                                SOBox = dtr[1].ToString();
                                SOTrim = dtr[2].ToString();
                                Customer = dtr[3].ToString();
                                Quantity = dtr[4].ToString();
                                EnterDate = dtr[5].ToString();
                                ReleaseDate = dtr[6].ToString();
                                CommitDate = dtr[7].ToString();
                                Tracking = dtr[8].ToString();
                                Urgency = dtr[9].ToString();

                                SpecialCustomer = (Boolean)dtr[10];
                                AMO = (Boolean)dtr[11];
                                IncLocLeft = (Boolean)dtr[12];
                                IncLocRight = (Boolean)dtr[13];
                                CrossBus = (Boolean)dtr[14];
                                OpenBottom = (Boolean)dtr[15];
                                ExtendedTop = (Boolean)dtr[16];
                                PaintedBox = (Boolean)dtr[17];
                                ThirtyDeepEncolsure = (Boolean)dtr[18];

                                DNSB = (Boolean)dtr[18];
                                Complete = (Boolean)dtr[19];
                                Short = (Boolean)dtr[20];

                                type = dtr[21].ToString();
                                volts = dtr[22].ToString();
                                amps = dtr[23].ToString();
                                torque = dtr[24].ToString();
                                appearance = dtr[25].ToString();
                                bus = dtr[26].ToString();
                                catalogue = dtr[27].ToString();
                                prodSpecialist = dtr[28].ToString();

                                pdfFilepath = dtr[29].ToString();
                                imageFilepath = dtr[30].ToString();
                                pageNumber = (int)dtr[31];
                            }
                            
                        }
                    }
                    pageNumber += 1;
                    imageFilepath = System.IO.Path.GetFullPath(System.IO.Path.Combine(imageFilepath, @"..\" + SelectedGO.Substring(0, 10) + "_" + SelectedGO.Substring(11, SelectedGO.Length - 11) + "_" + pageNumber.ToString() + ".png"));


                    string command = "";
                    if (CurrentProduct == ProductGroup.PRL4)
                    {
                        command = "INSERT INTO [PRL4]([GO_Item],[GO],[ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[ServiceEntrance],[PaintedBox],[RatedNeutral200],[DoorOverDist],[DoorInDoor],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber]) VALUES('" + insertGOI + "','" + insertGO + "','" + SOInterior + "','" + SOBox + "','" + SOTrim + "','" + Customer + "','" + Quantity + "','" + EnterDate + "','" + ReleaseDate + "','" + CommitDate + "','" + Tracking + "','" + Urgency + "'," + SpecialCustomer.ToString() + "," + AMO.ToString() + "," + ServiceEntrance.ToString() + "," + PaintedBox.ToString() + "," + RatedNeutral200.ToString() + "," + DoorOverDist.ToString() + "," + DoorInDoor.ToString() + "," + DNSB.ToString() + "," + Complete.ToString() + "," + Short.ToString() + ",'" + type + "','" + volts + "','" + amps + "','" + torque + "','" + appearance + "','" + bus + "','" + catalogue + "','" + prodSpecialist + "','" + pdfFilepath + "','" + imageFilepath + "'," + pageNumber.ToString() + ")";
                    }
                    else if(CurrentProduct == ProductGroup.PRLCS)
                    {
                        command = "INSERT INTO [PRLCS]([GO_Item],[GO],[ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[IncLocLeft],[IncLocRight],[CrossBus],[OpenBottom],[ExtendedTop],[PaintedBox],[ThirtyDeppEnclosure],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber]) VALUES('" + insertGOI + "','" + insertGO + "','" + SOInterior + "','" + SOBox + "','" + SOTrim + "','" + Customer + "','" + Quantity + "','" + EnterDate + "','" + ReleaseDate + "','" + CommitDate + "','" + Tracking + "','" + Urgency + "'," + SpecialCustomer.ToString() + "," + AMO.ToString() + "," + ServiceEntrance.ToString() + "," + PaintedBox.ToString() + "," + RatedNeutral200.ToString() + "," + DoorOverDist.ToString() + "," + DoorInDoor.ToString() + "," + DNSB.ToString() + "," + Complete.ToString() + "," + Short.ToString() + ",'" + type + "','" + volts + "','" + amps + "','" + torque + "','" + appearance + "','" + bus + "','" + catalogue + "','" + prodSpecialist + "','" + pdfFilepath + "','" + imageFilepath + "'," + pageNumber.ToString() + ")";
                    }
                    executeNonQueryLP(command);


                    string filepath = ofg.FileName;
                    string fileExtenstion = System.IO.Path.GetExtension(filepath);

                    BitmapImage NewInsertIMG;
                    if (fileExtenstion == ".pdf" || fileExtenstion == ".PDF")
                    {
                        BitmapSource NewImage = ConvertPDFToImage(filepath)[0];     //first page only
                        NewInsertIMG = (BitmapImage)NewImage;
                    }
                    else
                    {
                        NewInsertIMG = PNGtoBitmap(filepath);
                    }
                    //save as pdf
                    var output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pdfFilepath), SelectedGO + "_" + pageNumber.ToString() + "_CONSTR.pdf");
                    SaveImageToPdf(output, NewInsertIMG);

                    //save as png
                    SaveBitmapAsPNGinImages(imageFilepath, NewInsertIMG);

                    return 1;
                }
                return -1;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To AddToShopPackage");
                return -2;
            }
        }


        // The following is used for the feature CONSTR insert
        public static int executeCONSTRinsert(string SelectedGO, string pdfDirectory, ProductGroup currentProduct)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string filepath = ofg.FileName;

                    string fileExtenstion = System.IO.Path.GetExtension(filepath);
                    string newPath;
                    if (hasImageFilePath(SelectedGO, currentProduct))
                    {
                        newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(pdfDirectory, @"..\"));  //one folder back from pdfDirectory
                    }
                    else
                    {
                        newPath = pdfDirectory; //when there is no PDF Storage folder, stores in root
                    }

                    var pdfDir = new DirectoryInfo(pdfDirectory);
                    var rootDir = new DirectoryInfo(newPath);

                    FileInfo[] pdfFileArr = pdfDir.GetFiles(SelectedGO + "-*");
                    FileInfo[] pdfFileArr2 = rootDir.GetFiles("AdditionalInfo_" + SelectedGO + "-*");

                    int fileNumber = pdfFileArr.Length;
                    int fileNumberElse = pdfFileArr2.Length;

                    if (fileExtenstion == ".pdf" || fileExtenstion == ".PDF")
                    {
                        File.Copy(filepath, pdfDirectory + @"\" + SelectedGO + "-" + fileNumber + "_CONSTR.pdf", true);
                    }
                    else if (fileExtenstion == ".jpg" || fileExtenstion == ".JPG")
                    {
                        ConvertImageToPdf(filepath, pdfDirectory + @"\" + SelectedGO + "-" + fileNumber + "_CONSTR.pdf");
                    }
                    else if (fileExtenstion == ".png" || fileExtenstion == ".PNG")
                    {
                        ConvertImageToPdf(filepath, pdfDirectory + @"\" + SelectedGO + "-" + fileNumber + "_CONSTR.pdf");
                    }
                    else    //Just Copy into Folder, Do not append 
                    {
                        File.Copy(filepath, newPath + @"\AdditionalInfo_" + SelectedGO + "-" + fileNumberElse + fileExtenstion, true);
                    }
                    return 1;
                }
                return -1;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute CONSTR Insert");
                return -2;
            }
        }


        // The following is used for the feature BLT insert
        public static int executeBLTinsert(string SelectedGO, string pdfDirectory, ProductGroup currentProduct)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog();
                ofg.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string filepath = ofg.FileName;

                    string fileExtenstion = System.IO.Path.GetExtension(filepath);
                    string newPath;
                    if (hasImageFilePath(SelectedGO, currentProduct))
                    {
                        newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(pdfDirectory, @"..\"));     //one folder back from pdfDirectory
                    }
                    else
                    {
                        newPath = pdfDirectory; //when there is no PDF Storage folder, stores in root
                    }

                    var pdfDir = new DirectoryInfo(pdfDirectory);
                    var rootDir = new DirectoryInfo(newPath);

                    FileInfo[] pdfFileArr = pdfDir.GetFiles("BLT_" + SelectedGO + "-*");
                    FileInfo[] pdfFileArr2 = rootDir.GetFiles("AdditionalInfo_" + SelectedGO + "-*");

                    int fileNumber = pdfFileArr.Length;
                    int fileNumberElse = pdfFileArr2.Length;

                    if (fileExtenstion == ".pdf" || fileExtenstion == ".PDF")
                    {
                        File.Copy(filepath, pdfDirectory + @"\BLT_" + SelectedGO + "-" + fileNumber + ".pdf", true);
                    }
                    else if (fileExtenstion == ".jpg" || fileExtenstion == ".JPG")
                    {
                        ConvertImageToPdf(filepath, pdfDirectory + @"\BLT_" + SelectedGO + "-" + fileNumber + ".pdf");
                    }
                    else if (fileExtenstion == ".png" || fileExtenstion == ".PNG")
                    {
                        ConvertImageToPdf(filepath, pdfDirectory + @"\BLT_" + SelectedGO + "-" + fileNumber + ".pdf");
                    }
                    else    //Just Copy into Folder, Do not append 
                    {
                        File.Copy(filepath, newPath + @"\AdditionalInfo_" + SelectedGO + "-" + fileNumberElse + fileExtenstion, true);
                    }
                    return 1;
                }
                return -1;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Execute BLT Insert");
                return -2;
            }
        }


        //The following is used to open up the job GO folder on the network
        public static void AccessFolderGO(string GO)
        {
            try
            {
                string folderPath = ConfigurationManager.ConnectionStrings["orderFiles"].ToString() + @"\" + GO;

                if (Directory.Exists(folderPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = folderPath,
                        FileName = "explorer.exe"
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("The Requested FolderPath Does Not Exist");
                }
            }
            catch 
            {
                MessageBox.Show("An Error Occurred Accessing Directory");
            }
        }



        // The following is used to get the GO# given the ID#
        public static string IDtoGO(int ID, string ProductTable)
        {
            string query = "select [GO] from [" + ProductTable + "] where [ID]=" + ID.ToString();
            string output = "";

            using (DataTableReader dtr = loadData(query))
            {
                while (dtr.Read())
                {
                    output = dtr[0].ToString();
                }
            } //end using reader

            return output;
        }



        // The following is used to Merge a list of pdfFiles to an outputFile
        public static void Merge(List<string> InFiles, string OutFile)
        {
            using (FileStream stream = new FileStream(OutFile, FileMode.Create))
            {
                using (Document doc = new Document())
                {
                    using (PdfCopy pdf = new PdfCopy(doc, stream))
                    {
                        doc.Open();

                        PdfReader reader = null;
                        PdfImportedPage page = null;

                        InFiles.ForEach(file =>
                        {
                            reader = new PdfReader(file);

                            for (int i = 0; i < reader.NumberOfPages; i++)
                            {
                                page = pdf.GetImportedPage(reader, i + 1);
                                pdf.AddPage(page);
                            }

                            pdf.FreeReader(reader);
                            reader.Close();
                        });
                    }
                }
            }
        }



        // The following is used to write all lines of a string array to a TXT documentPath outputFile
        public static void WriteLinesToTXT(string[] lines, string documentPath)
        {
            using (StreamWriter outputFile = new StreamWriter(documentPath))
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
        }


        // The following is used to convert a txt file to a pdf file
        public static void ConvertTXTtoPDF(string txtPath, string pdfPath)
        {
            using (StreamReader rdr = new StreamReader(txtPath))
            {
                using (Document doc = new Document())
                {
                    using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                    {
                        PdfWriter.GetInstance(doc, fs);
                        doc.Open();
                        doc.Add(new iTextSharp.text.Paragraph(rdr.ReadToEnd()));
                        doc.Close();
                    }
                }
            }
        }



        // The following is used to turn a PDF into a string array where each index is a page
        public static string[] ExtractTextFromPdf(string path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int pgNumber = 1; pgNumber <= reader.NumberOfPages; pgNumber++)      //gets the text from each pdf page, appends \t to separate each page
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, pgNumber));
                    text.Append("\t");
                }
                return text.ToString().Split('\t');       //returns a string array where each index is a text block of a page 
            }
        }



        // The following is used to check LPdatabase for a duplicate GO_Item
        public static Boolean isDuplicate(string GO_Item, ProductGroup CurrentProduct)
        {
            try
            {
                string query = "";
                if (CurrentProduct == ProductGroup.PRL123)
                {
                    query = "select [GO_Item] from [PRL123] where [GO_Item]='" + GO_Item + "'";
                }
                else if (CurrentProduct == ProductGroup.PRL4)
                {
                    query = "select [GO_Item] from [PRL4] where [GO_Item]='" + GO_Item + "' and [PageNumber]=0";
                }
                else if (CurrentProduct == ProductGroup.PRLCS)
                {
                    query = "select [GO_Item] from [PRLCS] where [GO_Item]='" + GO_Item + "' and [PageNumber]=0";
                }
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        if (dtr[0] != null)
                        {
                            return true;    //there is a duplicate in LPdatabase
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("Error In Checking For Duplicates");
            }
            return false;
        }



        // The following is used to access the ReplacementParts table
        public static string ReplacePart(string part)
        {
            string replacement = part;
            try
            {
                string query = "select [Replace] from [ReplacementParts] where [Find]='" + part + "'";
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        if (dtr[0] != null)
                        {
                            replacement = dtr[0].ToString();    //there is a replacement available
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("Unable To Replace Part");
            }
            return replacement;
        }



        // The following is used to search the PullSequence table for a part, if the part is not there => it is standardAMO   
        public static Boolean standardAMO(string part)
        {
            try
            {
                int ID = -10;
                string query = "select [ID] from [PullSequence] where [Item]='" + part + "'";
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        ID = (int)dtr[0];
                    }
                } //end using reader
                if (ID == -10)
                {
                    return true;    //not in PullSequence table
                }
            }
            catch
            {
                MessageBox.Show("Error Accessing ID From Pull Sequence");
            }
            return false;
        }



        // The following is used to check if the quantity of a part required is more than half of the regular material we order => KanBanSpike
        public static Boolean KanBanSpike(string part, int quantity)
        {
            try
            {
                string query = "select [HalfSize] from [PullSequence] where [Item]='" + part + "'";
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        if (dtr[0] != null)
                        {
                            if (int.Parse(dtr[0].ToString()) <= quantity)    //converts string to integer and compares to quantity
                            {
                                return true;
                            }
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("Error In Checking For KanBanSpike");
            }
            return false;
        }


        // The following is used to get the ItemStatus (Active or Disabled) and the Description of a PartNumber
        public static string[] PullPartStatus(string part)
        {
            string[] output = new string[2];
            try
            {
                string query = "select [ItemStatus], [Description] from [PullPartStatus] where [Item]='" + part + "'";
                using (DataTableReader dtr = loadData(query))
                {
                    while (dtr.Read())
                    {
                        if (dtr[0] != null)
                        {
                            output[0] = dtr[0].ToString();
                            output[1] = dtr[1].ToString();
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("Error In PullingPartStatus");
            }
            return output;
        }



        // The following is used to get the max voltage given the full voltage string
        public static string getMaxVoltage(string fullVoltage)
        {
            if (string.IsNullOrEmpty(fullVoltage))
            {
                return "";
            }
            else 
            {
                if (fullVoltage.Contains("/"))
                {
                    string[] voltages = fullVoltage.Split('/'); 
                    string voltage1 = getNumberInString(voltages[0]);
                    string voltage2 = getNumberInString(voltages[1]);

                    if (Int32.Parse(voltage1) > Int32.Parse(voltage2))
                    {
                        return voltage1;
                    }
                    else 
                    {
                        return voltage2;
                    }
                }
                else 
                {
                    return getNumberInString(fullVoltage);
                }
            }
        }


        // The following is used to extract a number from a specified string
        public static string getNumberInString(string strSource)
        {
            string[] numbers;
            numbers = Regex.Split(strSource, @"\D+");

            return numbers[0];
        }


        // The following is used to truncate a source string given a start string and an end string
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            return "";
        }


        // The following is used to increment string itemNumber by int amount
        public static string IterateItem(string itemNumber, int amount)
        {
            string itemValue = itemNumber.Substring(0, 3);     //the first 3 characters of itemNumber 
            itemValue = itemValue.TrimStart('0');           //remove all leading zeros 

            int Value = Convert.ToInt32(itemValue);    //adds amount parameter
            Value += amount;

            return Value.ToString("D3") + "I";      //D3 -> three digit decimal toString format
        }


        // The following uses a string array as input and returns a single concatenated string for an SQL WHERE IN statement
        public static string GetProductNameListInString(string[] ProductNames) 
        {
            string output = "(";
            for (int i = 0; i < ProductNames.Length; i++) 
            {
                if (i != ProductNames.Length - 1) //make sure it is not the last index
                {
                    output += "'" + ProductNames[i] + "',";     //ends with comma
                }
                else            //in the case of the last index
                {
                    output += "'" + ProductNames[i] + "')";     //ends with closing bracket
                }
            }
            return output;
        }


    }
}

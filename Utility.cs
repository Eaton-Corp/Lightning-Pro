﻿using System;
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
using System.Windows.Shapes;

namespace LightningPRO
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
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred trying to get data from LPdatabase: {ex.Message}");
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


        // The following is used to ExecuteNonQueries from LPdatabase 
        public static void ExecuteNonQueryLP(string command)
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
        public static DataTableReader LoadData(string query)
        {
            DataTable dt;
            dt = SearchLP(query);
            DataTableReader dtr = new DataTableReader(dt);
            return dtr;
        }
        

        // The following is used to return a DataTableReader with the CSAValues for a GOItem
        public static DataTableReader GetCSAValues(string GOItem, ProductGroup CurrentProduct)
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
            DataTableReader dtr = LoadData(query);
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
            ExecuteNonQueryLP(command);
        }


        // The following is used to generate LP search queries for different field texts
        public static string SearchQueryGenerator(ProductGroup CurrentProduct, string FieldText, string SearchText)
        {
            string query = "";
            if (CurrentProduct == ProductGroup.PRL123)
            {
                if (FieldText == "ShopOrder")
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
                        "PRL123.[JobName], " +
                        "PRL123.[Customer], " +
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
                        "WHERE (PRL123.ShopOrderInterior LIKE '%" + SearchText + "%' " +
                        "OR PRL123.ShopOrderTrim LIKE '%" + SearchText + "%' " +
                        "OR PRL123.ShopOrderBox LIKE '%" + SearchText + "%' " +
                        "OR PRL123.SchedulingGroup LIKE '%" + SearchText + "%') " +
                        "AND PRL123.[PageNumber] = 0";
                }
                else
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
                        "PRL123.[JobName], " +
                        "PRL123.[Customer], " +
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
                        "WHERE PRL123.[" + FieldText + "] LIKE '%" + SearchText + "%' " +
                        "AND PRL123.[PageNumber] = 0";
                }

            }
            else if (CurrentProduct == ProductGroup.PRL4)
            {
                if (FieldText == "ShopOrder")
                {
                    query = "select Distinct PRL4.[ID], PRL4.[GO_Item], PRL4.[GO], CSALabel.[ProductID], PRL4.[ShopOrderInterior], PRL4.[ShopOrderBox], PRL4.[ShopOrderTrim], PRL4.[SchedulingGroup], PRL4.[JobName], PRL4.[Customer], PRL4.[Quantity], PRL4.[EnteredDate], PRL4.[ReleaseDate], PRL4.[CommitDate], PRL4.[Tracking], PRL4.[Urgency], PRL4.[AMO], PRL4.[SpecialCustomer], PRL4.[ServiceEntrance], PRL4.[PaintedBox], PRL4.[RatedNeutral200], PRL4.[DoorOverDist], PRL4.[DoorInDoor], PRL4.[DNSB], PRL4.[Complete], PRL4.[Short], PRL4.[NameplateRequired], PRL4.[NameplateOrdered], PRL4.[LabelsPrinted], PRL4.[BoxEarly], PRL4.[BoxSent], PRL4.[Notes] from [PRL4] inner join [CSALabel] on PRL4.[GO_Item] = CSALabel.[GO_Item] where PRL4.[PageNumber] = 0 and PRL4.ShopOrderInterior like '%" + SearchText + "%' OR PRL4.[PageNumber] = 0 and PRL4.ShopOrderTrim like '%" + SearchText + "%' OR PRL4.[PageNumber] = 0 and PRL4.ShopOrderBox like '%" + SearchText + "%' OR PRL4.[PageNumber] = 0 and PRL4.SchedulingGroup like '%" + SearchText + "%'";
                }
                else
                {
                    query = "select Distinct PRL4.[ID], PRL4.[GO_Item], PRL4.[GO], CSALabel.[ProductID], PRL4.[ShopOrderInterior], PRL4.[ShopOrderBox], PRL4.[ShopOrderTrim], PRL4.[SchedulingGroup], PRL4.[JobName], PRL4.[Customer], PRL4.[Quantity], PRL4.[EnteredDate], PRL4.[ReleaseDate], PRL4.[CommitDate], PRL4.[Tracking], PRL4.[Urgency], PRL4.[AMO], PRL4.[SpecialCustomer], PRL4.[ServiceEntrance], PRL4.[PaintedBox], PRL4.[RatedNeutral200], PRL4.[DoorOverDist], PRL4.[DoorInDoor], PRL4.[DNSB], PRL4.[Complete], PRL4.[Short], PRL4.[NameplateRequired], PRL4.[NameplateOrdered], PRL4.[LabelsPrinted], PRL4.[BoxEarly], PRL4.[BoxSent], PRL4.[Notes] from [PRL4] inner join [CSALabel] on PRL4.[GO_Item] = CSALabel.[GO_Item] where PRL4.[" + FieldText + "] like '%" + SearchText + "%' and PRL4.[PageNumber] = 0";
                }
            }
            else if (CurrentProduct == ProductGroup.PRLCS)
            {
                if (FieldText == "ShopOrder")
                {                    
                    query = "select Distinct PRLCS.[ID], PRLCS.[GO_Item], PRLCS.[GO], CSALabelPRLCS.[ProductID], PRLCS.[ShopOrderInterior], PRLCS.[ShopOrderBox], PRLCS.[ShopOrderTrim], PRLCS.[SchedulingGroup], PRLCS.[JobName], PRLCS.[Customer], PRLCS.[Quantity], PRLCS.[EnteredDate], PRLCS.[ReleaseDate], PRLCS.[CommitDate], PRLCS.[Tracking], PRLCS.[Urgency], PRLCS.[AMO], PRLCS.[SpecialCustomer], PRLCS.[IncLocLeft], PRLCS.[IncLocRight], PRLCS.[CrossBus], PRLCS.[OpenBottom], PRLCS.[ExtendedTop], PRLCS.[PaintedBox], PRLCS.[ThirtyDeepEnclosure], PRLCS.[DNSB], PRLCS.[Complete], PRLCS.[Short], PRLCS.[NameplateRequired], PRLCS.[NameplateOrdered], PRLCS.[LabelsPrinted], PRLCS.[Notes] from [PRLCS] inner join [CSALabelPRLCS] on PRLCS.[GO_Item] = CSALabelPRLCS.[GO_Item] where PRLCS.[PageNumber] = 0 and PRLCS.ShopOrderInterior like '%" + SearchText + "%' OR PRLCS.[PageNumber] = 0 and PRLCS.ShopOrderTrim like '%" + SearchText + "%' OR PRLCS.[PageNumber] = 0 and PRLCS.ShopOrderBox like '%" + SearchText + "%' OR PRLCS.[PageNumber] = 0 and PRLCS.SchedulingGroup like '%" + SearchText + "%'";
                }
                else
                {
                    query = "select Distinct PRLCS.[ID], PRLCS.[GO_Item], PRLCS.[GO], CSALabelPRLCS.[ProductID], PRLCS.[ShopOrderInterior], PRLCS.[ShopOrderBox], PRLCS.[ShopOrderTrim], PRLCS.[SchedulingGroup], PRLCS.[JobName], PRLCS.[Customer], PRLCS.[Quantity], PRLCS.[EnteredDate], PRLCS.[ReleaseDate], PRLCS.[CommitDate], PRLCS.[Tracking], PRLCS.[Urgency], PRLCS.[AMO], PRLCS.[SpecialCustomer], PRLCS.[IncLocLeft], PRLCS.[IncLocRight], PRLCS.[CrossBus], PRLCS.[OpenBottom], PRLCS.[ExtendedTop], PRLCS.[PaintedBox], PRLCS.[ThirtyDeepEnclosure], PRLCS.[DNSB], PRLCS.[Complete], PRLCS.[Short], PRLCS.[NameplateRequired], PRLCS.[NameplateOrdered], PRLCS.[LabelsPrinted], PRLCS.[Notes] from [PRLCS] inner join [CSALabelPRLCS] on PRLCS.[GO_Item] = CSALabelPRLCS.[GO_Item] where PRLCS.[" + FieldText + "] like '%" + SearchText + "%' and PRLCS.[PageNumber] = 0";
                }
            }
            return query;
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
            var encoder = new JpegBitmapEncoder
            {
                QualityLevel = 100
            };
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
        public static Boolean HasImageFilePath(string GO_Item, ProductGroup currentProduct)
        {
            try
            {
                if (currentProduct != ProductGroup.PRL123)
                {
                    return true;        //every other product group will have imageFilePath
                }
                //else it must be a ProductGroup.123
                Boolean hasImageFilePath = !string.IsNullOrEmpty(GetImageFilePath(GO_Item, ProductGroup.PRL123, 0)); //TODO: Need to figure out integration with CS --> Ask Ralla
                return hasImageFilePath;
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Check For ImageFilePath");
                return false;
            }
        }


        // The following is used to update the LastSave of an image edit
        public static void UpdateLastSave(string GO_Item, ProductGroup currentProduct, int pgNumber)
        {
            try
            {
                string updateTime = DateTime.Now.ToString();
                string command = "";
                if (currentProduct == ProductGroup.PRL123)
                {
                    command = "update [PRL123] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if (currentProduct == ProductGroup.PRL4)
                {
                    command = "update [PRL4] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if (currentProduct == ProductGroup.PRLCS)
                {
                    command = "update [PRLCS] set [LastSave]='" + updateTime + "' where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                ExecuteNonQueryLP(command);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To UpdateLastSave");
            }
        }

        // The following is used to get the time of the LastSave of an image edit
        public static string GetLastSave(string GO_Item, ProductGroup currentProduct, int pgNumber)
        {
            string output = "";
            try
            {
                string query = "";
                if (currentProduct == ProductGroup.PRL123)
                {
                    query = "select [LastSave] from [PRL123] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();;
                }
                else if (currentProduct == ProductGroup.PRL4)
                {
                    query = "select [LastSave] from [PRL4] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                else if(currentProduct == ProductGroup.PRLCS)
                {
                    query = "select [LastSave] from [PRLCS] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();
                }
                using (DataTableReader dtr = LoadData(query))
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
        public static void UpdateNotes(string GO_Item, string ProductTable, string NewNote)
        {
            try
            {
                string command = "update [" + ProductTable + "] set [Notes]='" + NewNote + "' where [GO_Item]='" + GO_Item + "'";
                ExecuteNonQueryLP(command);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Update GOI Notes");
            }
        }


        // The following is used to get the Notes related to a GOItem
        public static string GetNotes(string GO_Item, string ProductTable)
        {
            string output = "";
            try
            {
                string query = "select [Notes] from [" + ProductTable + "] where [GO_Item]='" + GO_Item + "'";
                using (DataTableReader dtr = LoadData(query))
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



        // The following is used to determine if the labels for a GOItem have already been printed
        public static bool HaveLabelsPrinted(string GO_Item, string ProductTable)
        {
            try
            {
                string query = "select [LabelsPrinted] from [" + ProductTable + "] where [GO_Item]='" + GO_Item + "'";
                using (DataTableReader dtr = LoadData(query))
                {
                    while (dtr.Read())
                    {
                        if ((Boolean)dtr[0]) 
                        {
                            return true;
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Check LabelsPrinted");
            }
            return false;
        }


        // The following is used to determine if all the labels for a GO Number have already been printed
        public static Boolean HaveAllLabelsPrinted(string GO_Num, string ProductTable)
        {
            try
            {
                string query = "select [LabelsPrinted] from [" + ProductTable + "] where [GO]='" + GO_Num + "' and [Tracking]='Production'";
                DataTable dt = SearchLP(query);
                
                int NumRows = dt.Rows.Count;
                int Counter = 0;
                
                if (NumRows > 0) 
                {
                    using (DataTableReader dtr = new DataTableReader(dt))
                    {
                        while (dtr.Read())
                        {
                            if ((Boolean)dtr[0])
                            {
                                Counter++;
                            }
                        }
                    } //end using reader
                    if (Counter >= NumRows) return true;
                }
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Check If AllLabelsPrinted");
            }
            return false;
        }


        // The following is used when the labels for a GOItem have been printed
        public static void LabelPrinted(string GO_Item, string ProductTable)
        {
            try
            {
                string command = "update [" + ProductTable + "] set [LabelsPrinted]=TRUE where [GO_Item]='" + GO_Item + "'";
                ExecuteNonQueryLP(command);
            }
            catch
            {
                MessageBox.Show("An Error Occurred Trying To Update LabelsPrinted");
            }
        }



        // The following is used to get the ImageFilePath
        public static string GetImageFilePath(string GO_Item, ProductGroup currentProduct, int pgNumber)
        {
            string output = "";
            try
            {
                DataTable dt;
                string query = "";
               
                if (currentProduct == ProductGroup.PRL123)
                {
                    query = "select [ImageFilePath] from [PRL123] where [GO_Item]='" + GO_Item + "' and [PageNumber]=" + pgNumber.ToString();;
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
        public static BitmapImage RetrieveBinaryBidman(string GO_Item)
        {
            BitmapImage img = new BitmapImage();
            try
            {
                DataTableReader rd = LoadData("select [Bidman] from [PRL123] where [GO_Item]='" + GO_Item + "'");
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
        public static string GetDirectoryForOrderFiles(string SelectedGO, ProductGroup CurrentProduct)
        {
            DataTable dt;
            string query = "";
            string output = "";

            if (CurrentProduct == ProductGroup.PRL123)
            {
                query = "select [FilePath] from [PRL123] where [GO_Item]='" + SelectedGO + "' AND [PageNumber]=0";
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
        public static int ExecuteAddToShopPack(string SelectedGO, ProductGroup CurrentProduct)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf"
                };
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string insertGOI = SelectedGO;
                    string insertGO = SelectedGO.Substring(0, 10);
                    string SOInterior = "";
                    string SOBox = "";
                    string SOTrim = "";
                    string SchedGroup = "";
                    string Customer = "";
                    string Quantity = "";

                    DateTime? EnterDate = null;
                    DateTime? ReleaseDate = null;
                    DateTime? CommitDate = null;

                    string Tracking = "";
                    string Urgency = "";

                    Boolean SpecialCustomer = false;
                    Boolean AMO = false;
                    Boolean PaintedBox = false;

                    //PRL4 Specific
                    Boolean ServiceEntrance = false;
                    Boolean RatedNeutral200 = false;
                    Boolean DoorOverDist = false;
                    Boolean DoorInDoor = false;
                    Boolean BoxEarly = false;
                    Boolean BoxSent = false;
                        
                    //PRLCS Checklist
                    Boolean IncLocLeft = false;
                    Boolean IncLocRight = false;
                    Boolean CrossBus = false;
                    Boolean OpenBottom = false;
                    Boolean ExtendedTop = false;
                    Boolean ThirtyDeepEncolsure = false;

                    Boolean NameplateRequired = false;
                    Boolean NameplateOrdered = false;
                    Boolean DNSB = false;
                    Boolean Complete = false;
                    Boolean Short = false;

                    //PRL123 Specific
                    Boolean DoubleSection = false;
                   

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
                    
                    string notes = "";
                    Boolean LabelsPrinted = false;

                    string query = "";
                    if (CurrentProduct == ProductGroup.PRL123) 
                    {
                        query = "select [ShopOrderInterior],"
                              + "[ShopOrderBox],"
                              + "[ShopOrderTrim],"
                              + "[SchedulingGroup],"
                              + "[Customer],"
                              + "[Quantity],"
                              + "[EnteredDate],"
                              + "[ReleaseDate],"
                              + "[CommitDate],"
                              + "[Tracking],"
                              + "[Urgency],"
                              + "[SpecialCustomer],"
                              + "[AMO],"
                              + "[PaintedBox],"
                              + "[NameplateRequired],"
                              + "[NameplateOrdered],"
                              + "[DNSB],"
                              + "[Complete],"
                              + "[Short],"
                              + "[Type],"
                              + "[Volts],"
                              + "[Amps],"
                              + "[Torque],"
                              + "[Appearance],"
                              + "[Bus],"
                              + "[Catalogue],"
                              + "[ProductSpecialist],"
                              + "[FilePath],"
                              + "[ImageFilePath],"
                              + "[PageNumber],"
                              + "[Notes],"
                              + "[LabelsPrinted],"
                              + "[ServiceEntrance],"
                              + "[RatedNeutral200],"
                              + "[BoxEarly],"
                              + "[Box Sent],"
                              + "[DoubleSection] "
                              + "from [PRL123] "
                              + "where [GO_Item]='" + SelectedGO + "' "
                              + "order by [GO_Item],[PageNumber]";
                    }
                    else if (CurrentProduct == ProductGroup.PRL4)
                    {
                        query = "select [ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[SchedulingGroup],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[PaintedBox],[NameplateRequired],[NameplateOrdered],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber],[Notes],[LabelsPrinted],[ServiceEntrance],[RatedNeutral200],[DoorOverDist],[DoorInDoor],[BoxEarly],[BoxSent] from [PRL4] where [GO_Item]='" + SelectedGO + "' order by [GO_Item],[PageNumber]";
                    }
                    else if(CurrentProduct == ProductGroup.PRLCS)
                    {                        
                        query = "select [ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[SchedulingGroup],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[PaintedBox],[NameplateRequired],[NameplateOrdered],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber],[Notes],[LabelsPrinted],[IncLocLeft],[IncLocRight],[CrossBus],[OpenBottom],[ExtendedTop],[ThirtyDeepEnclosure] from [PRLCS] where [GO_Item]='" + SelectedGO + "' order by [GO_Item],[PageNumber]";
                    }
                    using (DataTableReader dtr = LoadData(query))
                    {
                        while (dtr.Read())
                        {
                            SOInterior = dtr[0].ToString();
                            SOBox = dtr[1].ToString();
                            SOTrim = dtr[2].ToString();
                            SchedGroup = dtr[3].ToString();
                            Customer = dtr[4].ToString();
                            Quantity = dtr[5].ToString();

                            if (string.IsNullOrEmpty(dtr[6].ToString())) EnterDate = null; else EnterDate = (DateTime?)Convert.ToDateTime(dtr[6].ToString());
                            if (string.IsNullOrEmpty(dtr[7].ToString())) ReleaseDate = null; else ReleaseDate = (DateTime?)Convert.ToDateTime(dtr[7].ToString());
                            if (string.IsNullOrEmpty(dtr[8].ToString())) CommitDate = null; else CommitDate = (DateTime?)Convert.ToDateTime(dtr[8].ToString());

                            Tracking = dtr[9].ToString();
                            Urgency = dtr[10].ToString();

                            SpecialCustomer = (Boolean)dtr[11];
                            AMO = (Boolean)dtr[12];
                            PaintedBox = (Boolean)dtr[13];
                            NameplateRequired = (Boolean)dtr[14];
                            NameplateOrdered = (Boolean)dtr[15];
                            DNSB = (Boolean)dtr[16];
                            Complete = (Boolean)dtr[17];
                            Short = (Boolean)dtr[18];
                            type = dtr[19].ToString();
                            volts = dtr[20].ToString();
                            amps = dtr[21].ToString();
                            torque = dtr[22].ToString();
                            appearance = dtr[23].ToString();
                            bus = dtr[24].ToString();
                            catalogue = dtr[25].ToString();
                            prodSpecialist = dtr[26].ToString();

                            pdfFilepath = dtr[27].ToString();
                            imageFilepath = dtr[28].ToString();
                            pageNumber = (int)dtr[29];

                            notes = dtr[30].ToString();
                            LabelsPrinted = (Boolean)dtr[31];


                            if (CurrentProduct == ProductGroup.PRL123) 
                            {
                                ServiceEntrance = (Boolean)dtr[32];
                                RatedNeutral200 = (Boolean)dtr[33];
                                BoxEarly = (Boolean)dtr[34];
                                BoxSent = (Boolean)dtr[35];
                                DoubleSection = (Boolean)dtr[36];

                            }
                            else if (CurrentProduct == ProductGroup.PRL4)
                            {
                                ServiceEntrance = (Boolean)dtr[32];
                                RatedNeutral200 = (Boolean)dtr[33];
                                DoorOverDist = (Boolean)dtr[34];
                                DoorInDoor = (Boolean)dtr[35];
                                BoxEarly = (Boolean)dtr[36];
                                BoxSent = (Boolean)dtr[37];
                            }
                            else if (CurrentProduct == ProductGroup.PRLCS)
                            {
                                IncLocLeft = (Boolean)dtr[32];
                                IncLocRight = (Boolean)dtr[33];
                                CrossBus = (Boolean)dtr[34];
                                OpenBottom = (Boolean)dtr[35];
                                ExtendedTop = (Boolean)dtr[36];
                                ThirtyDeepEncolsure = (Boolean)dtr[37];
                            }                            
                        }
                    }

                    pageNumber += 1;
                    imageFilepath = System.IO.Path.GetFullPath(System.IO.Path.Combine(imageFilepath, @"..\" + SelectedGO.Substring(0, 10) + "_" + SelectedGO.Substring(11, SelectedGO.Length - 11) + "_" + pageNumber.ToString() + ".png"));


                    if (CurrentProduct == ProductGroup.PRL123)
                    {
                        string command = "INSERT INTO [PRL123] ([GO_Item],[GO],[ShopOrderInterior],[ShopOrderBox],"
                            + "[ShopOrderTrim],[SchedulingGroup],[Customer],[Quantity],"
                            + "[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],"
                            + "[Urgency],[SpecialCustomer],[AMO],[ServiceEntrance],"
                            + "[PaintedBox],[RatedNeutral200],[BoxEarly],[Box Sent],"
                            + "[NameplateRequired],[NameplateOrdered],[DNSB],[Complete],"
                            + "[Type],[Volts],[Amps],[Torque],"
                            + "[Appearance],[Bus],[Catalogue],[ProductSpecialist],"
                            + "[FilePath],[ImageFilePath],[PageNumber],[Notes],"
                            + "[LabelsPrinted], [DoubleSection], [Short]) "
                            + "VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";


                        using (OleDbCommand cmd = new OleDbCommand(command, MainWindow.LPcon))
                        {
                            cmd.Parameters.AddWithValue("[GO_Item]", insertGOI);
                            cmd.Parameters.AddWithValue("[GO]", insertGO);
                            cmd.Parameters.AddWithValue("[ShopOrderInterior]", SOInterior);
                            cmd.Parameters.AddWithValue("[ShopOrderBox]", SOBox);
                            cmd.Parameters.AddWithValue("[ShopOrderTrim]", SOTrim);
                            cmd.Parameters.AddWithValue("[SchedulingGroup]", SchedGroup);
                            cmd.Parameters.AddWithValue("[Customer]", Customer);
                            cmd.Parameters.AddWithValue("[Quantity]", Quantity);

                            if (EnterDate == null) cmd.Parameters.AddWithValue("EnteredDate", DBNull.Value); else cmd.Parameters.AddWithValue("EnteredDate", EnterDate);
                            if (ReleaseDate == null) cmd.Parameters.AddWithValue("ReleaseDate", DBNull.Value); else cmd.Parameters.AddWithValue("ReleaseDate", ReleaseDate);
                            if (CommitDate == null) cmd.Parameters.AddWithValue("CommitDate", DBNull.Value); else cmd.Parameters.AddWithValue("CommitDate", CommitDate);

                            cmd.Parameters.AddWithValue("[Tracking]", Tracking);
                            cmd.Parameters.AddWithValue("[Urgency]", Urgency);
                            cmd.Parameters.AddWithValue("[SpecialCustomer]", SpecialCustomer);
                            cmd.Parameters.AddWithValue("[AMO]", AMO);
                            cmd.Parameters.AddWithValue("ServiceEntrance", ServiceEntrance);
                            cmd.Parameters.AddWithValue("[PaintedBox]", PaintedBox);
                            cmd.Parameters.AddWithValue("[RatedNeutral200]", RatedNeutral200);
                            cmd.Parameters.AddWithValue("[BoxEarly]", BoxEarly);
                            cmd.Parameters.AddWithValue("[Box Sent]", BoxSent);
                            cmd.Parameters.AddWithValue("NameplateRequired", NameplateRequired);
                            cmd.Parameters.AddWithValue("NameplateOrdered", NameplateOrdered);
                            cmd.Parameters.AddWithValue("DNSB", DNSB);
                            cmd.Parameters.AddWithValue("Complete", Complete);
                            cmd.Parameters.AddWithValue("[Type]", type);
                            cmd.Parameters.AddWithValue("[Volts]", volts);
                            cmd.Parameters.AddWithValue("Amps", amps);
                            cmd.Parameters.AddWithValue("Torque", torque);
                            cmd.Parameters.AddWithValue("Appearance", appearance);
                            cmd.Parameters.AddWithValue("Bus", bus);
                            cmd.Parameters.AddWithValue("Catalogue", catalogue);
                            cmd.Parameters.AddWithValue("ProductSpecialist", prodSpecialist);
                            cmd.Parameters.AddWithValue("FilePath", pdfFilepath);
                            cmd.Parameters.AddWithValue("ImageFilePath", imageFilepath);
                            cmd.Parameters.AddWithValue("PageNumber", pageNumber);
                            cmd.Parameters.AddWithValue("Notes", notes);
                            cmd.Parameters.AddWithValue("LabelsPrinted", LabelsPrinted);
                            cmd.Parameters.AddWithValue("[DoubleSection]", DoubleSection);
                            cmd.Parameters.AddWithValue("[Short]", Short);



                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (CurrentProduct == ProductGroup.PRL4)
                    {
                        string command = "INSERT INTO [PRL4]([GO_Item],[GO],[ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[SchedulingGroup],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[ServiceEntrance],[PaintedBox],[RatedNeutral200],[DoorOverDist],[DoorInDoor],[BoxEarly],[BoxSent],[NameplateRequired],[NameplateOrdered],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber],[Notes],[LabelsPrinted]) " +
                                  "VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                        using (OleDbCommand cmd = new OleDbCommand(command, MainWindow.LPcon))
                        {
                            cmd.Parameters.AddWithValue("GO_Item", insertGOI);
                            cmd.Parameters.AddWithValue("GO", insertGO);
                            cmd.Parameters.AddWithValue("ShopOrderInterior", SOInterior);
                            cmd.Parameters.AddWithValue("ShopOrderBox", SOBox);
                            cmd.Parameters.AddWithValue("ShopOrderTrim", SOTrim);
                            cmd.Parameters.AddWithValue("SchedulingGroup", SchedGroup);
                            cmd.Parameters.AddWithValue("Customer", Customer);
                            cmd.Parameters.AddWithValue("Quantity", Quantity);

                            if (EnterDate == null) cmd.Parameters.AddWithValue("EnteredDate", DBNull.Value); else cmd.Parameters.AddWithValue("EnteredDate", EnterDate);
                            if (ReleaseDate == null) cmd.Parameters.AddWithValue("ReleaseDate", DBNull.Value); else cmd.Parameters.AddWithValue("ReleaseDate", ReleaseDate);
                            if (CommitDate == null) cmd.Parameters.AddWithValue("CommitDate", DBNull.Value); else cmd.Parameters.AddWithValue("CommitDate", CommitDate);

                            cmd.Parameters.AddWithValue("Tracking", Tracking);
                            cmd.Parameters.AddWithValue("Urgency", Urgency);
                            cmd.Parameters.AddWithValue("SpecialCustomer", SpecialCustomer);
                            cmd.Parameters.AddWithValue("AMO", AMO);
                            cmd.Parameters.AddWithValue("ServiceEntrance", ServiceEntrance);
                            cmd.Parameters.AddWithValue("PaintedBox", PaintedBox);
                            cmd.Parameters.AddWithValue("RatedNeutral200", RatedNeutral200);
                            cmd.Parameters.AddWithValue("DoorOverDist", DoorOverDist);
                            cmd.Parameters.AddWithValue("DoorInDoor", DoorInDoor);
                            cmd.Parameters.AddWithValue("BoxEarly", BoxEarly);
                            cmd.Parameters.AddWithValue("BoxSent", BoxSent);
                            cmd.Parameters.AddWithValue("NameplateRequired", NameplateRequired);
                            cmd.Parameters.AddWithValue("NameplateOrdered", NameplateOrdered);
                            cmd.Parameters.AddWithValue("DNSB", DNSB);
                            cmd.Parameters.AddWithValue("Complete", Complete);
                            cmd.Parameters.AddWithValue("Short", Short);
                            cmd.Parameters.AddWithValue("Type", type);
                            cmd.Parameters.AddWithValue("Volts", volts);
                            cmd.Parameters.AddWithValue("Amps", amps);
                            cmd.Parameters.AddWithValue("Torque", torque);
                            cmd.Parameters.AddWithValue("Appearance", appearance);
                            cmd.Parameters.AddWithValue("Bus", bus);
                            cmd.Parameters.AddWithValue("Catalogue", catalogue);
                            cmd.Parameters.AddWithValue("ProductSpecialist", prodSpecialist);
                            cmd.Parameters.AddWithValue("FilePath", pdfFilepath);
                            cmd.Parameters.AddWithValue("ImageFilePath", imageFilepath);
                            cmd.Parameters.AddWithValue("PageNumber", pageNumber);
                            cmd.Parameters.AddWithValue("Notes", notes);
                            cmd.Parameters.AddWithValue("LabelsPrinted", LabelsPrinted);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (CurrentProduct == ProductGroup.PRLCS)
                    {
                        string command = "INSERT INTO [PRLCS]([GO_Item],[GO],[ShopOrderInterior],[ShopOrderBox],[ShopOrderTrim],[SchedulingGroup],[Customer],[Quantity],[EnteredDate],[ReleaseDate],[CommitDate],[Tracking],[Urgency],[SpecialCustomer],[AMO],[IncLocLeft],[IncLocRight],[CrossBus],[OpenBottom],[ExtendedTop],[PaintedBox],[ThirtyDeepEnclosure],[NameplateRequired],[NameplateOrdered],[DNSB],[Complete],[Short],[Type],[Volts],[Amps],[Torque],[Appearance],[Bus],[Catalogue],[ProductSpecialist],[FilePath],[ImageFilePath],[PageNumber],[Notes],[LabelsPrinted]) " +
                                  "VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                        using (OleDbCommand cmd = new OleDbCommand(command, MainWindow.LPcon))
                        {
                            cmd.Parameters.AddWithValue("GO_Item", insertGOI);
                            cmd.Parameters.AddWithValue("GO", insertGO);
                            cmd.Parameters.AddWithValue("ShopOrderInterior", SOInterior);
                            cmd.Parameters.AddWithValue("ShopOrderBox", SOBox);
                            cmd.Parameters.AddWithValue("ShopOrderTrim", SOTrim);
                            cmd.Parameters.AddWithValue("SchedulingGroup", SchedGroup);
                            cmd.Parameters.AddWithValue("Customer", Customer);
                            cmd.Parameters.AddWithValue("Quantity", Quantity);

                            if (EnterDate == null) cmd.Parameters.AddWithValue("EnteredDate", DBNull.Value); else cmd.Parameters.AddWithValue("EnteredDate", EnterDate);
                            if (ReleaseDate == null) cmd.Parameters.AddWithValue("ReleaseDate", DBNull.Value); else cmd.Parameters.AddWithValue("ReleaseDate", ReleaseDate);
                            if (CommitDate == null) cmd.Parameters.AddWithValue("CommitDate", DBNull.Value); else cmd.Parameters.AddWithValue("CommitDate", CommitDate);

                            cmd.Parameters.AddWithValue("Tracking", Tracking);
                            cmd.Parameters.AddWithValue("Urgency", Urgency);
                            cmd.Parameters.AddWithValue("SpecialCustomer", SpecialCustomer);
                            cmd.Parameters.AddWithValue("AMO", AMO);
                            cmd.Parameters.AddWithValue("IncLocLeft", IncLocLeft);
                            cmd.Parameters.AddWithValue("IncLocRight", IncLocRight);
                            cmd.Parameters.AddWithValue("CrossBus", CrossBus);
                            cmd.Parameters.AddWithValue("OpenBottom", OpenBottom);
                            cmd.Parameters.AddWithValue("ExtendedTop", ExtendedTop);
                            cmd.Parameters.AddWithValue("PaintedBox", PaintedBox);
                            cmd.Parameters.AddWithValue("ThirtyDeepEncolsure", ThirtyDeepEncolsure);
                            cmd.Parameters.AddWithValue("NameplateRequired", NameplateRequired);
                            cmd.Parameters.AddWithValue("NameplateOrdered", NameplateOrdered);
                            cmd.Parameters.AddWithValue("DNSB", DNSB);
                            cmd.Parameters.AddWithValue("Complete", Complete);
                            cmd.Parameters.AddWithValue("Short", Short);
                            cmd.Parameters.AddWithValue("Type", type);
                            cmd.Parameters.AddWithValue("Volts", volts);
                            cmd.Parameters.AddWithValue("Amps", amps);
                            cmd.Parameters.AddWithValue("Torque", torque);
                            cmd.Parameters.AddWithValue("Appearance", appearance);
                            cmd.Parameters.AddWithValue("Bus", bus);
                            cmd.Parameters.AddWithValue("Catalogue", catalogue);
                            cmd.Parameters.AddWithValue("ProductSpecialist", prodSpecialist);
                            cmd.Parameters.AddWithValue("FilePath", pdfFilepath);
                            cmd.Parameters.AddWithValue("ImageFilePath", imageFilepath);
                            cmd.Parameters.AddWithValue("PageNumber", pageNumber);
                            cmd.Parameters.AddWithValue("Notes", notes);
                            cmd.Parameters.AddWithValue("LabelsPrinted", LabelsPrinted);

                            cmd.ExecuteNonQuery();
                        }
                    }


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
            catch (Exception ex)
            {
                MessageBox.Show($"An Error Occurred Trying To AddToShopPackage: {ex.Message}");
                return -2;
            }
        }


        // The following is used for the feature CONSTR insert
        public static int ExecuteCONSTRinsert(string SelectedGO, string pdfDirectory, ProductGroup currentProduct)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string filepath = ofg.FileName;

                    string fileExtenstion = System.IO.Path.GetExtension(filepath);
                    string newPath;
                    if (HasImageFilePath(SelectedGO, currentProduct))
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
        public static int ExecuteBLTinsert(string SelectedGO, string pdfDirectory, ProductGroup currentProduct)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog ofg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg|pdf files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };
                bool? response = ofg.ShowDialog();

                if (response == true)           //if the user selects a file and clicks OK
                {
                    string filepath = ofg.FileName;

                    string fileExtenstion = System.IO.Path.GetExtension(filepath);
                    string newPath;
                    if (HasImageFilePath(SelectedGO, currentProduct))
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

            using (DataTableReader dtr = LoadData(query))
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
        public static Boolean IsDuplicate(string GO_Item, ProductGroup CurrentProduct)
        {
            try
            {
                string query = "";
                if (CurrentProduct == ProductGroup.PRL123)
                {
                    query = "select [GO_Item] from [PRL123] where [GO_Item]='" + GO_Item + "' and [PageNumber]=0";
                }
                else if (CurrentProduct == ProductGroup.PRL4)
                {
                    query = "select [GO_Item] from [PRL4] where [GO_Item]='" + GO_Item + "' and [PageNumber]=0";
                }
                else if (CurrentProduct == ProductGroup.PRLCS)
                {
                    query = "select [GO_Item] from [PRLCS] where [GO_Item]='" + GO_Item + "' and [PageNumber]=0";
                }
                using (DataTableReader dtr = LoadData(query))
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



        // The following is used to access the ReplacementParts table - 1-1 mapping only
        //public static string ReplacePart(string part)
        //{
        //    string replacement = part;
        //    try
        //    {
        //        string query = "select [Replace] from [ReplacementParts] where [Find]='" + part + "'";
        //        using (DataTableReader dtr = LoadData(query))
        //        {
        //            while (dtr.Read())
        //            {
        //                if (dtr[0] != null)
        //                {
        //                    replacement = dtr[0].ToString();    //there is a replacement available
        //                }
        //            }
        //        } //end using reader
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Unable To Replace Part");
        //    }
        //    return replacement;
        //}



        // The following is used to access the ReplacementParts table - with the possibility of mapping 1-1 or 1-Many
        public static List<string> ReplacementParts(string part)
        {
            List<string> ReplacementPartsList = new List<string>();
            try
            {
                string query = "select [Replace] from [ReplacementParts] where [Find]='" + part + "'";
                using (DataTableReader dtr = LoadData(query))
                {
                    if (dtr.HasRows == false)
                    {
                        ReplacementPartsList.Add(part);
                        return ReplacementPartsList;
                    }
                    while (dtr.Read())
                    {
                        if (dtr[0] != null)             //there is a replacement available
                        {
                            ReplacementPartsList.Add(dtr[0].ToString());    
                        }
                    }
                } //end using reader
            }
            catch
            {
                MessageBox.Show("Unable To Execute ReplacementParts");
            }
            return ReplacementPartsList;
        }




        // The following is used to search the PullSequence table for a part, if the part is not there => it is standardAMO   
        public static Boolean StandardAMO(string part)
        {
            try
            {
                int ID = -10;
                string query = "select [ID] from [PullSequence] where [Item]='" + part + "'";
                using (DataTableReader dtr = LoadData(query))
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
                using (DataTableReader dtr = LoadData(query))
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
                using (DataTableReader dtr = LoadData(query))
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
        public static string GetMaxVoltage(string fullVoltage)
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
                    string voltage1 = GetNumberInString(voltages[0]);
                    string voltage2 = GetNumberInString(voltages[1]);

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
                    return GetNumberInString(fullVoltage);
                }
            }
        }


        // The following is used to extract a number from a specified string
        public static string GetNumberInString(string strSource)
        {
            string[] numbers;
            numbers = Regex.Split(strSource, @"\D+");

            return numbers[0];
        }


        // The following is used to truncate a source string given a start string and an end string
        public static string GetBetween(string strSource, string strStart, string strEnd)
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


        // The following is used to determine if a string contains digits only
        public static Boolean IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Xml.Serialization;

namespace LightningPRO
{
    public class XmlParser<T>
    {
        public T Parse(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        return (T)serializer.Deserialize(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing XML: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("XML file not found.");
            }

            return default(T);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LightningPRO
{
    internal class ParseXML
    {

        public string xmlFilePath;
        public XDocument xmlDoc;
        Dictionary<string, Dictionary<string, XMLParsingRule>> RuleSet { get; set; }
        private KickoffFunctionDelegate _kickoffFunction;

        //How to use this class
        //Initialize the class with the xml
        

        //Notes: Need to look for descendents








        public ParseXML(string xmlFilePath, KickoffFunctionDelegate kickoffFunction) {
            this.xmlFilePath = xmlFilePath;
            this.xmlDoc = XDocument.Load(xmlFilePath);
            _kickoffFunction = kickoffFunction;
            
            
            
            
           
        }

        public List<string> Parse()
        {
            List<string>  str = _kickoffFunction?.Invoke(xmlDoc);
            return str;
        }

    }

    
    public class XMLParsingRule
    {
        public string AttributeName { get; set; }
        public string SecondAttributeName { get; set; }
        public XElement XmlContext { get; set; }
        public Func<string, string, XElement, string> ParseFunc { get; set; } = (v1, v2, xml) => v1;
        public string Value { get; set; }
    }

    delegate List<string> KickoffFunctionDelegate(XDocument xmldoc);
}

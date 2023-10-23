using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightningPRO
{
    public class ConfigurationStorage
    {
       
        //This page is used to store and use all configuration information that may be required throughout the app
        
        //access these fields globally in the app
        public static string[] addressLocation = ConfigurationManager.ConnectionStrings["locationAddress"].ToString().Split('/');          //each index has an address line for TEST Report and last index has location abbreviation 
        public static string[] specialCustomer = ConfigurationManager.ConnectionStrings["specialCustomersList"].ToString().Split('/');         //each index has a special customer - or index[0] has "NONE"

        //product names as they appear under [Prod Group] in [tblOrderStatus] 
        public static string[] PRL123names;
        public static string[] PRL4names;
        public static string[] PRLCSnames;
        public static string[] ECnames;

        public static void SetProductNames()
        {
            string[] productlines = ConfigurationManager.ConnectionStrings["productNameList"].ToString().Split('/');
            PRL123names = productlines[0].Split(',');
            PRL4names = productlines[1].Split(',');
            PRLCSnames = productlines[2].Split(',');
            ECnames = productlines[3].Split(',');
        }

    }
}

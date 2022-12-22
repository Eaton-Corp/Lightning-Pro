using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRL123_Final.Models
{
    public class GOItem
    {
        public string ID { get; set; }
        public string GO_Item { get; set; }

        public string GO { get; set; }

        public string Suffix { get; set; }

        public string ShopOrderInterior { get; set; }

        public string ShopOrderBox { get; set; }

        public string ShopOrderTrim { get; set; }

        public string Customer { get; set; }

        public string Quantity { get; set; }

        public string EnteredDate { get; set; }

        public string ReleaseDate { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string CommitDate { get; set; }

        public string ItemStatus{ get; set; }

        public string Tracking { get; set; }

        public string Urgency { get; set; }
        
        public byte[] Bidman { get; set; }

        public Boolean BoxEarly { get; set; }

        public Boolean AMO { get; set; }

        public Boolean VaughanElectric { get; set; }

        public string Type { get; set; }

        public string Volts { get; set; }

        public string Amps { get; set; }

        public string Torque { get; set; }

        public string Appearance { get; set; }

        public string Bus { get; set; }

        public Boolean Complete { get; set; }

        public Boolean Short { get; set; }

    }
}

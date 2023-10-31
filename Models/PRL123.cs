using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace LightningPRO.Models
{
    [Table(Name = "PRL123")]
    public class PRL123
    {
        [PrimaryKey, Identity]
        public int ID { get; set; }

        [Column, NotNull]
        public string GO_Item { get; set; }
        [Column]
        public string GO { get; set; }
        [Column]
        public string Suffix { get; set; }
        [Column]
        public string ShopOrderInterior { get; set; }
        [Column] 
        public string ShopOrderBox { get; set; }
        [Column]
        public string ShopOrderTrim { get; set; }
        [Column] 
        public string Customer { get; set; }
        [Column] 
        public string Quantity { get; set; }
        [Column] 
        public DateTime EnteredDate { get; set; }
        [Column] 
        public DateTime ReleaseDate { get; set; }
        [Column] 
        public DateTime StartDate { get; set; }
        [Column] 
        public DateTime EndDate { get; set; }
        [Column] 
        public DateTime CommitDate { get; set; }
        [Column] 
        public string ItemStatus { get; set; }
        [Column] 
        public string Tracking { get; set; }
        [Column] 
        public string Urgency { get; set; }
        [Column] 
        public byte[] Bidman { get; set; }  // OLE Object usually indicates binary data
        [Column] 
        public bool BoxEarly { get; set; }
        [Column] 
        public bool BoxSent { get; set; }
        [Column] 
        public bool SpecialCustomer { get; set; }
        [Column] 
        public bool AMO { get; set; }
        [Column] 
        public bool ServiceEntrance { get; set; }
        [Column] 
        public bool DoubleSection { get; set; }
        [Column] 
        public bool PaintedBox { get; set; }
        [Column] 
        public bool RatedNeutral200 { get; set; }
        [Column] 
        public bool DNSB { get; set; }
        [Column] 
        public bool Complete { get; set; }
        [Column] 
        public string Type { get; set; }
        [Column] 
        public string Volts { get; set; }
        [Column] 
        public string Amps { get; set; }
        [Column] 
        public string Torque { get; set; }
        [Column] 
        public string Appearance { get; set; }
        [Column] 
        public string Bus { get; set; }
        [Column] 
        public string Catalogue { get; set; }
        [Column] 
        public string ProductSpecialist { get; set; }
        [Column] 
        public string FilePath { get; set; }
        [Column] 
        public string ImageFilePath { get; set; }
        [Column] 
        public string Notes { get; set; }
        [Column] 
        public string LastSave { get; set; }
        [Column] 
        public bool LabelsPrinted { get; set; }
        [Column] 
        public bool NameplateOrdered { get; set; }
        [Column] 
        public bool NameplateRequired { get; set; }
        [Column] 
        public string SchedulingGroup { get; set; }
        [Column]
        public int PageNumber { get; set; }
    }
}
using System;
using System.Linq;
using LinqToDB.Mapping;

namespace LightningPRO.Models
{
    [Table(Name = "PRLCS")]
    public class PRLCS
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
        public byte[] Bidman { get; set; } // Assuming OLE Object is binary data

        [Column]
        public bool SpecialCustomer { get; set; }

        [Column]
        public bool AMO { get; set; }

        [Column]
        public bool InclLeft { get; set; }

        [Column]
        public bool InclRight { get; set; }

        [Column]
        public bool CrossBus { get; set; }

        [Column]
        public bool OpenBottom { get; set; }

        [Column]
        public bool ExtendedTop { get; set; }

        [Column]
        public bool PaintedBox { get; set; }

        [Column]
        public bool ThirtyDeepEnclosure { get; set; }

        [Column]
        public bool DNSB { get; set; }

        [Column]
        public bool Complete { get; set; }

        [Column]
        public bool Short { get; set; }

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
        public int PageNumber { get; set; }

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
    }
}

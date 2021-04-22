using System;
using System.Collections.Generic;
using System.Text;
//using MySqlConnector;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TESCopper
{
    [XmlRoot(ElementName = "OrderData")]
    public class OrderData
    {
        [XmlElement(ElementName = "ID")]
        public string Id                                   { get; set; }

        [XmlElement(ElementName = "ProjectName")]
        public string ProjName                             { get; set; }

        [XmlElement(ElementName = "PONumber")]
        public string PONumber                             { get; set; }

        [XmlElement(ElementName = "CreatedOn")]
        public DateTime CreatedOn                          { get; set; }

        [XmlElement(ElementName = "DueOn")]
        public DateTime DueOn                              { get; set; }

        [XmlElement(ElementName = "Instructions")]
        public string Instructions                         { get; set; }

        [XmlElement(ElementName = "Rush")]
        public bool ShouldRushOrder                        { get; set; }

        [XmlElement(ElementName = "KeepOnFile")]
        public bool KeepOnFile                             { get; set; }

        [XmlElement(ElementName = "PrintType")]
        public PrintOutType PrintOutType                   { get; set; }

        [XmlElement(ElementName = "Contact")]
        public ContactInfo[] Contacts                      { get; set; }

        [XmlElement(ElementName = "ShippingInfos")]
        public ShippingInfo[] ShippingAddresses            { get; set; }

        public override string ToString()
        {
            return String.Format(@"
            ID: {0}
            ProjectName: {1}
            Instructions: {2}
            PONumber: {3}
            CreatedOn: {4}
            DueOn: {5}
            Rush?: {6}
            Keep On File: {7}
            Print Type: {8}
            Contact >
            Shipping Info: 
", 
             Id,
             ProjName,
             Instructions,
             PONumber,
             CreatedOn,
             DueOn,
             ShouldRushOrder ? "yes" : "no",
             KeepOnFile ? "yes" : "no",
             Enum.GetName(typeof(PrintOutType),PrintOutType)
             );
        }
    }

    [XmlRoot(ElementName = "Contact")]
    public class ContactInfo
    {
        [XmlElement(ElementName = "ID")]
        public string Id            { get; set; }

        [XmlElement(ElementName = "CompanyName")]
        public string CompanyName   { get; set; }

        [XmlElement(ElementName = "FirstName")]
        public string FirstName     { get; set; }

        [XmlElement(ElementName = "LastName")]
        public string LastName      { get; set; }

        [XmlElement(ElementName = "StreetAddress")]
        public string StreetAddress { get; set; }

        [XmlElement(ElementName = "City")]
        public string City          { get; set; }

        [XmlElement(ElementName = "State")]
        public string State         { get; set; }

        [XmlElement(ElementName = "ZipCode")]
        public string ZipCode       { get; set; }

        [XmlElement(ElementName = "PhoneNumber")]
        public string PhoneNumber   { get; set; }

        [XmlElement(ElementName = "Email")]
        public string Email         { get; set; }

    }

    [XmlRoot(ElementName = "Shipping")]
    public class ShippingInfo
    {
        [XmlElement(ElementName = "ID")]
        public string Id                    { get; set; }

        [XmlElement(ElementName = "ShippingInfo")]
        public ContactInfo MainShippingInfo { get; set; }

        [XmlElement(ElementName = "ShipType")]
        public ShippingInfo ShipType        { get; set; }
    }

    public enum ShippingType
    {
        HLD_PICKUP = 0,
        HND_DEL,
        PRIORITY_OVN,
        STND_OVN,
        SND_AIR
    }

    public enum PrintOutType
    {
        BLACK_WHITE = 0,
        COLOR,
        MIXED
    }
}

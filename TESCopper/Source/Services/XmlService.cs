using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace TESCopper
{
    class XmlService
    {
        private const string DEFAULT_XML_SETUP = @"
<Data>
  <Orders>
    <OpenOrders>
      
    </OpenOrders>
    <ClosedOrders>
      
    </ClosedOrders>
  </Orders>
  <Contacts>
    <Clients>
      
    </Clients>
  </Contacts>
</Data>
";
        private const string XPATH_OPENORDERS = "Data/Orders/OpenOrders";
        private const string XPATH_CLOSEDORDERS = "Data/Orders/ClosedOrders";
        private const string XPATH_CLIENTS = "Data/Contacts/Clients";
        private const string XPATH_SHIPPING = "Data/Contacts/Clients/ShippingInfo";

        private XDocument DataDoc;

        public static string DocumentFileName
        {
            get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "dataset.xml");
        }
        public static string DocumentFolder 
        {
            get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        }
        public XmlService()
        {
              DataDoc = XDocument.Load(DocumentFileName);
        }
        ~XmlService()
        {
            DataDoc = null;
        }

        public static void SetupNewXmlFile(string fullFileName)
        {
            XElement newDocument = XElement.Parse(DEFAULT_XML_SETUP);
            using (StringWriter writer = new StringWriter())
                newDocument.Save(DocumentFileName);
        }

        
        public IEnumerable<OrderData> GetOpenOrderDetails()
        {
            IEnumerable<XNode> elements = DataDoc.XPathSelectElements(XPATH_OPENORDERS + "/OrderData");
            List<OrderData> orders = new List<OrderData>();

            foreach (var element in elements)
                orders.Add(DeserializeElement<OrderData>(element));

            return orders.ToArray();
        }
        public IEnumerable<OrderData> GetClosedOrderDetails()
        {
            IEnumerable<XElement> elements = DataDoc.XPathSelectElements(XPATH_CLOSEDORDERS + "/OrderData");
            List<OrderData> orders = new List<OrderData>();

            foreach (var element in elements)
                orders.Add(DeserializeElement<OrderData>(element));

            return orders.ToArray();
        }
        public IEnumerable<ContactInfo> GetContactInfo()
        {
            IEnumerable<XElement> elements = DataDoc.XPathSelectElements(XPATH_CLIENTS);
            List<ContactInfo> contacts = new List<ContactInfo>();

            foreach (var element in elements)
                contacts.Add(DeserializeElement<ContactInfo>(element));

            return contacts.ToArray();
        }
        public IEnumerable<ShippingInfo> GetShippingInfo(string clientID)
        {
            IEnumerable<XElement> elements = DataDoc.XPathSelectElements(
                string.Format("//ContactInfo[Id='{0}']", clientID));
            List<ShippingInfo> shippingData = new List<ShippingInfo>();

            foreach (var element in elements)
                shippingData.Add(DeserializeElement<ShippingInfo>(element));

            return shippingData.ToArray();
        }

        public void RemoveOpenOrderDetails(string orderId)
        {
            try
            {
                XNode node = DataDoc.XPathSelectElement(XPATH_OPENORDERS);
                string s = string.Format("OrderData[ID='{0}']", orderId);
                node.XPathSelectElement(s).Remove();
            }
            catch(Exception e)
            {
                Console.WriteLine("REMOVE ELEMENT FUNCTION FAILED! Try Removing the Object Again. If it Fails then its a bug.");
                Console.WriteLine(e);
            }
        }
        public void RemoveCloseOrderDetails(string orderId)
        {
            try
            {
                XNode node = DataDoc.XPathSelectElement(XPATH_CLOSEDORDERS);
                node.XPathSelectElement(string.Format("OrderData[ID='{0}']", orderId)).Remove();
            }
            catch (Exception e)
            {
                Console.WriteLine("REMOVE ELEMENT FUNCTION FAILED! Try Removing the Object Again. If it Fails then its a bug.");
                Console.WriteLine(e);
            }
        }
        public void AddOpenOrderDetails(OrderData data)
        {
            DataDoc.XPathSelectElement(XPATH_OPENORDERS).Add(SerializeElement<OrderData>(data));
        }
        public void AddClosedOrderDetails(OrderData data)
        {
            DataDoc.XPathSelectElement(XPATH_CLOSEDORDERS).Add(SerializeElement<OrderData>(data));
        }

        public void MoveToClosed(string orderId)
        {
            DataDoc.XPathSelectElement(XPATH_CLOSEDORDERS).Add(
                DataDoc.XPathSelectElement(XPATH_OPENORDERS).XPathSelectElement(
                    string.Format("OrderData[ID='{0}']", orderId)));

            RemoveOpenOrderDetails(orderId);
        }
        public void MoveToOpen(string orderId)
        {
            DataDoc.XPathSelectElement(XPATH_OPENORDERS).Add(
                DataDoc.XPathSelectElement(XPATH_CLOSEDORDERS).XPathSelectElement(
                    string.Format("OrderData[ID='{0}']", orderId)));

            RemoveCloseOrderDetails(orderId);
        }
        public void SaveDocument()
        {
            using (StringWriter writer = new StringWriter())
                DataDoc.Root.Save(DocumentFileName);
        }

        /// <summary>
        /// Takes the Xml element and converts it into a usable object with less leg work
        /// </summary>
        /// <typeparam name="T">Xml Object Type to Convert to</typeparam>
        /// <param name="element">The Element to Convert</param>
        /// <returns>Object of Converted Xml Storage</returns>
        private T DeserializeElement<T>(XNode element)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var reader = element.CreateReader())
                return (T)serializer.Deserialize(reader);
        }

        /// <summary>
        /// Converts an Object into Xml to be stored into a file.
        /// </summary>
        /// <typeparam name="T">Object Converting From</typeparam>
        /// <param name="objectIn">Object to Convert</param>
        /// <returns>Xml Element</returns>
        private static XNode SerializeElement<T>(T objectIn)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (MemoryStream ramStream = new MemoryStream())
            {
                serializer.Serialize(ramStream, objectIn);
                ramStream.Position = 0;
                return ClearAtributes<T>(XDocument.Load(ramStream));
            }
        }

        /// <summary>
        /// Strips all Unwanted Atributes from an xml element;
        /// </summary>
        /// <typeparam name="T">Object Type that it is working with</typeparam>
        /// <param name="element">element to be stripped</param>
        /// <returns>Cleaned Element</returns>
        private static XNode ClearAtributes<T>(XNode element)
        {
            element.XPathSelectElement(typeof(T).Name).RemoveAttributes();
            return element.XPathSelectElement(typeof(T).Name);
        }

        /// <summary>
        /// Converts any stream into an XmlNode
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns> 
        private static XNode ToXmlFromStream(Stream stream)
        {
            return XDocument.Load(stream);
        }
    }
}

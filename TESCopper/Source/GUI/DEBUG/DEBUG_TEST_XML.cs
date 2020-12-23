/*///////////////////////////////////////////////
 *  Created By Brandon Cook
 *  Created On: 12/22/2020
 *  Script Name: DEBUG_TEST_XML.cs
 *  Description: Allow for the testing of the 
 *  database storage on a local Server and or
 *  computer for any errors in the XmlService
 *  
 *  ALL RIGHTS RESERVED
 *///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace TESCopper
{
    class DEBUG_TEST_XML
    {
        XmlService xmlDocumentService;
        enum XML_TEST
        {
            SETUP = 0,
            Get_Open_Orders,
            Get_Closed_Orders,
            Add_Open_Order,
            Add_Close_Order,
            Remove_Open_Order,
            Remove_Close_Order,
            Move_Order_ToClose,
            Move_Order_ToOpen
        }

        public DEBUG_TEST_XML()
        {
            Console.WriteLine("Starting XMl Service.");
            Console.WriteLine("\tChecking If Data File Exist");
            if (!File.Exists(XmlService.DocumentFileName))
            {
                Console.WriteLine("\t\tDoes Not Exist. Starting Setup.");
                Console.WriteLine("\t\t\tSetting Up...");
                SetupNewXmlFile();
            }
            xmlDocumentService = new XmlService();

            Console.WriteLine("XML DEBUG");
            Console.WriteLine("Chose a Test");

            byte counter = 0;
            bool isRunning = true;

            foreach (var option in Enum.GetValues(typeof(XML_TEST)))
            {
                Console.WriteLine("\t{1}:[{0}]", option, counter);
                counter++;
            }
            Console.WriteLine("\tQ:Quite XML DEBUG");
            Console.WriteLine("\tH:Help");

            while (isRunning)
            {
                try
                {
                    char usrInpChar = GetKeyWMsg(":XML:>").KeyChar;
                    Console.WriteLine();

                    //Use a Switching Statement Here LATER
                    switch (usrInpChar)
                    {
                        case 'q': // Quit
                            isRunning = false;
                            break;

                        case 'h': // Help
                               counter = 0;
                               foreach (var e in Enum.GetValues(typeof(XML_TEST)))
                               {
                                   Console.WriteLine("\t{1}:[{0}]", e, counter);
                                   counter++;
                               }
                               Console.WriteLine("\tQ:Quite XML DEBUG");
                               Console.WriteLine("\tH:Help");
                            break;

                        default: // Other Options
                            XML_TEST testSection = XML_TEST.SETUP;
                            bool checkSelString = Enum.TryParse<XML_TEST>(usrInpChar.ToString(), true, out testSection);

                            if (checkSelString)
                                switch (testSection)
                                {
                                    case XML_TEST.SETUP:
                                        #region SETUP

                                        Console.WriteLine("Setting Up");
                                        if (!File.Exists(XmlService.DocumentFileName))
                                            SetupNewXmlFile();
                                        else
                                        {
                                            Console.WriteLine("\nWARNING THIS WILL DELETE ALL DATA IN THE FILE!!\n");
                                            Console.WriteLine("File Exist!\n Would you like to Redo the file? [y/n]");

                                            if (GetKeyWMsg(":XML/RedoFile:>").Key == ConsoleKey.Y)
                                            {
                                                Console.WriteLine();

                                                Console.WriteLine("\tARE YOU SURE? [Y/N]");

                                                if (GetKeyWMsg(":XML/RedoFile/CONFIRM:>").Key == ConsoleKey.Y)
                                                {
                                                    Console.WriteLine();

                                                    Console.WriteLine("\t\tDELETING FILE.");
                                                    File.Delete(XmlService.DocumentFileName);
                                                    Console.WriteLine("\t\t\tDeleted.");

                                                    Console.WriteLine("\t Re Setting Up File");
                                                    Console.WriteLine("\t Checking if Folder Exist..");
                                                    if (!Directory.Exists(XmlService.DocumentFolder))
                                                    {
                                                        Console.WriteLine("\t\t Creating Data Folder");
                                                        Directory.CreateDirectory(XmlService.DocumentFolder);
                                                    }
                                                    Console.WriteLine("\t Creating XML File");
                                                    XmlService.SetupNewXmlFile(XmlService.DocumentFileName);
                                                }
                                                else Console.WriteLine("CANCELLED");
                                            }
                                            else Console.WriteLine("CANCELLED");
                                        }
                                        Console.WriteLine("Finished Setting up Document.");

                                        break;
                                    #endregion
                                    case XML_TEST.Get_Open_Orders:
                                        #region Get Open Orders

                                        Console.WriteLine("Getting Open Orders");
                                        if (File.Exists(XmlService.DocumentFileName))
                                            PrintOpenOrders();
                                        break;

                                    #endregion
                                    case XML_TEST.Get_Closed_Orders:
                                        #region Get Closed Orders

                                        Console.WriteLine("Getting CLosed Orders");
                                        if (File.Exists(XmlService.DocumentFileName))
                                            PrintClosedOrders();
                                        break;

                                    #endregion
                                    case XML_TEST.Add_Open_Order:
                                        #region ADD ORDER

                                        Console.WriteLine("Adding To Open Orders");
                                        Console.WriteLine("\t Checking If XMLService Exist");

                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Creating New Order. Please Input Data.");
                                            xmlDocumentService.AddOpenOrderDetails(PromptNewOrder());
                                            xmlDocumentService.SaveDocument();
                                        }
                                        else Console.WriteLine("Dataset Does not exist! Recommend running Setup! XML MENU OPTION 0");
                                        break;

                                    #endregion
                                    case XML_TEST.Add_Close_Order:
                                        #region ADD Closed Orders

                                        Console.WriteLine("Adding To Closed Orders");
                                        Console.WriteLine("\t Checking If XMLService Exist");
                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Creating New Closed Order. Please Input Data.");
                                            xmlDocumentService.AddClosedOrderDetails(PromptNewOrder());
                                            xmlDocumentService.SaveDocument();
                                        }
                                        else Console.WriteLine("Dataset Does not exist! Recommend running Setup! XML MENU OPTION 0");
                                        break;

                                    #endregion
                                    case XML_TEST.Remove_Open_Order:
                                        #region Remove Open Order
                                        Console.WriteLine("Removing Open Order");

                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Building List..");
                                            Dictionary<string, string> ids = PrintOpenOrders(true);

                                            string key = GetKeyWMsg("Select Project> ").ToString();
                                            Console.WriteLine();

                                            try
                                            {
                                                xmlDocumentService.RemoveOpenOrderDetails(ids[key]);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("\t\t\t {0}", e.Message);
                                            }
                                        }
                                        break;
                                    #endregion
                                    case XML_TEST.Remove_Close_Order:
                                        #region Remove Closed Order
                                        Console.WriteLine("Removing Closed Order");
                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Building List..");
                                            Dictionary<string, string> ids = PrintClosedOrders(true);

                                            string key = GetKeyWMsg("Select Project> ").ToString();
                                            Console.WriteLine();

                                            try
                                            {
                                                xmlDocumentService.RemoveCloseOrderDetails(ids[key]);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("\t\t\t {0}", e.Message);
                                            }
                                        }
                                        break;

                                    #endregion
                                    case XML_TEST.Move_Order_ToClose:
                                        #region MOVE ORDER TO CLOSED
                                        Console.WriteLine("Move a Open Order To Closed");

                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Building List..");
                                            Dictionary<string, string> ids = PrintOpenOrders(true);

                                            string key = GetKeyWMsg("Select Order To Move> ").ToString();
                                            Console.WriteLine();

                                            try
                                            {
                                                xmlDocumentService.MoveToClosed(ids[key]);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("\t\t\t {0}", e.Message);
                                            }
                                        }
                                        break;

                                    #endregion
                                    case XML_TEST.Move_Order_ToOpen:
                                        #region MOVE ORDER TO OPEN
                                        Console.WriteLine("Move a Closed Order to Open");

                                        if (File.Exists(XmlService.DocumentFileName))
                                        {
                                            Console.WriteLine("\t Building List..");
                                            Dictionary<string, string> ids = PrintClosedOrders(true);

                                            string key = GetKeyWMsg("Select Order To Move> ").ToString();
                                            Console.WriteLine();

                                            try
                                            {
                                                xmlDocumentService.MoveToOpen(ids[key]);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("\t\t\t {0}", e.Message);
                                            }
                                        }

                                        #endregion
                                        break;
                                    
                                }
                            else throw new Exception("Could Not Find Test");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Exiting XML Tests");

        }

        private void SetupNewXmlFile()
        {
            Console.WriteLine("\t Checking if Folder Exist..");
            if (!Directory.Exists(XmlService.DocumentFolder))
            {
                Console.WriteLine("\t\t Creating Data Folder");
                Directory.CreateDirectory(XmlService.DocumentFolder);
            }
            Console.WriteLine("\t Creating XML File");
            XmlService.SetupNewXmlFile(XmlService.DocumentFileName);
            Console.WriteLine("\t\t\tCompleted");
        }
        private void PrintOpenOrders()
        {

            Console.WriteLine("\t Building List..");
            List<OrderData> orderData = new List<OrderData>(xmlDocumentService.GetOpenOrderDetails());
            foreach (var bit in orderData)
                Console.WriteLine(bit.ToString());

        }
        private void PrintClosedOrders()
        {
            Console.WriteLine("\t Building List..");
            List<OrderData> orderData = new List<OrderData>(xmlDocumentService.GetClosedOrderDetails());
            foreach (var bit in orderData)
                Console.WriteLine(bit.ToString());
        }

        private Dictionary<string, string> PrintOpenOrders(bool retIds)
        {
            List<OrderData> orderData = new List<OrderData>(xmlDocumentService.GetOpenOrderDetails());
            if (retIds)
            {
                Dictionary<string, string> ids = new Dictionary<string, string>();
                var i = 0;
                foreach (var bit in orderData)
                {
                    Console.WriteLine("\t\t{0} :ID: {1}     ProjectName: {2}\n", i, bit.Id, bit.ProjName);
                    ids.Add(i.ToString(), bit.Id);
                    i++;
                }
                return ids;
            }
            else
            {
                foreach (var bit in orderData)
                    Console.WriteLine("\t\t:ID: {0}     ProjectName: {1}\n", bit.Id, bit.ProjName);
                return null;
            }
        }
        private Dictionary<string, string> PrintClosedOrders(bool retIds)
        {
            List<OrderData> orderData = new List<OrderData>(xmlDocumentService.GetClosedOrderDetails());
            if (retIds)
            {
                Dictionary<string, string> ids = new Dictionary<string, string>();
                var i = 0;
                foreach (var bit in orderData)
                {
                    Console.WriteLine("\t\t{0} :ID: {1}     ProjectName: {2}\n", i, bit.Id, bit.ProjName);
                    ids.Add(i.ToString(), bit.Id);
                    i++;
                }
                return ids;
            }
            else
            {
                foreach (var bit in orderData)
                    Console.WriteLine("\t\t ID: {0}     ProjectName: {1}\n", bit.Id, bit.ProjName);
                return null;
            }
        }

        private OrderData PromptNewOrder()
        {
            OrderData data = new OrderData();
            ContactInfo contact = new ContactInfo();

            data.Id = GetInputMsg("NEW ORDER [ID]>");
            data.ProjName = GetInputMsg("NEW ORDER [Project Name]>");
            data.PONumber = GetInputMsg("NEW ORDER [PO Number]>");
            data.Instructions = GetInputMsg("NEW ORDER [Instructions]>");
            data.ShouldRushOrder = GetKeyMsg("NEW ORDER [Rush?][y/n]>", ConsoleKey.Y);
            data.KeepOnFile = GetKeyMsg("NEW ORDER [KeepOnFile?][y/n]>", ConsoleKey.Y);
            contact.Id = GetInputMsg("NEW ORDER [Contact ID]>");
            contact.CompanyName = GetInputMsg("NEW ORDER [Contact [Company Name]]>");
            contact.FirstName = GetInputMsg("NEW ORDER [Contact [First Name]]>");
            contact.LastName = GetInputMsg("NEW ORDER [Contact [Last Name]]>");
            contact.StreetAddress = GetInputMsg("NEW ORDER [Contact [Street Address]]>");
            contact.City = GetInputMsg("NEW ORDER [Contact [City]]>");
            contact.State = GetInputMsg("NEW ORDER [Contact [State]]>");
            contact.ZipCode = GetInputMsg("NEW ORDER [Contact [ZIPCode]]>");
            contact.PhoneNumber = GetInputMsg("NEW ORDER [Contact [Phone Number]]>");
            contact.Email = GetInputMsg("NEW ORDER [Contact [Email]]>");
            Console.WriteLine();

            data.Contacts = new ContactInfo[] { contact };

            return data;
        }

        private string GetInputMsg(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }
        private ConsoleKeyInfo GetKeyWMsg(string msg)
        {
            Console.Write(msg);
            return Console.ReadKey();
        }
        private bool GetKeyMsg(string msg,ConsoleKey correctKey)
        {
            Console.Write(msg);
            return (Console.ReadKey().Key == correctKey? true : false);
        }
    }
}

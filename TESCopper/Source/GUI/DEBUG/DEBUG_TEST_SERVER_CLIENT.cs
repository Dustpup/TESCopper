using System;
using System.Collections.Generic;
using System.Text;

namespace TESCopper
{
    class DEBUG_TEST_SERVER_CLIENT
    {
        enum TESTS
        {
            SERVER,
            CLIENT
        }
        public DEBUG_TEST_SERVER_CLIENT()
        {
            Console.WriteLine("Starting Server and Client Testing...");

            byte counter = 0;
            bool isRunning = true;

            foreach (var option in Enum.GetValues(typeof(TESTS)))
            {
                Console.WriteLine("\t{1}:[{0}]", option, counter);
                counter++;
            }
            Console.WriteLine("\tQ:Quite SEVER CLIENT DEBUG");
            Console.WriteLine("\tH:Help");

            while (isRunning)
            {
                try
                {
                    char usrInpChar = GetKeyWMsg(": SER CLI TES:>").KeyChar;
                    Console.WriteLine();

                    switch (usrInpChar)
                    {
                        case 'q':
                            isRunning = false;
                            break;
                        case 'h':
                            foreach (var option in Enum.GetValues(typeof(TESTS)))
                            {
                                Console.WriteLine("\t{1}:[{0}]", option, counter);
                                counter++;
                            }
                            Console.WriteLine("\tQ:Quite XML DEBUG");
                            Console.WriteLine("\tH:Help");
                            break;
                        default:
                            TESTS testSelection = TESTS.CLIENT;
                            bool checkSelString = Enum.TryParse<TESTS>(usrInpChar.ToString(), true, out testSelection);
                            if (checkSelString)
                                switch (testSelection)
                                {
                                    case TESTS.CLIENT:
                                        Console.WriteLine("\t\t Starting Communications Client.. \n");
                                        SocketClientService client = new SocketClientService();
                                        client.StartClient();
                                        break;
                                    case TESTS.SERVER:
                                        SocketCommandService service = new SocketCommandService();
                                        service.StartServer();
                                        break;
                                }
                            else throw new Exception("Could Not Find Test");
                            
                            break;


                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
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
        private bool GetKeyMsg(string msg, ConsoleKey correctKey)
        {
            Console.Write(msg);
            return (Console.ReadKey().Key == correctKey ? true : false);
        }
    }
}

using System;

namespace TESCopper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isRunning = true;
            Console.WriteLine("Welcome To TESCopper");
            Console.WriteLine("What Would you like to do?");
            Console.WriteLine("1: XML");
            Console.WriteLine("s: Server Client");
            Console.WriteLine("h: ssh Test");
            Console.WriteLine("q: Quit");

            while(isRunning)
            {
                try
                {
                    Console.Write("::>");
                    var selection = Console.ReadKey();
                    Console.WriteLine();

                    switch (selection.Key)
                    {
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            Console.WriteLine("Starting XML TEST");
                            new DEBUG_TEST_XML();
                            break;

                        case ConsoleKey.Q:
                            isRunning = false;
                            Console.WriteLine("Quitting");
                            break;
                        case ConsoleKey.S:
                            Console.WriteLine("Starting Server Client Test");
                            new DEBUG_TEST_SERVER_CLIENT();
                            break;
                        case ConsoleKey.H:
                            Console.WriteLine("Starting SSH Test");
                            new DEBUG_TEST_SSH();
                            break;
                        default:
                            throw new Exception("Invalid Key");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            
        }
    }
}

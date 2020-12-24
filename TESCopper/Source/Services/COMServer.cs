using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TESCopper
{
    class COMServer
    {
        public COMServer()
        {
            IPAddress ipAddr = new IPAddress(new byte[]{ 127,0,0,1});
            IPEndPoint localEndpoint = new IPEndPoint(ipAddr, 8888);

            Socket listener = new Socket(
                ipAddr.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndpoint);

                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waitingg for Connection");

                    Socket clientSocket = listener.Accept();

                    byte[] bytes = new byte[1024];
                    string data = null;

                    while(true)
                    {
                        int numByte = clientSocket.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes, 0, numByte);

                        if (!String.IsNullOrEmpty(data))
                            break;
                    }

                    Console.WriteLine("Text received -> {0}", data);
                    byte[] message = Encoding.ASCII.GetBytes("Test Server");

                    clientSocket.Send(message);

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}

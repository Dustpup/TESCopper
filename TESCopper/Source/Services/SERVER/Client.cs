using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper.Source.Services.SERVER
{
    class Client
    {
        public const int MAX_BUFFER = 1024;
        public const int MAX_BACKLOG_OF_PACKETS = 100;
        public const string END_OF_MSG = ":..:";
        private static bool isListening = false;
        private static EndPoint localEndPoint;

        public static Task StartClient(IPAddress ipAddress, int port)
        {
            return Task.Run(
                () =>
                {
                    byte[] buffer = new byte[MAX_BUFFER];
                    localEndPoint = new IPEndPoint(ipAddress, port);
                    Socket client = new Socket(
                        ipAddress.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                    InitEvents();

                    client.BeginConnect(
                        localEndPoint,
                        new AsyncCallback(ConnectionCallback));
                }
                );
        }
        public static void ConnectionCallback(IAsyncResult)
        private static void InitEvents()
        {
            
        }
    }
}

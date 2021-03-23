using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper.Source.Services.SERVER
{
    delegate void ConnectionHandler(Socket connector);

    class Client
    {
        public const int MAX_BUFFER = 1024;
        public const int MAX_BACKLOG_OF_PACKETS = 100;
        public const string END_OF_MSG = ":..:";

        private static bool isListening = false;
        private static EndPoint localEndPoint;

        private static ManualResetEvent connectionComplete =
            new ManualResetEvent(false);

        public static event ConnectionHandler OnConnection;
        public static event ConnectionHandler OnConnected;

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

                    // instert Start Connection here.
                    OnConnection.Invoke(client);
                }
                );
        }
        private static void Receive(Socket Client)
        {
            try
            {
                State state = new State();
                state.WorkerSocket = Client;

                Client.BeginReceive(state.Buffer, 0, MAX_BUFFER, 0,
                    new AsyncCallback(ReceiveCallBack), state);
            }
            catch
            {
                // Add Error Code
            }
        }
        private static void ConnectionCallback(IAsyncResult result)
        {
            try
            {
                Socket client = (Socket)result.AsyncState;

                client.EndConnect(result);

                connectionComplete.Set();
                OnConnected.Invoke(client);
            }
            catch
            {
                // Add Error Code
            }
        }
        private static void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                State state = (State)result.AsyncState;
                Socket client = state.WorkerSocket;

                int bytesRead = client.EndReceive(result);
                if (bytesRead > 0)
                {
                    state.recieverString += Encoding.ASCII.GetString(
                        state.Buffer, 0, StateObject.BufferSize);

                    client.BeginReceive(state.Buffer, 0, MAX_BUFFER, 0,
                        new AsyncCallback(ReceiveCallBack), state);
                }
                else
                {
                    
                }
            }
            catch
            {
                
            }
        }

        private static void Client_OnConnection(Socket connector)
        {
            try
            {
                connector.BeginConnect(localEndPoint, new AsyncCallback(ConnectionCallback), connector);
                connectionComplete.WaitOne();

                // Connection Success
            }
            catch
            {
                // Add Error Code
            }
            finally
            {
                connector.Shutdown(SocketShutdown.Both);
                connector.Close();
            }
        }
        private static void Client_OnConnected(Socket connector)
        {
            
        }

        private static void InitEvents()
        {
            OnConnection += Client_OnConnection;
            OnConnected += Client_OnConnected;
        }

        private struct State
        {
            public Socket WorkerSocket { get; set; }
            public byte[] Buffer { get; set; }
            public string recieverString { get; set; }

            public void Init()
            {
                Buffer = new byte[MAX_BUFFER];
                recieverString = "";
                WorkerSocket = null;
            }
        }
    }
}

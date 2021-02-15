using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper
{
    delegate void ListenHandler(object sender,Socket listener);
    delegate void CallBackHandler(IAsyncResult result);

    class Server
    {
        public const int MAX_BUFFER = 1024;
        public const int MAX_BACKLOG_OF_PACKETS = 100;

        private bool isListening = false;
        private EndPoint localEndPoint;
        private event ListenHandler OnStartListening;
        private event CallBackHandler OnAcceptClientCallBack;
        private event CallBackHandler OnReadCallBack;
        private event CallBackHandler OnSendCallBack;

        private ManualResetEvent waitForClient;

        /// <summary>
        /// Starts the server to begin accepting client request.
        /// </summary>
        /// <param name="address">the address that the server will be using to communicate</param>
        /// <param name="port">the port that the server will be using to accept request</param>
        /// <returns></returns>
        public Task StartServer(IPAddress address, int port)
        {
            return Task.Run(
                () =>
                {
                    byte[] buffer = new byte[MAX_BUFFER];
                    localEndPoint = new IPEndPoint(address, port);
                    Socket listener = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                        );
                    InitEvents();

                    OnStartListening.Invoke(this, listener);
                }
                );
        }

        private void StartListening(object sender,Socket listener)
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(MAX_BACKLOG_OF_PACKETS);
                isListening = true;

                while(isListening)
                {
                    waitForClient.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptClientCallBack), listener);
                    waitForClient.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void AcceptClientCallBack(IAsyncResult callbackResult)
        {
            waitForClient.Set();
            OnAcceptClientCallBack.Invoke(callbackResult);

            Socket listener    = (Socket)callbackResult.AsyncState;
            Socket handler     = listener.EndAccept(callbackResult);
                               
            ClientState state  = new ClientState();
            state.WorkerSocket = handler;

            handler.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallBack), state);
        }
        private void ReadCallBack(IAsyncResult callbackResult)
        {
            string incommingMsg = "";


        }
        private void SendCallBack(IAsyncResult callbackResult)
        {

        }
        

        private void InitEvents()
        {
            OnStartListening       += StartListening;
            waitForClient = new ManualResetEvent(true);

        }
    }
}
 
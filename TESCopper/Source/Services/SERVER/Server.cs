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
    delegate void RequestHandler(object sender,string message,Socket handler);

    class Server
    {
        public const int MAX_BUFFER = 1024;
        public const int MAX_BACKLOG_OF_PACKETS = 100;
        public const string END_OF_MSG = ":..:";
        private bool isListening = false;
        private EndPoint localEndPoint;

        public event ListenHandler OnStopListening;
        public event ListenHandler OnServerStopped;
        public event ListenHandler OnStartListening;
        public event RequestHandler OnClientRequest;

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
        public void CloseServer()
        { isListening = false; }

        private void StopServer(Socket listener)
        {
            listener.Disconnect(true);
            listener.Dispose();
            waitForClient.Close();

            OnServerStopped.Invoke(this, listener);
            Console.WriteLine("Server Stopped..");
            
        }
        public static void StopListeningToClient(object sender,Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
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
            finally
            {
                StopServer(listener);
            }
        }
        private void Send(Socket handle,string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handle.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallBack), handle);

        }
        private void InitEvents()
        {
            OnStartListening += StartListening;
            OnClientRequest += Server_OnClientRequest;
            waitForClient = new ManualResetEvent(true);
        }

        private void AcceptClientCallBack(IAsyncResult callbackResult)
        {
            waitForClient.Set();

            Socket listener    = (Socket)callbackResult.AsyncState;
            Socket handler     = listener.EndAccept(callbackResult);
                               
            State state  = new State();
            state.Init();
            state.WorkerSocket = handler;

            handler.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallBack), state);
        }
        private void ReadCallBack(IAsyncResult callbackResult)
        {
            string incommingMsg = "";
            State state    = (State)callbackResult.AsyncState;
            Socket handler = state.WorkerSocket;
            int bytesRead  = handler.EndReceive(callbackResult);

            if (bytesRead > 0)
            {
                state.recieverString += Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                incommingMsg = state.recieverString;
                incommingMsg = incommingMsg.Trim('\r').Trim('\n'); //Clean Up Bad Chars

                if (incommingMsg.IndexOf(END_OF_MSG) > -1)
                {
                    
                    OnClientRequest.Invoke(this, incommingMsg, handler);

                    state.recieverString = "";
                    incommingMsg = "";

                    handler.BeginReceive(state.Buffer, 0, MAX_BUFFER, 0,
                    new AsyncCallback(ReadCallBack), state);
                }
                else handler.BeginReceive(state.Buffer, 0, MAX_BUFFER, 0,
                    new AsyncCallback(ReadCallBack), state);

                // Remeber to shutdown socket. -------------------------------------------------------------------------------------
            }

        }
        private void SendCallBack(IAsyncResult callbackResult)
        {
            try
            {
                Socket handler = (Socket)callbackResult.AsyncState;
                int bytesToSend = handler.EndSend(callbackResult);
            }
            catch
            {
                Console.WriteLine("Error Could Not Send Message.");
            }
        }

        private void Server_OnClientRequest(object sender, string message, Socket handler)
        {
            string msg = message.Replace(END_OF_MSG, "");
            if (msg == "stop")
            {
                Send(handler, "Hello To You Too!");
            }
        }

        public struct State
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
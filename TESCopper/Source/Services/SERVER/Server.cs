using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper
{
    delegate void ListenHandler(Socket listener);
    delegate void RequestHandler(string message,Socket handler);
    delegate void SocketExceptionHandler(Socket handler,SocketException error);

    class Server
    {
        public const int MAX_BUFFER = 1024;
        public const int MAX_BACKLOG_OF_PACKETS = 100;
        public const string END_OF_MSG = ":..:";
        private static bool isListening = false;
        private static EndPoint localEndPoint;

        // To initialize events in this code Use InitEvents()
        public static event ListenHandler OnServerStopped;
        public static event ListenHandler OnStartListening;
        public static event RequestHandler OnClientRequest;
        public static event SocketExceptionHandler OnClientError;
        
        private static ManualResetEvent waitForClient;

        /// <summary>
        /// Starts the server to begin accepting client request.
        /// </summary>
        /// <param name="address">the address that the server will be using to communicate</param>
        /// <param name="port">the port that the server will be using to accept request</param>
        /// <returns></returns>
        public static Task StartServer(IPAddress address, int port)
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

                    OnStartListening.Invoke(listener);
                }
                );
        }
        /// <summary>
        /// Forces the server to stop the loop
        /// </summary>
        public static void CloseServer()
        { isListening = false; }

        /// <summary>
        /// Stops the listner to the server
        /// </summary>
        /// <param name="listener">the listener to stop</param>
        private static void StopServer(Socket listener)
        {
            listener.Disconnect(true);
            listener.Dispose();
            waitForClient.Close();

            OnServerStopped.Invoke(listener);
            Console.WriteLine("Server Stopped..");
            
        }

        /// <summary>
        /// Stops listening to a paticular client.
        /// </summary>
        /// <param name="sender">object sending request</param>
        /// <param name="handler">the clients socket</param>
        public static void StopListeningToClient(Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        /// <summary>
        /// starts listening on the local host for clients
        /// </summary>
        /// <param name="sender">Object sending request</param>
        /// <param name="listener">Listener to start</param>
        private static void StartListening(Socket listener)
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

        /// <summary>
        /// Sends a message back to the client
        /// </summary>
        /// <param name="handle">clients socket</param>
        /// <param name="data">message to send to client</param>
        private static void Send(Socket handle,string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handle.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallBack), handle);

        }
        
        private static void AcceptClientCallBack(IAsyncResult callbackResult)
        {
            waitForClient.Set();

            Socket listener    = (Socket)callbackResult.AsyncState;
            Socket handler     = listener.EndAccept(callbackResult);
                               
            State state  = new State();
            state.Init();
            state.WorkerSocket = handler;

            ReadClientInput(handler, state);
        }
        private static void ReadCallBack(IAsyncResult callbackResult)
        {
            string incommingMsg = "";
            State state    = (State)callbackResult.AsyncState;
            Socket handler = state.WorkerSocket;

            try
            {
                int bytesRead = handler.EndReceive(callbackResult);

                if (bytesRead > 0)
                {
                    state.recieverString += Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                    incommingMsg = CleanUpClientInput(state.recieverString);

                    if (ContainsEndOfLineMessage(incommingMsg))
                    {
                        OnClientRequest.Invoke(incommingMsg, handler);

                        state.recieverString = "";
                        incommingMsg = "";

                        ReadClientInput(handler, state);
                    }
                    else ReadClientInput(handler, state);

                    // Remeber to shutdown socket. -------------------------------------------------------------------------------------
                }
            }
            catch (SocketException ex)
            {
                OnClientError.Invoke(handler, ex);
            }

        }
        private static void SendCallBack(IAsyncResult callbackResult)
        {
            Socket handler = (Socket)callbackResult.AsyncState;
            try
            {
                int bytesToSend = handler.EndSend(callbackResult);
            }
            catch (SocketException ex)
            {
                OnClientError.Invoke(handler, ex);
            }
        }

        private static void Server_OnClientRequest(string message, Socket handler)
        {
#if DEBUG
            string msg = message.Replace(END_OF_MSG, "");
            if (msg == "stop")
            {
                Console.WriteLine("Sent Info :)");
                Send(handler, "Hello To You Too!");
            }
#endif
        }
        private static void Server_OnClientError(Socket handler, SocketException error)
        {
            Socket client = handler;
            IPEndPoint
                remoteEndP = client.RemoteEndPoint as IPEndPoint,
                localEndP = client.LocalEndPoint as IPEndPoint;

            switch (error.SocketErrorCode)
            {
                case SocketError.ConnectionReset:
                    Console.WriteLine("Client Closed Connection Prematurely");
                    break;
                case SocketError.TimedOut:
                    Console.WriteLine("Client Haded Timed Out");
                    break;
                default:
                    Console.WriteLine("{1}:{2} Socket Error has Occured. Error Code {0}", error.ErrorCode, remoteEndP.Address.ToString(), localEndP.Port);
                    break;
            }
        }

        private static void InitEvents()
        {
            OnStartListening += StartListening;
            OnClientError += Server_OnClientError;
            OnClientRequest += Server_OnClientRequest;
            waitForClient = new ManualResetEvent(true);
        }
        private static void ReadClientInput(Socket handler,State state)
        {
            handler.BeginReceive(state.Buffer, 0, MAX_BUFFER, 0,
                        new AsyncCallback(ReadCallBack), state);
        }
        private static string CleanUpClientInput(string clientInput)
        { return clientInput.Trim('\r').Trim('\n'); }
        private static bool ContainsEndOfLineMessage(string input)
        {
            return input.IndexOf(END_OF_MSG) > -1;
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
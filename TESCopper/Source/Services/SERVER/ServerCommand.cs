using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper
{

    class ServerCommand
    {
        const int MAX_BACKLOG_OF_PACKETS = 100;

        IPEndPoint socketEndPoint;
        bool isListening = false;
        ManualResetEvent waitForAClient = new ManualResetEvent(false);

        public event EventHandler OnNewEndPoint;
        public event EventHandler OnStartedListening;
        public event EventHandler OnStoppedListening;
        public event EventHandler OnClientConnected;

        public IPEndPoint GetLocalEndPoint 
        {

            get => socketEndPoint;
            private set
            {
                socketEndPoint = value;
                try
                {
                    //OnNewEndPoint.Invoke(this, null);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("No Event Found For socket End Point");
                }
            }
        }

       public Task StartServer(IPAddress address,int port)
        {
            return Task.Run(
                () =>
                {
                    byte[] buffer = new byte[ClientState.MAX_BUFFER_SIZE];
                    Socket listener = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                    GetLocalEndPoint = new IPEndPoint(address, port);

                    StartListening(listener);
                }
                );
        }

        private void StartListening(Socket listener)
        {
            OnStartedListening += ServerCommand_OnStartedListening;
            OnStartedListening.Invoke(this, null);

            try
            {
                listener.Bind(GetLocalEndPoint);
                listener.Listen(MAX_BACKLOG_OF_PACKETS);
                while(isListening)
                {
                    waitForAClient.Reset();
                    listener.BeginAccept(
                        new AsyncCallback(AcceptClientCallBack),
                        listener);
                    waitForAClient.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                StopListening(listener); 
            }
        }

        private void ServerCommand_OnStartedListening(object sender, EventArgs e)
        {
            isListening = true;
        }

        private void StopListening(Socket listener)
        {
            listener.Disconnect(true);
            waitForAClient.Close();
            Console.WriteLine("Session Closed");
            //OnStoppedListening.Invoke(this, null);
        }

        private void AcceptClientCallBack(IAsyncResult callBackResult)
        {
            waitForAClient.Set();

            Socket listener = (Socket)callBackResult.AsyncState;
            Socket handler = listener.EndAccept(callBackResult);

            ClientState state = new ClientState();
            state.WorkerSocket = handler;
            handler.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallBack), state);

        }

        private void ReadCallBack(IAsyncResult callBackResult)
        {
            string content = String.Empty;

            ClientState state = (ClientState)callBackResult.AsyncState;
            Socket handler = state.WorkerSocket;

            int bytesRead = handler.EndReceive(callBackResult);

            if(bytesRead > 0)
            {
                state.recieverString.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                content = state.recieverString.ToString();

                if(content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data {1}",
                        content.Length, content);

                    state.recieverString.Clear();

                    if (state.WorkerSocket.Connected)
                    {
                        handler.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                            new AsyncCallback(ReadCallBack), state);
                    }

                    
                    // Attach Return Commands Here. 
                }
                else
                {
                    handler.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                        new AsyncCallback(ReadCallBack),state);
                }
            }
        }

        private void Send(Socket handle, string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
                handle.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handle);
            
        }

        private void SendCallback(IAsyncResult callBackResult)
        {
            try
            {
                Socket handler = (Socket)callBackResult.AsyncState;

                int bytesToSend = handler.EndSend(callBackResult);
                Console.WriteLine("Sent {0} bytes to client.", bytesToSend);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

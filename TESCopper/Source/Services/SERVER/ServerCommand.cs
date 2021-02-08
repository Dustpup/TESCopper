using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TESCopper.Source.Services.SERVER
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
                    OnNewEndPoint.Invoke(this, null);
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
        private void StopListening(Socket listener)
        {
            listener.Disconnect(true);
            waitForAClient.Close();
            Console.WriteLine("Session Closed");
            OnStoppedListening.Invoke(this, null);
        }
        private void AcceptClientCallBack(IAsyncResult callBackResult)
        {
            waitForAClient.Set();


        }
    }
}

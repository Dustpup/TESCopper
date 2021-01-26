using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
namespace TESCopper
{
    public class SocketCommandService
    {
        const int MAX_BUFFERSIZE = 1024;
        const int MAX_BACKLOG = 100;
        const int MAIN_PORT = 10101;

        private IPEndPoint localIpEndPoint;
        private bool isListening = false;
        private ManualResetEvent waitForNewClient = new ManualResetEvent(false);

        public IPHostEntry GetLocalHost    { get => Dns.GetHostEntry("localhost"); }
        public IPAddress GetIpLocalAddress { get => GetLocalHost.AddressList[0]; }
        public IPEndPoint GetLocalEndPoint { get => localIpEndPoint; private set => localIpEndPoint = value; }


        public Task StartServer()
        {
            //Add a new Thread
            return Task.Run(
                () =>
                {
                    byte[] buffer = new byte[MAX_BUFFERSIZE];
                    Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    GetLocalEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127,0,0,1}), MAIN_PORT);

                    StartListening(listener);
                }
                );
        }

        private void StartListening(Socket listener)
        {
            isListening = true;

            try
            {
                listener.Bind(localIpEndPoint);
                listener.Listen(MAX_BACKLOG);

                while (isListening)
                {
                    waitForNewClient.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
                    waitForNewClient.WaitOne();
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            finally { StopListening(listener); }
        }
        private void StartReceiving(ClientState clientState, Socket clientHandler)
        {
            clientHandler.BeginReceive(clientState.Buffer, 0, MAX_BUFFERSIZE, 0,
                    new AsyncCallback(ReaderCallBack), clientState);
        }
        private void StopListening(Socket listener)
        {
            listener.Disconnect(true);
            waitForNewClient.Close();
            Console.WriteLine("Session Closed");
        }

        private void Send(Socket clientHandler, string message)
        {
            byte[] data = GetByteArray(message);
            clientHandler.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallBack), clientHandler);
        }

        private void AcceptCallBack(IAsyncResult callBResult)
        {
            waitForNewClient.Set();

            Socket clientListener = (Socket)callBResult.AsyncState;
            Socket clientHandler = clientListener.EndAccept(callBResult);
            ClientState clientState = new ClientState();

            clientState.WorkerSocket = clientHandler;

            StartReceiving(clientState, clientHandler);
        }
        private void ReaderCallBack(IAsyncResult callBResult)
        {
            string incomMsg = string.Empty;
            ClientState clientState = (ClientState)callBResult.AsyncState;
            Socket clientHandler = clientState.WorkerSocket;

            int byteRead = clientHandler.EndReceive(callBResult);

            if(byteRead > 0)
            {
                clientState.recieverString.Append(
                    Encoding.ASCII.GetString(clientState.Buffer, 0, byteRead));

                incomMsg = clientState.recieverString.ToString();
                if (incomMsg.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        incomMsg);


                }
                else StartReceiving(clientState, clientHandler);
            }

        }
        private void SendCallBack(IAsyncResult callBResult)
        {
            try
            {
                Socket clientHandler = (Socket)callBResult.AsyncState;

                int bytesSend = clientHandler.EndSend(callBResult);
                Console.WriteLine("Sent {0} bytes to the client.", bytesSend);

                clientHandler.Shutdown(SocketShutdown.Both);
                clientHandler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
           
        private byte[] GetByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }

    
}

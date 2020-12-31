using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TESCopper
{

    class COMServer
    {
        private bool isListening = false;
        private ManualResetEvent mREvent = new ManualResetEvent(false);

        public COMServer()
        {
            IPAddress ipAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });
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
                    Console.WriteLine("Waiting for Connection");

                    Socket clientSocket = listener.Accept();

                    byte[] bytes = new byte[1024];
                    string data = null;

                    while (true)
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

        public Task StartListening_Ipv4(int port, byte[] ipAddress)
        {
            return Task.Run(() =>
            {

                byte[] dataBuffer = new byte[1024]; 
                IPHostEntry ipHostInfo = Dns.EndGetHostEntry(null);
                IPAddress ipAddress = ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                isListening = true;


                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    while (isListening)
                    {
                        mREvent.Reset();

                        listener.BeginAccept(new AsyncCallback(AcceptCallBack),listener);

                        mREvent.WaitOne();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    listener.Disconnect(true);
                    mREvent.Close();
                    Console.WriteLine("Connection Broken Due To Error");
                }

            });
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            mREvent.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.WorkerSocket = handler;

            handler.BeginReceive(state.buffer,0, StateObject.bufferSize,0,
                new AsyncCallback(ReadCallback), state);


        }
        private void ReadCallback(IAsyncResult ar)
        { 
        
        }

    }

    public partial class StateObject
    {
        public Socket WorkerSocket { get; set; }
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder recieverString { get; set; }
    }
}

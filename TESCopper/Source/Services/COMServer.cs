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

                        listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);

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

            handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkerSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.recieverString.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.recieverString.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.  
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }

        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
}
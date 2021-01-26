using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TESCopper
{
    class SocketClientService
    {
        private const int port = 10101;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent recieveDone = new ManualResetEvent(false);

        private string responce = string.Empty;

        public void StartClient()
        {
            try 
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("");
                IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
                IPEndPoint remoteEP = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);


                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                client.BeginConnect(remoteEP, 
                    new AsyncCallback(ConnectCallback),client);
                connectDone.WaitOne();

                Send(client,"This Is a test<EOF>");
                sendDone.WaitOne();


                Receive(client);
                recieveDone.WaitOne();

                Console.WriteLine("Response recieved : {0}", responce);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = (Socket)asyncResult.AsyncState;

                client.EndConnect(asyncResult);
                Console.WriteLine("Socket connect to {0}", client.RemoteEndPoint.ToString());
                connectDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                ClientState state = new ClientState();
                state.WorkerSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                ClientState state = (ClientState)asyncResult.AsyncState;
                Socket client = state.WorkerSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.recieverString.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.Buffer, 0, ClientState.MAX_BUFFER_SIZE, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.recieverString.Length > 1)
                    {
                        responce = state.recieverString.ToString();
                    }
                    // Signal that all bytes have been received.  
                    recieveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
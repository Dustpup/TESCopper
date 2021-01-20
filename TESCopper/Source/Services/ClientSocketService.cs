using System;
using System.Net.Sockets;
using System.Threading;

namespace TESCopper
{
    class SocketClientService
    {
        private const int port = 11000;

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone= new ManualResetEvent(false);
        private static ManualResetEvent recieveDone = new ManualResetEvent(false);

        private static string responce = string.Empty;

        private static void StartClient()
        {
            try
            {

            }
            catch(Exception e)
            {

            }
        }
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {

            }
            catch (Exception e)
            {

            }
        }
        private void Receive(Socket client)
        {

        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {

        }

        private void Send(Socket client, string data)
        {

        }
    }
}
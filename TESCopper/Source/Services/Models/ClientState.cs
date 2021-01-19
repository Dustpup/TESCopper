using System.Net.Sockets;
using System;
using System.Text;

namespace TESCopper
{
    partial class ClientState
    {
        public const int MAX_BUFFER_SIZE = 1024;

        public Socket WorkerSocket { get; set; }
        public byte[] Buffer { get; set; }
        public StringBuilder recieverString { get; set; }
    }
}
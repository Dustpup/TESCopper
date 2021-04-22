using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet;
using SshNet;

namespace TESCopper
{
    class SshService
    {
        const string ADD_ADDRESS = "set address ";
        const string TAG = "TAG";

        ConnectionInfo connectionInfo;
        public void Init(byte[] password, string username,string IPAddress)
        {
                connectionInfo = new ConnectionInfo(
                IPAddress,
                username,
                new PasswordAuthenticationMethod(username,password));
        }

        public void AddAddress(string address,params string[] Tags)
        {
            using (SshClient client = new SshClient(connectionInfo))
            {
                client.Connect();
                Run_AddAddress(client,address, Tags);
                client.Disconnect();
            }
        }
        public void AddMultiAddress(string[] addresses,params string[] Tags)
        {
            using (SshClient client = new SshClient(connectionInfo))
            {
               // client.Connect();
               // client.KeepAliveInterval = TimeSpan.FromSeconds(120);

                foreach(string s in addresses)
                    Run_AddAddress(client,s , Tags);

                //client.Disconnect();
            }
        }
        
        private void Run_AddAddress(SshClient client,string address, string[] Tags)
        {
            //client.RunCommand(
            //        ADD_ADDRESS + address
            //        + TAG + "[" +
            //        ConvertToStrings(Tags)
            //        + "]");

            Console.WriteLine(ADD_ADDRESS + address + " "
                    + TAG + " [" +
                    ConvertToStrings(Tags)
                    + " ] ");
        }
        private string ConvertToStrings(string[] tags)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < tags.Length; i++)
                builder.Append(" "+tags[i]);

            return builder.ToString();
        }

    }
}

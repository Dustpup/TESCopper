﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TESCopper
{
    class DEBUG_TEST_SSH
    {
        SshService service = new SshService();

        public DEBUG_TEST_SSH()
        {
            Console.WriteLine("Testing SSH...");
            service.Init(ASCIIEncoding.ASCII.GetBytes("..."),
                "...","000.000.000.000");
            service.AddMultiAddress(
                CVSService.GetIps("C:\\Users\\Desktop3245\\Downloads\\google.csv").ToArray(),
                new string[] { "BANNED", "HACKER" });
            
            
        }
    }
} 

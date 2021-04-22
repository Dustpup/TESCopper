using System;
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
            service.Init(ASCIIEncoding.ASCII.GetBytes("TEST"),
                "TEST","123.456.123.132");

            service.AddMultiAddress(new string[] {
                "123.123.123.123",
                "147.147.147.147",
                "159.159.159.159" },
                new string[] { "BANNED", "HACKER"});
        }
    }
}

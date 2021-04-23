using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TESCopper
{
    class CVSService
    {
        public static List<string> GetIps(string fileName)
        {
            List<string> IPS = new List<string>();
            StreamReader reader;
            if (File.Exists(fileName))
                using (reader = new StreamReader(fileName))
                {
                    while(!reader.EndOfStream)
                    IPS.Add(reader.ReadLine().Split(',')[0]);
                }

            return IPS;
        }
    }
}

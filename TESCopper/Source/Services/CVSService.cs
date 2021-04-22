using System;
using System.IO
using System.Collections.Generic;
using System.Text;

namespace TESCopper.Source.Services
{
    class CVSService
    {
        public CVSService(string fileName)
        {
            if(File.Exists(fileName))
            {
                FileStream file =  File.OpenRead(fileName);
                StreamReader reader = new StreamReader(file);

                reader.ReadToEnd();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Doomnet.TestFiles;

namespace Doomnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Wad wad = new Wad();

            wad.Read(new FileStream(args[0], FileMode.Open));
        }
    }
}

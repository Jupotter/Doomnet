using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Doomnet.TestFiles;
using SDL2;

namespace Doomnet
{
    class Program
    {
        static void Main(string[] args)
        {

            SDL2.SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            Wad wad = new Wad();

            wad.Read(new FileStream(args[0], FileMode.Open));

        }
    }
}

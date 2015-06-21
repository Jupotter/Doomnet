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
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            IntPtr window = SDL.SDL_CreateWindow("DoomNet",
                SDL.SDL_WINDOWPOS_UNDEFINED,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                800, 600,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            var screenSurface = SDL.SDL_GetWindowSurface(window);

            Wad wad = new Wad(new FileStream(args[0], FileMode.Open));

            wad.Read();

            var troo = wad.ReadSprite("TROOA1");

            SDL.SDL_BlitSurface(troo.Surface, IntPtr.Zero, screenSurface, IntPtr.Zero);

            SDL.SDL_UpdateWindowSurface(window);

            SDL.SDL_Delay(5000);

            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}

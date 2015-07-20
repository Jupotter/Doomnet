using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;
using System;
using System.IO;

namespace Doomnet
{
    internal class Program
    {
        private const int WIDTH = 1600;
        private const int HEIGHT = 900;

        static void Main(string[] args)
        {
            var p = new Program();
            p.MyMain(args);
        }

        private void MyMain(string[] args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            IntPtr window = SDL.SDL_CreateWindow("DoomNet",
                SDL.SDL_WINDOWPOS_UNDEFINED,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                WIDTH, HEIGHT,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            var screenSurface = SDL.SDL_GetWindowSurface(window);

            Wad wad = new Wad(new FileStream(args[0], FileMode.Open));

            wad.Read();

            var levelDrawer = new LevelDrawer();
            var level = wad.LoadLevel("E1M1");

            levelDrawer.SaveImage(level);

            var width = level.Width;
            var height = level.Height;
            var bitmap = new Bitmap(width, height);

            var start = level.Things.First(t => t.type == 1);

            SDL.SDL_Rect smallRect;
            IntPtr smalRectPtr;
            unsafe
            {
                smalRectPtr = Marshal.AllocHGlobal(sizeof (SDL.SDL_Rect));
            }
            int angle = start.angle;

            bool end = false;
            while (end == false)
            {
                SDL.SDL_Event @event;
                if (SDL.SDL_PollEvent(out @event) != 0)
                {
                    switch (@event.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            end = true;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_LEFT)
                            {
                                angle--;
                            }
                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_RIGHT)
                            {
                                angle++;
                            }
                            
                            if (angle < 0)
                                angle = 360 + angle;
                            angle = angle%360;

                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_UP)
                            {
                                start.posX += (short)(Math.Cos(angle * Math.PI / 180) * 10);
                                start.posY += (short)(Math.Sin(angle * Math.PI / 180) * 10);
                            }
                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_DOWN)
                            {
                                start.posX -= (short)(Math.Cos(angle * Math.PI/180) * 10);  
                                start.posY -= (short)(Math.Sin(angle * Math.PI/180) * 10);
                            }
                            break;
                    }
                }
                //angle++;
                var surface = ViewRenderer.DrawVision(start, angle, level);

                //bitmap.Save(level.Definition.Name.Trim() + "FP.png");

                //var troo = wad.ReadSprite("TROOA1");

                smallRect = new SDL.SDL_Rect { h = 300, w = 300, x = start.posX - 150, y = start.posY - 150 };
                Marshal.StructureToPtr(smallRect, smalRectPtr, true);

                SDL.SDL_BlitSurface(surface, IntPtr.Zero, screenSurface, IntPtr.Zero);
                SDL.SDL_BlitSurface(ViewRenderer.MapSurface, smalRectPtr, screenSurface, IntPtr.Zero);

                SDL.SDL_UpdateWindowSurface(window);

                SDL.SDL_Delay(50);

            }
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        private readonly ViewRenderer viewRenderer = new ViewRenderer(WIDTH, HEIGHT);

        public ViewRenderer ViewRenderer
        {
            get { return viewRenderer; }
        }
    }
}
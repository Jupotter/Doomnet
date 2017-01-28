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
        private const int RENDER_WIDTH = 640;
        private const int RENDER_HEIGHT = 400;


        private const int SCREEN_WIDTH = 1440;
        private const int SCREEN_HEIGHT = 900;

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
                SCREEN_WIDTH, SCREEN_HEIGHT,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            var screenSurface = SDL.SDL_GetWindowSurface(window);

            Wad wad = new Wad(new FileStream(args[0], FileMode.Open));

            wad.Read();

            wad.LoadTextures();
            var levelDrawer = new LevelDrawer();
            wad.CreateTexturesForRenderer(ViewRenderer.Renderer);

            var level = wad.LoadLevel("E1M1");


            levelDrawer.SaveImage(level);

            var start = level.Things.First(t => t.type == 1);

            SDL.SDL_Rect smallRect;
            IntPtr smalRectPtr;
            unsafe
            {
                smalRectPtr = Marshal.AllocHGlobal(sizeof (SDL.SDL_Rect));
            }
            int angle = start.angle;

            bool end = false;
            var showDebug = false;
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
                                angle += 10;
                            }
                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_RIGHT)
                            {
                                angle -= 10;
                            }
                            
                            if (angle < 0)
                                angle = 360 + angle;
                            angle = angle%360;

                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_UP)
                            {
                                start.posX += (short)(Math.Cos(angle * Math.PI / 180) * 25);
                                start.posY += (short)(Math.Sin(angle * Math.PI / 180) * 25);
                            }
                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_DOWN)
                            {
                                start.posX -= (short)(Math.Cos(angle * Math.PI/180) * 25);  
                                start.posY -= (short)(Math.Sin(angle * Math.PI/180) * 25);
                            }

                            if (@event.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_SPACE)
                            {
                                showDebug = !showDebug;
                            }
                            break;
                    }
                }
                //angle++;
                var surface = ViewRenderer.DrawVision(start, angle, level);

                //bitmap.Save(level.Definition.Name.Trim() + "FP.png");

                //var troo = wad.ReadSprite("TROOA1");

                smallRect = new SDL.SDL_Rect { h = 1000, w = 1000, x = start.posX - 500, y = start.posY - 500 };
                var fullRect = new SDL.SDL_Rect { h = 500, w = 500, x = 0, y = 0 };
                var screenRect = new SDL.SDL_Rect { h = SCREEN_HEIGHT, w = SCREEN_WIDTH, x = 0, y = 0 };
                var renderRect = new SDL.SDL_Rect { h = RENDER_HEIGHT, w = RENDER_WIDTH, x = 0, y = 0 };
                Marshal.StructureToPtr(smallRect, smalRectPtr, true);

                SDL.SDL_BlitScaled(surface, ref renderRect, screenSurface, ref screenRect);
                if (showDebug)
                    SDL.SDL_BlitScaled(ViewRenderer.MapSurface, ref smallRect, screenSurface, ref fullRect);

                SDL.SDL_UpdateWindowSurface(window);
                

            }
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        private readonly ViewRenderer viewRenderer = new ViewRenderer(RENDER_WIDTH, RENDER_HEIGHT);

        public ViewRenderer ViewRenderer
        {
            get { return viewRenderer; }
        }
    }
}
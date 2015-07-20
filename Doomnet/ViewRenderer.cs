using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace Doomnet
{
    internal class ViewRenderer
    {

        private const double TO_RADIAN =  Math.PI / 180;
        private readonly int sWidth;
        private readonly int sHeight;
        private readonly IntPtr viewSurface;
        private readonly IntPtr renderer;
        private readonly IntPtr mapRenderer;
        private readonly IntPtr mapSurface = SDL.SDL_CreateRGBSurface(0, 5000, 5000, 32, 0, 0, 0, 0);

        public ViewRenderer(int width, int height)
        {
            this.sWidth = width;
            this.sHeight = height;

            viewSurface = SDL.SDL_CreateRGBSurface(0, width, height, 32, 0, 0, 0, 0);
            renderer = SDL.SDL_CreateSoftwareRenderer(ViewSurface);
            mapRenderer = SDL.SDL_CreateSoftwareRenderer(MapSurface);
        }

        public IntPtr MapSurface
        {
            get { return mapSurface; }
        }

        public IntPtr ViewSurface
        {
            get { return viewSurface; }
        }

        public IntPtr DrawVision(Thing start, int angle, Level level)
        {
            SDL.SDL_SetRenderDrawColor(mapRenderer, 100, 149, 237, 255);
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderFillRect(mapRenderer, IntPtr.Zero);
            SDL.SDL_RenderFillRect(renderer, IntPtr.Zero);

            int width = level.Width;
            int height = level.Height;

            const double baseAngle = 90;
            int p0X = start.posX;
            int p0Y = start.posY;
            for (int i = 0; i < sWidth; i++)
            {
                var aDelta = baseAngle*i/sWidth - baseAngle/2;
                var alph = start.angle + aDelta + angle;
                alph = alph % 360;

                int p1X;
                int p1Y;

                p1X = 0;
                p1Y = 0;
                if (alph >= 0 && alph < 90)
                {
                    p1Y = (int)(p0X * Math.Tan(alph * TO_RADIAN)) + p0Y;
                    p1X = width;
                }
                else if (alph >= 90 && alph < 180)
                {
                    p1Y = p0Y - (int)(p0X * Math.Tan(alph * TO_RADIAN));
                    p1X = 0;
                }
                else if (alph >= 180 && alph < 270)
                {
                    p1Y = p0Y - (int)(p0X * Math.Tan(alph * TO_RADIAN));
                    p1X = 0;
                }
                else
                {
                    p1X = p0X - (int)(p0Y * Math.Tan((90 - alph) * TO_RADIAN));
                    p1Y = 0;
                }

                var segment =
                    level.Linedefs.FirstOrDefault(
                        seg => FindIntersection(p0X, p0Y, p1X, p1Y, seg.start.X, seg.start.Y, seg.end.X, seg.end.Y) !=
                               null && (seg.flags & Linedef.Flags.Impassible) != 0 &&
                               (seg.flags & Linedef.Flags.TwoSided) == 0);

                if (segment == null)
                {
                    //graphics.DrawLine(new Pen(Color.FromArgb(100, Color.Red)), start.posX, start.posY, p1X, p1Y);
                }
                else
                {
                    var intersection = FindIntersection(p0X, p0Y, p1X, p1Y, segment.start.X, segment.start.Y,
                        segment.end.X, segment.end.Y);

                    var distance = Math.Pow(start.posX - intersection.Item1, 2) +
                                   Math.Pow(start.posY - intersection.Item2, 2);
                    distance = Math.Sqrt(distance) * Math.Cos(aDelta * TO_RADIAN);

                    var color =(byte)((distance/level.Width * 4)*255);

                    SDL.SDL_SetRenderDrawColor(renderer,
                        color,color,color, 255);

                    SDL.SDL_SetRenderDrawColor(mapRenderer,
                        color,color,color, 255);

                    int colHeight = (int)(300 / distance * 150 ) ;
                    SDL.SDL_RenderDrawLine(renderer, i, sHeight/2 - colHeight/2, i, sHeight/2 + colHeight/2);
                    SDL.SDL_RenderDrawLine(mapRenderer, start.posX, start.posY, intersection.Item1,
                        intersection.Item2);
                }
            }


            //Marshal.Copy(pixels, 0, structure.pixels, 1024 * 768);
            SDL.SDL_RenderPresent(renderer);
            SDL.SDL_RenderPresent(mapRenderer);

            return ViewSurface;
        }

        public static Tuple<int, int> FindIntersection(int p0X, int p0Y, int p1X, int p1Y,
            int p2X, int p2Y, int p3X, int p3Y)
        {
            double s1X = p1X - p0X;
            double s1Y = p1Y - p0Y;
            double s2X = p3X - p2X;
            double s2Y = p3Y - p2Y;

            try
            {
                double s = (-s1Y * (p0X - p2X) + s1X * (p0Y - p2Y)) / (-s2X * s1Y + s1X * s2Y);

                double t = (s2X * (p0Y - p2Y) - s2Y * (p0X - p2X)) / (-s2X * s1Y + s1X * s2Y);

                if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
                {
                    return new Tuple<int, int>((int)(p0X + t * s1X),
                        (int)(p0Y + t * s1Y));
                }
            }
            catch (DivideByZeroException)
            {
                return null;
            }
            return null;
        }
    }
}
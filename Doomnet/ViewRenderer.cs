using System;
using System.IO;
using SDL2;
using static System.Math;

namespace Doomnet
{
    public class ViewRenderer
    {

        private const double TO_RADIAN =  PI / 180;
        private readonly int sWidth;
        private readonly int sHeight;
        private readonly IntPtr viewSurface;
        private readonly IntPtr renderer;
        private readonly IntPtr mapRenderer;
        private readonly IntPtr mapSurface = SDL.SDL_CreateRGBSurface(0, 5000, 5000, 32, 0, 0, 0, 0);

        public ViewRenderer(int width, int height)
        {
            sWidth = width;
            sHeight = height;

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

        public IntPtr Renderer
        {
            get { return renderer; }
        }

        public Tuple<double, double> GetSideLine(int posX, int posY, double angle, int width, int height)
        {
            int p0X = posX;
            int p0Y = posY;

            var alph = angle;
            double p1X;
            double p1Y;

            if (alph >= 0 && alph < 45)
            {
                p1Y = (width - p0X)*Tan(alph*TO_RADIAN) + p0Y;
                p1X = width;
            }
            else if (alph >= 45 && alph < 135)
            {
                p1X = p0X - (height-p0Y)*Tan((alph - 90)*TO_RADIAN);
                p1Y = height;
            }
            else if (alph >= 135 && alph < 225)
            {
                p1Y = p0Y - p0X*Tan((alph - 180)*TO_RADIAN);
                p1X = 0;
            }
            else if (alph >= 225 && alph < 315)
            {
                p1X = p0Y*Tan((alph - 270)*TO_RADIAN) + p0X;
                p1Y = 0;
            }
            else if (alph < 360 && alph >= 315)
            {
                p1Y = (width - p0X) * Tan(alph * TO_RADIAN) + p0Y;
                p1X = width;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(angle), $"Angle must be between 0 and 360, value is {angle}");
            }

            return new Tuple<double, double>(p1X, p1Y);
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
            double minAngle = 0;
            for (int i = 0; i < sWidth; i++)
            {
                var aDelta = baseAngle*i/sWidth - baseAngle/2;
                var alph = aDelta + angle;
                if (alph < 0)
                    alph += 360;
                alph = alph%360;

                var point = GetSideLine(p0X, p0Y, alph, width, height);

                var p1X = point.Item1;
                var p1Y = point.Item2;

                Linedef segment = null;
                var minDistance = int.MaxValue;
                Tuple<double, double> intersection = null;

                foreach (var seg in level.Linedefs)
                {
                    var locIntersection = FindIntersection(p0X, p0Y, p1X, p1Y,
                        seg.start.X, seg.start.Y, seg.end.X, seg.end.Y);

                    if (locIntersection == null
                        || seg.right.middle == null)
                        continue;

                    var distance = Pow(start.posX - locIntersection.Item1, 2) +
                                   Pow(start.posY - locIntersection.Item2, 2);
                    if (!(distance < minDistance)) continue;
                    segment = seg;
                    minDistance = (int) distance;
                    intersection = locIntersection;
                }

                if (segment == null)
                {
                    SDL.SDL_SetRenderDrawColor(mapRenderer,
                        255, 0, 0, 255);
                    SDL.SDL_RenderDrawLine(mapRenderer, start.posX, start.posY, (int) p1X, (int) p1Y);
                }
                else
                {
                    var distance = Pow(start.posX - intersection.Item1, 2) +
                                   Pow(start.posY - intersection.Item2, 2);
                    distance = Sqrt(distance)*Cos(aDelta*TO_RADIAN);

                    var offset = (int) Sqrt(Pow(intersection.Item1 - segment.start.X, 2) +
                                            Pow(intersection.Item2 - segment.start.Y, 2));

                    var texture = segment.right.middle;

                    offset = offset%texture.Width;

                    var color = (byte) ((distance/level.Width*4)*255);

                    SDL.SDL_SetRenderDrawColor(renderer,
                        color, color, color, 255);

                    SDL.SDL_SetRenderDrawColor(mapRenderer,
                        color, color, color, 255);

                    int colHeight = (int) (300/distance*150);
                    //SDL.SDL_RenderDrawLine(renderer, i, sHeight/2 - colHeight/2, i, sHeight/2 + colHeight/2);
                    //SDL.SDL_RenderDrawLine(mapRenderer, start.posX, start.posY, (int)intersection.Item1,
                    //    (int)intersection.Item2);

                    SDL.SDL_RenderDrawLine(mapRenderer, start.posX, start.posY, (int) p1X, (int) p1Y);

                    var srcrect = new SDL.SDL_Rect
                    {
                        h = texture.Height,
                        w = 1,
                        x = offset,
                        y = 0
                    };
                    var dstrect = new SDL.SDL_Rect
                    {
                        h = colHeight,
                        w = 1,
                        x = i,
                        y = sHeight/2 - colHeight/2
                    };
                    SDL.SDL_RenderCopy(renderer, texture.SdlTexture, ref srcrect, ref dstrect);
                }
            }

            //Marshal.Copy(pixels, 0, structure.pixels, 1024 * 768);
            SDL.SDL_RenderPresent(renderer);
            SDL.SDL_RenderPresent(mapRenderer);

            return ViewSurface;
        }

        public static Tuple<double, double> FindIntersection(double p0X, double p0Y, double p1X, double p1Y,
            double p2X, double p2Y, double p3X, double p3Y)
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
                    return new Tuple<double, double>(p0X + t * s1X, p0Y + t * s1Y);
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
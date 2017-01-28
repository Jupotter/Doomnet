using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Math;

namespace Doomnet
{
    public class ViewRenderer
    {
        private const double TO_RADIAN = PI / 180;
        private readonly int sWidth;
        private readonly int sHeight;
        private readonly IntPtr viewSurface;
        private readonly IntPtr renderer;
        private readonly IntPtr mapRenderer;
        private readonly IntPtr mapSurface = SDL.SDL_CreateRGBSurface(0, 5000, 5000, 32, 0, 0, 0, 0);
        private double aDelta;

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

        /// <summary>
        /// Get point intersecting the side of the level bounding box
        /// </summary>
        /// <param name="posX">X position of the player</param>
        /// <param name="posY">Y position of the player</param>
        /// <param name="angle">View angle of the player</param>
        /// <param name="width">width of the level Bounding box</param>
        /// <param name="height">height of the level bounding box</param>
        /// <returns></returns>
        public Tuple<double, double> GetSideLine(int posX, int posY, double angle, int width, int height)
        {
            int p0X = posX;
            int p0Y = posY;

            var alph = angle;
            double p1X;
            double p1Y;

            if (alph >= 0 && alph < 45)
            {
                p1Y = (width - p0X) * Tan(alph * TO_RADIAN) + p0Y;
                p1X = width;
            }
            else if (alph >= 45 && alph < 135)
            {
                p1X = p0X - (height - p0Y) * Tan((alph - 90) * TO_RADIAN);
                p1Y = height;
            }
            else if (alph >= 135 && alph < 225)
            {
                p1Y = p0Y - p0X * Tan((alph - 180) * TO_RADIAN);
                p1X = 0;
            }
            else if (alph >= 225 && alph < 315)
            {
                p1X = p0Y * Tan((alph - 270) * TO_RADIAN) + p0X;
                p1Y = 0;
            }
            else if (alph < 360 && alph >= 315)
            {
                p1Y = (width - p0X) * Tan(alph * TO_RADIAN) + p0Y;
                p1X = width;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(angle),
                    $"Angle must be between 0 and 360, value is {angle}");
            }

            return new Tuple<double, double>(p1X, p1Y);
        }

        private Level level;
        private int column;
        private PointD point1 = new PointD();
        private PointD point2 = new PointD();

        /// <summary>
        /// Draw what is visible to the player
        /// </summary>
        /// <param name="start">Player position</param>
        /// <param name="angle">Player view angle</param>
        /// <param name="level">Current level</param>
        /// <returns></returns>
        public IntPtr DrawVision(Thing start, int angle, Level level)
        {
            this.level = level;
            SDL.SDL_SetRenderDrawColor(mapRenderer, 100, 149, 237, 255);
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderFillRect(mapRenderer, IntPtr.Zero);
            SDL.SDL_RenderFillRect(renderer, IntPtr.Zero);

            int width = level.Width;
            int height = level.Height;

            const double baseAngle = 90;
            int p0X = start.posX;
            int p0Y = start.posY;
            var rootNode = level.RootNode;

            var i = 0;
            aDelta = baseAngle * i / sWidth - baseAngle / 2;
            var alpha = aDelta + angle;
            if (alpha < 0)
                alpha += 360;
            alpha = alpha % 360;

            var point = GetSideLine(p0X, p0Y, alpha, width, height);

            var p1X = point.Item1;
            var p1Y = point.Item2;

            point2 = new PointD { X = p1X, Y = p1Y };

            SortSectors(rootNode, new PointD { X = start.posX, Y = start.posY });

            SDL.SDL_SetRenderDrawColor(mapRenderer, 255, 0, 0, 255);

            SDL.SDL_RenderDrawLine(mapRenderer, (int)p0X, (int)p0Y, (int)p1X, (int)p1Y);

            //foreach (var sector in sectors)
            //{
            //    column = i;
            //    //RenderNode(rootNode, new PointD {X = start.posX, Y = start.posY}, new PointD {X = p1X, Y = p1Y});
            //    RenderSsector(sector, new PointD {X = start.posX, Y = start.posY}, new PointD {X = p1X, Y = p1Y});
            //}

            //Marshal.Copy(pixels, 0, structure.pixels, 1024 * 768);
            SDL.SDL_RenderPresent(renderer);
            SDL.SDL_RenderPresent(mapRenderer);

            return ViewSurface;
        }

        private double GetPointsAngle(PointD a, PointD b, PointD c)
        {
            var ab2 = Pow(a.X - b.X, 2) + Pow(a.Y - b.Y, 2);
            var ac2 = Pow(a.X - c.X, 2) + Pow(a.Y - c.Y, 2);
            var bc2 = Pow(b.X - c.X, 2) + Pow(b.Y - c.Y, 2);

            var angle = Acos((ab2 + ac2 - bc2) / (2 * Sqrt(ab2) * Sqrt(ac2)));

            if (FindSide(a, b, c) < 0)
                angle = -angle;
            return angle;
        }

        private int AngleToScreen(double angle)
        {
            angle = -45.0 * TO_RADIAN + angle;
            if (angle < 0)
                angle = 2 * PI + angle;
            return (int)(Tan(angle) * sWidth / 2) + sWidth / 2;
        }

        private void RenderSsector(SSector sect, PointD start, PointD endLeft)
        {
            foreach (var segment in sect.Segments)
            {
                var line = segment.line;

                var leftAngle = GetPointsAngle(start, endLeft, new PointD { X = segment.start.X, Y = segment.start.Y });
                var rightAngle = GetPointsAngle(start, endLeft, new PointD { X = segment.end.X, Y = segment.end.Y });

                if (leftAngle > rightAngle)
                {
                    continue;
                }
                if (leftAngle > 90.0 * TO_RADIAN)
                {
                    continue;
                }
                if (rightAngle < 0)
                {
                    continue;
                }

                var side = FindSide(new PointD { X = segment.start.X, Y = segment.start.Y },
                    new PointD { X = segment.end.X, Y = segment.end.Y },
                    start);

                var sidedef = side > 0 ? line.right : line.left;
                var oSidedef = side > 0 ? line.left : line.right;
                if (sidedef == null)
                    continue;

                var distanceLeft = Sqrt(Pow(start.X - segment.start.X, 2) +
                               Pow(start.Y - segment.start.Y, 2));
                var distanceRight = Sqrt(Pow(start.X - segment.end.X, 2) +
                               Pow(start.Y - segment.end.Y, 2));
                //distance = Sqrt(distance)*Cos(aDelta*TO_RADIAN);

                var offset = segment.offset;

                Texture texture;
                short bottom;
                short top;
                var distanceDelt = distanceRight - distanceLeft;
                var min = AngleToScreen(leftAngle);
                var max = AngleToScreen(rightAngle);

                if (min > sWidth || max < 0)
                    continue;

                var color = (byte)((distanceLeft / level.Width * 4) * 255);

                SDL.SDL_SetRenderDrawColor(mapRenderer, color, color, color, 255);
                SDL.SDL_SetRenderDrawColor(mapRenderer, (byte)segment.start.X, (byte)segment.start.Y, (byte)segment.angle, 255);

                //SDL.SDL_RenderDrawLine(mapRenderer, (int)start.X, (int)start.Y, segment.start.X, segment.start.Y);
                //SDL.SDL_RenderDrawLine(mapRenderer, (int)start.X, (int)start.Y, segment.end.X, segment.end.Y);

                if (oSidedef != null )
                {
                    for (int i = min; i < max; i++)
                    {
                        texture = sidedef.lower;

                        var distance = distanceLeft + (distanceDelt) * (i - min) / (max - min);
                        if (texture != null)
                        {
                            bottom = sidedef.sector.bottom;
                            top = oSidedef.sector.bottom;
                            var loffset = (short)((offset + i) % texture.Width);
                            DrawColumn(top, bottom, distance, texture, loffset, i);
                        }
                        texture = sidedef.upper;
                        if (texture != null)
                        {
                            bottom = oSidedef.sector.top;
                            top = sidedef.sector.top;
                            var loffset = (short)((offset + i) % texture.Width);
                            DrawColumn(top, bottom, distance, texture, loffset, i);
                        }
                        texture = sidedef.middle;

                        if (texture != null)
                        {
                            bottom = oSidedef.sector.bottom;
                            top = oSidedef.sector.top;
                            var loffset = (short)((offset + i) % texture.Width);
                            DrawColumn(top, bottom, distance, texture, loffset, i);
                        }
                    }
                    continue;
                }

                texture = sidedef.middle;

                if (texture == null)
                    continue;
                bottom = sidedef.sector.bottom;
                top = sidedef.sector.top;

                for (int i = min; i < max; i++)
                {
                    var loffset = (short)((offset + i) % texture.Width);
                    var distance = distanceLeft + (distanceDelt) * (i - min) / (max - min);
                    DrawColumn(top, bottom, distance, texture, loffset, i);
                }
            }
        }

        private void DrawColumn(short top, short bottom, double distance, Texture texture, int offset, int column)
        {
            var wallHeight = top - bottom;
            var colHeight = (int)(wallHeight / distance * sHeight);
            var viewBottom = (int)((bottom - 64)  / distance * sHeight);

            var srcrect = new SDL.SDL_Rect
            {
                h = colHeight,
                w = 1,
                x = offset,
                y = 0
            };
            var dstrect = new SDL.SDL_Rect
            {
                h = colHeight,
                w = 1,
                x = sWidth - column,
                y = sHeight/2 - (viewBottom + colHeight)
            };

            SDL.SDL_RenderCopy(renderer, texture.SdlTexture, ref srcrect, ref dstrect);
        }

        private int FindSide(PointD start, PointD end, PointD point)
        {
            return Sign((end.X - start.X) * (point.Y - start.Y) -
                        (end.Y - start.Y) * (point.X - start.X));
        }

        private void SortSectors(Node node, PointD start)
        {
            var side = Sign((node.EndX - node.StartX) * (start.Y - node.StartY) -
                            (node.EndY - node.StartY) * (start.X - node.StartX));

            SDL.SDL_SetRenderDrawColor(mapRenderer, 0, 0, 0, 255);
            SDL.SDL_RenderDrawLine(mapRenderer, (int)node.StartX, (int)node.StartY, node.EndX, node.EndX);

            side = -side;

            if (side > 0)
            {
                List<SSector> l;
                if (node.LeftNode != null)
                    SortSectors(node.LeftNode, start);
                else
                {
                    RenderSsector(node.LeftSector, start, point2);
                }

                if (node.RightNode != null)
                    SortSectors(node.RightNode, start);
                else
                {
                    RenderSsector(node.RightSector, start, point2);
                }
            }
            else if (side < 0)
            {
                List<SSector> l;
                if (node.RightNode != null)
                    SortSectors(node.RightNode, start);
                else
                {
                    RenderSsector(node.RightSector, start, point2);
                }

                if (node.LeftNode != null)
                    SortSectors(node.LeftNode, start);
                else
                {
                    RenderSsector(node.LeftSector, start, point2);
                }
            }
            else
            {
                return;
                //Debugger.Break();
            }
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SDL2;

namespace Doomnet
{
    class Level
    {
        private List<Vertex> vertices = new List<Vertex>();
        private List<Segment> segments = new List<Segment>();
        private List<Linedef> linedefs = new List<Linedef>();
        private List<Sidedef> sidedefs = new List<Sidedef>(); 
        private List<Thing> things = new List<Thing>(); 
        private LevelDef definition;

        public Level(LevelDef definition)
        {
            this.definition = definition;
        }

        public void Read(Stream stream)
        {
            ReadVertices(stream);
            ReadSegments(stream);
            ReadSidedefs(stream);
            ReadLinedefs(stream);
            ReadThings(stream);

            NormalizeLevel();
        }

        private void ReadThings(Stream stream)
        {
            stream.Seek(definition.THINGS.Offset, SeekOrigin.Begin);

            for (int i = 0; i < definition.THINGS.Size/10; i++)
            {
                var buffer = new byte[10];

                stream.Read(buffer, 0, 10);

                var s = new Thing()
                {
                    posX = BitConverter.ToInt16(buffer, 0),
                    posY = BitConverter.ToInt16(buffer, 2),
                    angle = BitConverter.ToInt16(buffer, 4),
                    type = BitConverter.ToInt16(buffer, 6),
                    options = BitConverter.ToInt16(buffer, 8)
                };

                things.Add(s);
            }
        }

        private void ReadSidedefs(Stream stream)
        {
            stream.Seek(definition.Sidedefs.Offset, SeekOrigin.Begin);

            for (int i = 0; i < definition.Sidedefs.Size/30; i++)
            {
                var buffer = new byte[30];

                stream.Read(buffer, 0, 30);

                var s = new Sidedef
                {
                    xOffset = BitConverter.ToInt16(buffer, 0),
                    yOffset = BitConverter.ToInt16(buffer, 2),
                    upper = Encoding.ASCII.GetString(buffer, 4, 8).Replace("\0", String.Empty),
                    lower = Encoding.ASCII.GetString(buffer, 12, 8).Replace("\0", String.Empty),
                    middle = Encoding.ASCII.GetString(buffer, 20, 8).Replace("\0", String.Empty),
                    sector = BitConverter.ToInt16(buffer, 28)
                };

                sidedefs.Add(s);
            }
        }

        private void ReadLinedefs(Stream stream)
        {
            stream.Seek(definition.Linedefs.Offset, SeekOrigin.Begin);

            for (int i = 0; i < definition.Linedefs.Size/14; i++)
            {
                var buffer = new byte[14];

                stream.Read(buffer, 0, 14);

                var start = vertices[BitConverter.ToInt16(buffer, 0)];
                var end = vertices[BitConverter.ToInt16(buffer, 2)];

                var leftnum = BitConverter.ToInt16(buffer, 12);
                var right = sidedefs[BitConverter.ToInt16(buffer, 10)];
                var left = leftnum != -1 ? sidedefs[leftnum] : null;

                var l = new Linedef
                {
                    start = start,
                    end = end,
                    flags = (Linedef.Flags) BitConverter.ToInt16(buffer, 4),
                    type = BitConverter.ToInt16(buffer, 6),
                    tag = BitConverter.ToInt16(buffer, 8),
                    right = right,
                    left = left
                };

                linedefs.Add(l);
            }
        }

        private void ReadSegments(Stream stream)
        {
            stream.Seek(definition.SEGS.Offset, SeekOrigin.Begin);

            for (int i = 0; i < definition.SEGS.Size/12; i++)
            {
                var buffer = new byte[12];

                stream.Read(buffer, 0, 12);

                var start = vertices[BitConverter.ToInt16(buffer, 0)];
                var end = vertices[BitConverter.ToInt16(buffer, 2)];

                var s = new Segment
                {
                    start = start,
                    end = end,
                    angle = BitConverter.ToInt16(buffer, 4),
                    line = BitConverter.ToInt16(buffer, 6),
                    reverse = BitConverter.ToInt16(buffer, 8) != 0,
                    offset = BitConverter.ToInt16(buffer, 10)
                };

                segments.Add(s);
            }
        }

        private void NormalizeLevel()
        {
            var minX = vertices.Min(v => v.X);
            var minY = vertices.Min(v => v.Y);
            foreach (var vertex in vertices)
            {
                vertex.X -= minX;
                vertex.Y -= minY;
            }

            foreach (var thing in things)
            {
                thing.posX -= minX;
                thing.posY -= minY;
            }
        }

        private void ReadVertices(Stream stream)
        {
            stream.Seek(definition.VERTEXES.Offset, SeekOrigin.Begin);

            for (int i = 0; i < definition.VERTEXES.Size/4; i++)
            {
                var buffer = new byte[4];

                stream.Read(buffer, 0, 4);

                var v = new Vertex
                {
                    X = BitConverter.ToInt16(buffer, 0),
                    Y = BitConverter.ToInt16(buffer, 2),
                };

                vertices.Add(v);
            }
        }

        public void SaveImage()
        {
            var width = vertices.Max(v => v.X) + 1;
            var height = vertices.Max(v => v.X) + 1;
            var bitmap = new Bitmap(width, height);

            foreach (var vertex in vertices)
            {
                bitmap.SetPixel(vertex.X, vertex.Y, Color.Black);
            }

            var penNormal = new Pen(Color.Black);
            var penImpass = new Pen(Color.Black, 3);
            var penSecret = new Pen(Color.DarkGreen);
            var penInvis = new Pen(Color.Gray);
            var penTwoside = new Pen(Color.Red);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var segment in segments)
                {
                    var line = linedefs.FirstOrDefault(l => (segment.start == l.start && segment.end == l.end)
                        || (segment.end == l.start && segment.start == l.end));
                    
                    Pen pen;
                    if (line != null)
                    {
                        switch (line.flags)
                        {
                            case Linedef.Flags.Secret:
                                pen = penSecret;
                                break;
                            case Linedef.Flags.NotOnMap:
                                pen = penInvis;
                                break;
                            case Linedef.Flags.Impassible:
                                pen = penImpass;
                                break;
                            case Linedef.Flags.TwoSided:
                                pen = penTwoside;
                                break;
                            default:
                                pen = penNormal;
                                break;
                        }
                        if (line.left == null)
                            pen = penImpass;
                    }
                    else
                    {
                        pen = penNormal;
                    }
                    graphics.DrawLine(pen, segment.start.X, segment.start.Y, segment.end.X, segment.end.Y);
                }

                foreach (var thing in things)
                {
                    graphics.DrawLine(Pens.Red, thing.posX - 5, thing.posY - 5, thing.posX + 5, thing.posY + 5);
                    graphics.DrawLine(Pens.Red, thing.posX - 5, thing.posY + 5, thing.posX + 5, thing.posY - 5);
                }
            }

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bitmap.Save(definition.Name.Trim() + ".png");
        }

        public IntPtr Display()
        {
            var width = vertices.Max(v => v.X);
            var height = vertices.Max(v => v.X);

            var surface = SDL.SDL_CreateRGBSurface(0,
                width, height,
                32, 0, 0, 0, 0);

            var structure = (SDL.SDL_Surface)Marshal.PtrToStructure(surface, typeof(SDL.SDL_Surface));
            var color = (int)SDL.SDL_MapRGBA(structure.format, 255, 255, 255, 255);

            var pixels = new Int32[width * height];

            foreach (var vertex in vertices)
            {
                pixels[vertex.Y * width + vertex.X] = color;
            }

            Marshal.Copy(pixels, 0, structure.pixels, width * height);

            return surface;
        }
    }
}

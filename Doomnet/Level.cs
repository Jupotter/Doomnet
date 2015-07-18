using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace Doomnet
{
    class Level
    {
        internal class Linedef
        {
            [Flags]
            public enum Flags
            {
                Impassible = 1 << 0,
                BlockMonsters = 1 << 1,
                TwoSided  = 1 << 2,
                UpperUnpegged = 1 << 3,
                LowerUnpegged  =1 << 4,
                Secret  = 1 << 5,
                BlockSound = 1 << 6,
                NotOnMap = 1 << 7,
                AlreadyOnMap = 1 << 8,
            }

            public Vertex start, end;
            public Flags flags;
            public short type;
            public short tag;
            public short right, left;
        }

        private class Segment
        {
            public Vertex start, end;
            public short angle;
            public short line;
            public bool reverse;
            public short offset;

            public override string ToString()
            {
                return String.Format("{0} --> {1}", start, end);
            }
        }

        public class Vertex
        {
            public short X;
            public short Y;

            public override string ToString()
            {
                return String.Format("{0}:{1}", X, Y);
            }
        }

        private List<Vertex> vertices = new List<Vertex>();
        private List<Segment> segments = new List<Segment>();
        private List<Linedef> linedefs = new List<Linedef>(); 
        private LevelDef definition;

        public Level(LevelDef definition)
        {
            this.definition = definition;
        }

        public void Read(Stream stream)
        {
            ReadVertices(stream);

            ReadSegments(stream);

            ReadLinedefs(stream);
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

                var l = new Linedef
                {
                    start = start,
                    end = end,
                    flags = (Linedef.Flags) BitConverter.ToInt16(buffer, 4),
                    type = BitConverter.ToInt16(buffer, 6),
                    tag = BitConverter.ToInt16(buffer, 8),
                    right = BitConverter.ToInt16(buffer, 10),
                    left = BitConverter.ToInt16(buffer, 12),
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

        private void NormalizeVertices()
        {
            var minX = vertices.Min(v => v.X);
            var minY = vertices.Min(v => v.Y);
            foreach (var vertex in vertices)
            {
                vertex.X -= minX;
                vertex.Y -= minY;
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

            NormalizeVertices();
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
                    else
                    {
                        pen = penNormal;
                    }
                    graphics.DrawLine(pen, segment.start.X, segment.start.Y, segment.end.X, segment.end.Y);
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

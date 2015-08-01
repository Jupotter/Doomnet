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
    public class Level
    {
        private readonly List<Vertex> vertices = new List<Vertex>();
        private readonly List<Segment> segments = new List<Segment>();
        private readonly List<Linedef> linedefs = new List<Linedef>();
        private readonly List<Sidedef> sidedefs = new List<Sidedef>(); 
        private readonly List<Thing> things = new List<Thing>();
        private readonly List<Sector> sectors = new List<Sector>();
        private readonly LevelDef definition;
        private int width = 0;
        private int height = 0;

        public Level(LevelDef definition)
        {
            this.definition = definition;
        }

        public List<Vertex> Vertices
        {
            get { return vertices; }
        }

        public List<Segment> Segments
        {
            get { return segments; }
        }

        public List<Linedef> Linedefs
        {
            get { return linedefs; }
        }

        public List<Sidedef> Sidedefs
        {
            get { return sidedefs; }
        }

        public List<Thing> Things
        {
            get { return things; }
        }

        public LevelDef Definition
        {
            get { return definition; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public void Read(Stream stream, Dictionary<string, Texture> textures)
        {
            ReadVertices(stream);
            ReadSegments(stream);
            ReadSectors(stream);
            ReadSidedefs(stream, textures);
            ReadLinedefs(stream);
            ReadThings(stream);

            NormalizeLevel();
        }

        private void ReadSectors(Stream stream)
        {
            stream.Seek(Definition.SECTORS.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.SECTORS.Size / 26; i++)
            {
                var buffer = new byte[26];

                stream.Read(buffer, 0, 26);

                var s = new Sector()
                {
                    bottom = BitConverter.ToInt16(buffer, 0),
                    top = BitConverter.ToInt16(buffer, 2),
                    ground = Encoding.ASCII.GetString(buffer, 4, 8).Replace("\0", string.Empty),
                    ceiling = Encoding.ASCII.GetString(buffer, 12, 8).Replace("\0", string.Empty),
                    light = BitConverter.ToInt16(buffer, 20),
                    type = BitConverter.ToInt16(buffer, 22),
                    tag = BitConverter.ToInt16(buffer, 24),
                };

                sectors.Add(s);
            }
        }

        private void ReadThings(Stream stream)
        {
            stream.Seek(Definition.THINGS.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.THINGS.Size/10; i++)
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

                Things.Add(s);
            }
        }

        private void ReadSidedefs(Stream stream, Dictionary<string, Texture> textures)
        {
            stream.Seek(Definition.Sidedefs.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.Sidedefs.Size/30; i++)
            {
                var buffer = new byte[30];

                stream.Read(buffer, 0, 30);

                var upper = Encoding.ASCII.GetString(buffer, 4, 8).Replace("\0", string.Empty);
                var lower = Encoding.ASCII.GetString(buffer, 12, 8).Replace("\0", string.Empty);
                var middle = Encoding.ASCII.GetString(buffer, 20, 8).Replace("\0", string.Empty);

                var s = new Sidedef
                {
                    xOffset = BitConverter.ToInt16(buffer, 0),
                    yOffset = BitConverter.ToInt16(buffer, 2),
                    sector = sectors[BitConverter.ToInt16(buffer, 28)],
                    upper = upper != "-" ? textures[upper] : null,
                    middle = middle != "-" ? textures[middle] : null,
                    lower = lower != "-" ? textures[lower] : null,
                };

                Sidedefs.Add(s);
            }
        }

        private void ReadLinedefs(Stream stream)
        {
            stream.Seek(Definition.Linedefs.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.Linedefs.Size/14; i++)
            {
                var buffer = new byte[14];

                stream.Read(buffer, 0, 14);

                var start = Vertices[BitConverter.ToInt16(buffer, 0)];
                var end = Vertices[BitConverter.ToInt16(buffer, 2)];

                var leftnum = BitConverter.ToInt16(buffer, 12);
                var right = Sidedefs[BitConverter.ToInt16(buffer, 10)];
                var left = leftnum != -1 ? Sidedefs[leftnum] : null;

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

                Linedefs.Add(l);
            }
        }

        private void ReadSegments(Stream stream)
        {
            stream.Seek(Definition.SEGS.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.SEGS.Size/12; i++)
            {
                var buffer = new byte[12];

                stream.Read(buffer, 0, 12);

                var start = Vertices[BitConverter.ToInt16(buffer, 0)];
                var end = Vertices[BitConverter.ToInt16(buffer, 2)];

                var s = new Segment
                {
                    start = start,
                    end = end,
                    angle = BitConverter.ToInt16(buffer, 4),
                    line = BitConverter.ToInt16(buffer, 6),
                    reverse = BitConverter.ToInt16(buffer, 8) != 0,
                    offset = BitConverter.ToInt16(buffer, 10)
                };

                Segments.Add(s);
            }
        }

        private void NormalizeLevel()
        {
            var minX = Vertices.Min(v => v.X);
            var minY = Vertices.Min(v => v.Y);
            foreach (var vertex in Vertices)
            {
                vertex.X -= minX;
                vertex.Y -= minY;
            }

            foreach (var thing in Things)
            {
                thing.posX -= minX;
                thing.posY -= minY;
            }

            width = Vertices.Max(v => v.X);
            height = Vertices.Max(v => v.X);
        }

        private void ReadVertices(Stream stream)
        {
            stream.Seek(Definition.VERTEXES.Offset, SeekOrigin.Begin);

            for (int i = 0; i < Definition.VERTEXES.Size/4; i++)
            {
                var buffer = new byte[4];

                stream.Read(buffer, 0, 4);

                var v = new Vertex
                {
                    X = BitConverter.ToInt16(buffer, 0),
                    Y = BitConverter.ToInt16(buffer, 2),
                };

                Vertices.Add(v);
            }
        }

        public IntPtr Display()
        {
            var width = Vertices.Max(v => v.X);
            var height = Vertices.Max(v => v.X);

            var surface = SDL.SDL_CreateRGBSurface(0,
                width, height,
                32, 0, 0, 0, 0);

            var structure = (SDL.SDL_Surface)Marshal.PtrToStructure(surface, typeof(SDL.SDL_Surface));
            var color = (int)SDL.SDL_MapRGBA(structure.format, 255, 255, 255, 255);

            var pixels = new Int32[width * height];

            foreach (var vertex in Vertices)
            {
                pixels[vertex.Y * width + vertex.X] = color;
            }

            Marshal.Copy(pixels, 0, structure.pixels, width * height);

            return surface;
        }
    }
}

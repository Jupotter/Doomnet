using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Doomnet
{
    class Level
    {
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
        private LevelDef definition;

        public Level(LevelDef definition)
        {
            this.definition = definition;
        }

        public void Read(Stream stream)
        {
            ReadVertices(stream);

            ReadSegments(stream);
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

            var pen = new Pen(Color.Black);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var segment in segments)
                {
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

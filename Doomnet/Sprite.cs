using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Doomnet
{
    class Sprite
    {
        private Int16 width;
        private Int16 height;
        private Int16 lOffset;
        private Int16 tOffset;

        private SDL.SDL_Color[,] data;
        private readonly Palette palette;

        private IntPtr surface;

        public Sprite(Palette palette)
        {
            this.palette = palette;
        }

        public IntPtr Surface
        {
            get { return surface; }
        }

        public void Read(Stream stream)
        {
            long start = stream.Position;
            var header = new byte[8];

            stream.Read(header, 0, 8);

            width = BitConverter.ToInt16(header, 0);
            height = BitConverter.ToInt16(header, 2);
            lOffset = BitConverter.ToInt16(header, 4);
            tOffset = BitConverter.ToInt16(header, 6);

            data = new SDL.SDL_Color[width, height];

            var columns = new Int32[width];
            var buffer = new Byte[width * 4];

            stream.Read(buffer, 0, width * 4);

            for (int i = 0; i < width; i++)
            {
                columns[i] = BitConverter.ToInt32(buffer, i * 4);
            }

            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                stream.Position = start + column;

                while (true)
                {

                    int row = stream.ReadByte();
                    if (row == 255)
                        break;
                    int number = stream.ReadByte();
                    stream.ReadByte();
                    for (int j = 0; j < number; j++)
                    {
                        int color = stream.ReadByte();
                        data[i, row + j] = palette.BaseColors[color];
                    }
                    stream.ReadByte();
                }
            }

            surface = SDL.SDL_CreateRGBSurface(0,
                width, height,
                32,0,0,0,0);

            var structure = (SDL.SDL_Surface) Marshal.PtrToStructure(surface, typeof (SDL.SDL_Surface));

            var pixels = new Int32[width*height];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var color = data[j, i];
                    pixels[i*width + j] = (int)SDL.SDL_MapRGBA(structure.format, color.r, color.g, color.b, color.a);
                }
            }

            Marshal.Copy(pixels, 0, structure.pixels, width*height);
        }
    }
}

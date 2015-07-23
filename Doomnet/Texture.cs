using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Doomnet
{
    public class Texture
    {
        private class Patch
        {
            public short x, y;
            public short number;
            public Sprite sprite;
        }

        private string name;
        private short width;
        private short height;
        private short size;

        private readonly List<Patch> patches = new List<Patch>();

        private IntPtr surface;
        private IntPtr texture;

        public string Name
        {
            get { return name; }
        }

        public int Width {
            get { return width; }
        }

        public IntPtr SdlTexture
        {
            get { return texture; }
        }

        public short Height
        {
            get { return height; }
        }

        public void Read(Stream stream, List<string> pnames, Wad wad)
        {
            var buffer = new byte[22];

            stream.Read(buffer, 0, 22);
            name = Encoding.ASCII.GetString(buffer, 0, 8).Replace("\0", String.Empty);

            width = BitConverter.ToInt16(buffer, 12);
            height = BitConverter.ToInt16(buffer, 14);
            size = BitConverter.ToInt16(buffer, 20);

            for (int i = 0; i < size; i++)
            {
                buffer = new byte[10];

                stream.Read(buffer, 0, 10);

                var p = new Patch
                {
                    x = BitConverter.ToInt16(buffer, 0),
                    y = BitConverter.ToInt16(buffer, 2),
                    number = BitConverter.ToInt16(buffer, 4),
                };

                var position = stream.Position;
                p.sprite = wad.ReadSprite(pnames[p.number]);

                stream.Seek(position, SeekOrigin.Begin);

                patches.Add(p);
            }


            surface = SDL.SDL_CreateRGBSurface(0,
                width, height,
                32, 0, 0, 0, 0);

            IntPtr rectPtr;
            unsafe
            {
                rectPtr = Marshal.AllocHGlobal(sizeof (SDL.SDL_Rect));
            }

            foreach (var patch in patches)
            {
                var sprite = patch.sprite;
                var smallRect = new SDL.SDL_Rect {h = sprite.Width, w = sprite.Height, x = patch.x, y = patch.y};
                Marshal.StructureToPtr(smallRect, rectPtr, false);
                SDL.SDL_BlitSurface(patch.sprite.Surface, IntPtr.Zero, surface, rectPtr);
            }
        }

        public void CreateTextureFromSurface(IntPtr renderer)
        {
            if (IntPtr.Zero == (texture = SDL.SDL_CreateTextureFromSurface(renderer, surface)))
            {
                throw new Exception(SDL.SDL_GetError());
            }
        }
    }
}

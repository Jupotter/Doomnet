using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace Doomnet
{
    class Palette
    {
        private readonly Dictionary<int, SDL.SDL_Color> baseColors = new Dictionary<int, SDL.SDL_Color>();

        public Dictionary<int, SDL.SDL_Color> BaseColors
        {
            get { return baseColors; }
        }

        public void Read(Stream stream)
        {
            byte[] buffer = new byte[768];

            stream.Read(buffer, 0, 768);

            for (int i = 0; i < 256; i++)
            {
                var color = new SDL.SDL_Color
                {
                    a = 255,
                    r = buffer[i*3],
                    g = buffer[i*3 + 1],
                    b = buffer[i*3 + 1]
                };

                BaseColors.Add(i, color);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doomnet
{
    class Sprite
    {
        private Int16 width;
        private Int16 height;
        private Int16 lOffset;
        private Int16 tOffset;

        private Byte[,] data;

        public void Read(Stream stream)
        {
            long start = stream.Position;
            Byte[] header = new byte[8];

            stream.Read(header, 0, 8);

            width = BitConverter.ToInt16(header, 0);
            height = BitConverter.ToInt16(header, 2);
            lOffset = BitConverter.ToInt16(header, 4);
            tOffset = BitConverter.ToInt16(header, 6);

            data = new byte[width, height];

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
                        data[i, row + j] = (byte)stream.ReadByte();
                    }
                    stream.ReadByte();
                }
            }

            Bitmap bitmap = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    bitmap.SetPixel(i, j, Color.FromArgb(data[i, j]));
                }
            }

            bitmap.Save("TestSprite.png");
        }
    }
}

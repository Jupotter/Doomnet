using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doomnet
{
    class Directory
    {
        public struct Entry
        {
            public Int32 Offset;
            public Int32 Size;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public readonly List<Entry> Entries = new List<Entry>(); 

        public void Read(Stream stream, Int32 size)
        {
            var buffer = new byte[16];
            for (int i = 0; i < size; i++)
            {
                stream.Read(buffer, 0, 16);

                var entry = new Entry
                {
                    Offset = BitConverter.ToInt32(buffer, 0),
                    Size = BitConverter.ToInt32(buffer, 4),
                    Name = Encoding.ASCII.GetString(buffer, 8, 8)
                };

                Entries.Add(entry);
            }
        }
    }
}

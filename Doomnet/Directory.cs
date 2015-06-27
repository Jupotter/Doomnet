using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Doomnet
{
    class Directory
    {
        private static readonly Regex LevelName = new Regex(@"E\dM\d");

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

        private List<Entry> levels;

        public IEnumerable<Entry> Levels
        {
            get { return levels; }
        }

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

            levels = Entries.FindAll(e => e.Size == 0 && LevelName.Match(e.Name).Success);
            levels = Levels.OrderBy(e => e.Name).ToList();
        }
    }
}

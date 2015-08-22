using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Doomnet
{
    public class Directory
    {
        private static readonly Regex LevelName = new Regex(@"E\dM\d");

        public class Entry
        {
            public Int32 Offset;
            public Int32 Size;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        private List<Entry> entries = new List<Entry>();

        private List<LevelDef> levels = new List<LevelDef>();

        public IEnumerable<LevelDef> Levels
        {
            get { return levels; }
        }

        public IEnumerable<Entry> Entries
        {
            get { return entries; }
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
                    Name = Encoding.ASCII.GetString(buffer, 8, 8).Replace("\0", String.Empty)
                };

                entries.Add(entry);
            }

            entries = entries.OrderBy(e => e.Offset).ToList();

            foreach (var entry in entries.FindAll(e => e.Size == 0 && LevelName.Match(e.Name).Success))
            {
                Entry entry1 = entry;
                var possible = Entries.SkipWhile(e => e.Name != entry1.Name);

                var enumerable = possible as IList<Entry> ?? possible.ToList();
                var level = new LevelDef
                {
                    Level = entry,
                    BLOCKMAP = enumerable.First(e => e.Name == "BLOCKMAP"),
                    Linedefs = enumerable.First(e => e.Name == "LINEDEFS"),
                    Sidedefs = enumerable.First(e => e.Name == "SIDEDEFS"),
                    VERTEXES = enumerable.First(e => e.Name == "VERTEXES"),
                    SEGS = enumerable.First(e => e.Name == "SEGS"),
                    THINGS = enumerable.First(e => e.Name == "THINGS"),
                    SECTORS = enumerable.First(e => e.Name == "SECTORS"),
                    SSECTORS = enumerable.First(e => e.Name == nameof(LevelDef.SSECTORS)),
                    NODES = enumerable.First(e => e.Name == nameof(LevelDef.NODES)),
                    REJECT = enumerable.First(e => e.Name == nameof(LevelDef.REJECT)),
                };
                levels.Add(level);
            }
            levels = Levels.OrderBy(e => e.Level.Name).ToList();
        }
    }
}
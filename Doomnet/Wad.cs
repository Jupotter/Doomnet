using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Doomnet.TestFiles
{
    class Wad
    {
        private readonly Stream stream;

        public Header Header;
        public Directory Directory;
        public Palette Palette;
        public List<Directory.Entry> Levels; 

        public Wad(Stream file)
        {
            stream = file;
        }

        public void Read()
        {
            Header = new Header();
            Header.Read(stream);

            stream.Position = Header.Offset;

            Directory = new Directory();
            Directory.Read(stream, Header.Lumps);

            Directory.Entry entry = Directory.Entries.First(e => e.Name.Contains("PLAYPAL"));

            stream.Position = entry.Offset;
            Palette = new Palette();
            Palette.Read(stream);
        }

        public Sprite ReadSprite(string name)
        {
            Directory.Entry spriteEntry = Directory.Entries.First(e => e.Name.Contains(name));

            stream.Position = spriteEntry.Offset;
            var sprite = new Sprite(Palette);
            sprite.Read(stream);

            return sprite;
        }
    }
}

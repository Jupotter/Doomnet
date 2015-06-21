using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doomnet.TestFiles
{
    class Wad
    {
        public Header Header;
        public Directory Directory;

        public void Read(Stream stream)
        {
            Header = new Header();
            Header.Read(stream);

            stream.Position = Header.Offset;

            Directory = new Directory();
            Directory.Read(stream, Header.Lumps);

            Directory.Entry imp = Directory.Entries.First(e => e.Name.Contains("TROOA1"));

            stream.Position = imp.Offset;

            Sprite sprite = new Sprite();
            sprite.Read(stream);
        }
    }
}

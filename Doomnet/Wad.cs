using System.IO;
using System.Linq;

namespace Doomnet
{
    internal class Wad
    {
        private readonly Stream stream;

        private Header header;
        private Directory directory;
        private Palette palette;

        public Wad(Stream file)
        {
            stream = file;
        }

        public void Read()
        {
            header = new Header();
            header.Read(stream);

            stream.Position = header.Offset;

            directory = new Directory();
            directory.Read(stream, header.Lumps);

            Directory.Entry entry = directory.Entries.First(e => e.Name.Contains("PLAYPAL"));

            stream.Position = entry.Offset;
            palette = new Palette();
            palette.Read(stream);
        }

        public Sprite ReadSprite(string name)
        {
            Directory.Entry spriteEntry = directory.Entries.First(e => e.Name.Contains(name));

            stream.Position = spriteEntry.Offset;
            var sprite = new Sprite(palette);
            sprite.Read(stream);

            return sprite;
        }

        public Level LoadLevel(string name)
        {
            var levelDef = directory.Levels.First(e => e.Name.Contains(name));

            var level = new Level(levelDef);
            level.Read(stream);

            return level;
        }
    }
}
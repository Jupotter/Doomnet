using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doomnet
{
    public class Wad
    {
        private readonly Stream stream;

        private Header header;
        private Directory directory;
        private Palette palette;

        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();  

        public Wad(Stream file)
        {
            stream = file;
        }

        public Dictionary<string, Texture> Textures
        {
            get { return textures; }
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
            level.Read(stream, Textures);

            return level;
        }

        public void LoadTextures()
        {
            stream.Seek(directory.Entries.First(e => e.Name.Contains("PNAMES")).Offset, SeekOrigin.Begin);

            var buffer = new Byte[4];
            stream.Read(buffer, 0, 4);

            var size = BitConverter.ToInt32(buffer, 0);
            var pnames = new List<string>();

            for (int i = 0; i < size; i++)
            {
                buffer = new byte[8];

                stream.Read(buffer, 0, 8);

                pnames.Add(Encoding.ASCII.GetString(buffer, 0, 8).Replace("\0", String.Empty).ToUpper());
            }

            stream.Seek(directory.Entries.First(e => e.Name.Contains("TEXTURE1")).Offset, SeekOrigin.Begin);

            buffer = new Byte[4];
            stream.Read(buffer, 0, 4);

            size = BitConverter.ToInt32(buffer, 0);

            stream.Seek(4*size, SeekOrigin.Current);

            for (int i = 0; i < size; i++)
            {
                Texture t= new Texture();
                t.Read(stream, pnames, this);

                textures.Add(t.Name, t);
            }
        }

        public void CreateTexturesForRenderer(IntPtr renderer)
        {
            foreach (var texture in Textures.Values)
            {
                texture.CreateTextureFromSurface(renderer);
            }
        }
    }
}
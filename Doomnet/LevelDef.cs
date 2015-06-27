using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doomnet
{
    struct LevelDef
    {
        public Directory.Entry Level;
        public Directory.Entry THINGS;
        public Directory.Entry Linedefs;
        public Directory.Entry Sidedefs;
        public Directory.Entry VERTEXES;
        public Directory.Entry SEGS;
        public Directory.Entry SSECTORS;
        public Directory.Entry NODES;
        public Directory.Entry SECTORS;
        public Directory.Entry REJECT;
        public Directory.Entry BLOCKMAP;

        public override string ToString()
        {
            return Name;
        }

        public string Name
        {
            get { return Level.Name; }
        }
    }
}

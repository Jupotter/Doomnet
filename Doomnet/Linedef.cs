using System;

namespace Doomnet
{
    public class Linedef
    {
        [Flags]
        public enum Flags
        {
            Impassible = 1 << 0,
            BlockMonsters = 1 << 1,
            TwoSided  = 1 << 2,
            UpperUnpegged = 1 << 3,
            LowerUnpegged  =1 << 4,
            Secret  = 1 << 5,
            BlockSound = 1 << 6,
            NotOnMap = 1 << 7,
            AlreadyOnMap = 1 << 8,
        }

        public Vertex start, end;
        public Flags flags;
        public short type;
        public short tag;
        public Sidedef right, left;
            
        public override string ToString()
        {
            return String.Format("{0} --> {1}", start, end);
        }
    }
}
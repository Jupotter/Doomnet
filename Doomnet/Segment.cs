using System;

namespace Doomnet
{
    public  class Segment
    {
        public Vertex start, end;
        public short angle;
        public Linedef line;
        public bool reverse;
        public short offset;

        public override string ToString()
        {
            return String.Format("{0} --> {1}", start, end);
        }
    }
}
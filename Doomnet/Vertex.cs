using System;

namespace Doomnet
{
    public class Vertex
    {
        public short X;
        public short Y;

        public override string ToString()
        {
            return String.Format("{0}:{1}", X, Y);
        }
    }
}
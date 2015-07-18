using System;

namespace Doomnet
{
    internal class Vertex
    {
        public short X;
        public short Y;

        public override string ToString()
        {
            return String.Format("{0}:{1}", X, Y);
        }
    }
}
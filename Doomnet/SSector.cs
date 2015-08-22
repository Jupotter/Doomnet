using System.Collections.Generic;

namespace Doomnet
{
    public class SSector
    {
        short size, offset;
        
        public List<Segment> Segments { get; private set; }

        public SSector(short size, short offset, List<Segment> source)
        {
            Segments = source.GetRange(offset, size);
            this.offset = offset;
            this.size = size;
        }
    }
}
using System;
using System.IO;
using System.Text;

namespace Doomnet
{
    internal class Header
    {
        private string type;
        private Int32 lumps;
        private Int32 offset;

        public string Type
        {
            get { return type; }
        }

        public int Lumps
        {
            get { return lumps; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Type: {0}\n", type)
                .AppendFormat("Lumps: {0}\n", lumps)
                .AppendFormat("Offset: {0}\n", offset);
            return builder.ToString();
        }

        public void Read(Stream stream)
        {
            var buffer = new byte[12];
            stream.Read(buffer, 0, 12);

            type = Encoding.ASCII.GetString(buffer, 0, 4);
            lumps = BitConverter.ToInt32(buffer, 4);
            offset = BitConverter.ToInt32(buffer, 8);
        }
    }
}
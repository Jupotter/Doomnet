namespace Doomnet
{
    public class Node
    {
        public short StartX, StartY, EndX, EndY;
        public short LeftTopX, LeftTopY, LeftBotX, LeftBotY;
        public short RightTopX, RightTopY, RightBotX, RightBotY;
        public ushort LeftNum;
        public ushort RightNum;

        public Node RightNode = null;
        public Node LeftNode = null;

        public SSector RightSector = null;
        public SSector LeftSector = null;

        public Node()
        {
        }
    }
}
namespace SharpCraft.gui
{
    internal class FontMapCharacterNode
    {
        public readonly char Char;
        public readonly int X;
        public readonly int Y;

        public readonly int W;
        public readonly int H;

        public readonly int OffsetX;
        public readonly int OffsetY;

        public FontMapCharacterNode(char c, int x, int y, int w, int h, int oX, int oY)
        {
            Char = c;
            X = x;
            Y = y;
            W = w;
            H = h;

            OffsetX = oX;
            OffsetY = oY;
        }
    }
}
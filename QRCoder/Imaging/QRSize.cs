namespace QRCoder
{
    /// <summary>
    /// Width and height of a rendered QR code graphic.
    /// </summary>
    public readonly struct QRSize
    {
        public int Width { get; }
        public int Height { get; }

        public QRSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString() => $"{Width}x{Height}";
    }
}

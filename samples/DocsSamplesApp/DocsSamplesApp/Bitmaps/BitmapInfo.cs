namespace DocsSamplesApp.Bitmaps
{
    class BitmapInfo
    {
        public BitmapInfo(int pixelWidth, int pixelHeight, int[] iterationCounts)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            IterationCounts = iterationCounts;
        }

        public int PixelWidth { private set; get; }

        public int PixelHeight { private set; get; }

        public int[] IterationCounts { private set; get; }
    }
}

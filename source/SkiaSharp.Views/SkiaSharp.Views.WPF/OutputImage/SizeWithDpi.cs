namespace SkiaSharp.Views.WPF.OutputImage
{
    internal struct SizeWithDpi
    {
		public static SizeWithDpi Empty = new SizeWithDpi(0, 0);

	    public int Width;
	    public int Height;
	    public double DpiX;
	    public double DpiY;

	    public SizeWithDpi(int width, int height, double dpiX = 96.0, double dpiY = 96.0)
	    {
		    Width = width;
		    Height = height;
		    DpiX = dpiX;
		    DpiY = dpiY;
	    }

        public override bool Equals(object obj)
        {
            return obj is SizeWithDpi dpi &&
                   Width == dpi.Width &&
                   Height == dpi.Height &&
                   DpiX == dpi.DpiX &&
                   DpiY == dpi.DpiY;
        }

        public bool Equals(SizeWithDpi dpi)
        {
	        return Width == dpi.Width &&
	               Height == dpi.Height &&
	               DpiX == dpi.DpiX &&
	               DpiY == dpi.DpiY;
        }
	}
}

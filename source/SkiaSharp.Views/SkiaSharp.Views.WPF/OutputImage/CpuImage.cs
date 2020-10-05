using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkiaSharp.Views.WPF.OutputImage
{
    internal class CpuImage : IOutputImage
	{
		private static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(2000));
		private WriteableBitmap image;
		private Int32Rect rect;
		private SKColorType colorType;

		public SizeWithDpi Size { get; private set; }

		public CpuImage(SizeWithDpi size, SKColorType colorType)
		{
			this.colorType = colorType;
			TryResize(size);
		}

	    public ImageSource Source => image;

	    public SKSurface CreateSurface() => SKSurface.Create(new SKImageInfo(Size.Width, Size.Height, colorType), image.BackBuffer);

	    public bool TryLock() => image.TryLock(LockTimeout);

	    public void Unlock()
	    {
		    image.AddDirtyRect(rect);
		    image.Unlock();
	    }

		public void TryResize(SizeWithDpi size)
		{
			if (Size.Equals(size))
			{
				return;
			}

			Size = size;
			image = new WriteableBitmap(size.Width, size.Height, size.DpiX, size.DpiY, PixelFormats.Pbgra32, null);
			rect = new Int32Rect(0, 0, size.Width, size.Height);
		}
	}
}

using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	public class SKLockedSurface
	{
		private readonly Canvas canvas;
		private readonly Bitmap bitmap;

		internal SKLockedSurface(Canvas canvas, Bitmap bitmap)
		{
			this.bitmap = bitmap;
			this.canvas = canvas;

			// create a surface
			ImageInfo = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			Surface = SKSurface.Create(ImageInfo, bitmap.LockPixels(), ImageInfo.RowBytes);
		}

		public SKImageInfo ImageInfo { get; private set; }

		public SKSurface Surface { get; private set; }

		public SKCanvas Canvas => Surface.Canvas;

		internal Canvas Post()
		{
			// dispose our canvas
			Surface.Canvas.Flush();
			Surface.Dispose();

			// unlock the bitmap data and write to canvas
			bitmap.UnlockPixels();
			canvas.DrawBitmap(bitmap, 0, 0, null);

			return canvas;
		}
	}
}

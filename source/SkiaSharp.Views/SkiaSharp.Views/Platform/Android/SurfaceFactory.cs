using System;
using Android.Graphics;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#else
namespace SkiaSharp.Views.Android
#endif
{
	internal class SurfaceFactory : IDisposable
	{
		private Bitmap bitmap;

		public SKImageInfo Info { get; private set; }

		public void UpdateCanvasSize(int w, int h, float density = 1f)
		{
			if (density != 1)
				Info = CreateInfo((int)(w / density), (int)(h / density));
			else
				Info = CreateInfo(w, h);

			// if there are no pixels, clean up
			if (Info.Width == 0 || Info.Height == 0)
				FreeBitmap();
		}

		public SKSurface CreateSurface(out SKImageInfo info)
		{
			// get context details
			info = Info;

			// if there are no pixels, clean up and return
			if (info.Width == 0 || info.Height == 0)
			{
				Dispose();
				return null;
			}

			// if the memory size has changed, then reset the underlying memory
			if (bitmap?.Handle == IntPtr.Zero || bitmap?.Width != info.Width || bitmap?.Height != info.Height)
				FreeBitmap();

			// create the bitmap data if we need it
			if (bitmap == null)
				bitmap = Bitmap.CreateBitmap(info.Width, info.Height, Bitmap.Config.Argb8888);

			return SKSurface.Create(info, bitmap.LockPixels(), info.RowBytes);
		}

		public void DrawSurface(SKSurface surface, Canvas canvas)
		{
			// clean up skia objects
			surface.Canvas.Flush();
			surface.Dispose();

			// get the bitmap ready for drawing
			bitmap.UnlockPixels();

			// get the bounds
			var src = new Rect(0, 0, Info.Width, Info.Height);
			var dst = new RectF(0, 0, canvas.Width, canvas.Height);

			// draw bitmap to the view canvas
			canvas.DrawBitmap(bitmap, src, dst, null);
		}

		public void Dispose()
		{
			FreeBitmap();
			Info = CreateInfo(0, 0);
		}

		public void FreeBitmap()
		{
			if (bitmap == null)
				return;

			// free and recycle the bitmap data
			if (bitmap.Handle != IntPtr.Zero && !bitmap.IsRecycled)
				bitmap.Recycle();

			bitmap.Dispose();
			bitmap = null;
		}

		private SKImageInfo CreateInfo(int width, int height) =>
			new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
	}
}

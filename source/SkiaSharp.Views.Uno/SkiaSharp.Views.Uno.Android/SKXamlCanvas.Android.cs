using System;
using Android.Graphics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private Bitmap bitmap;
		private SKImageInfo info;

		public SKXamlCanvas()
		{
			Initialize();
			SetWillNotDraw(false);
		}

		partial void DoUnloaded() =>
			FreeBitmap();

		private SKSize GetCanvasSize() =>
			info.Size;

		private void DoInvalidate()
		{
			UpdateCanvasSize((int)ActualWidth, (int)ActualHeight);
			base.Invalidate();
		}

		private void UpdateCanvasSize(int w, int h)
		{
			if (designMode)
				return;

			if (!IgnorePixelScaling)
			{
				var display = DisplayInformation.GetForCurrentView();
				var scale = display.LogicalDpi / 96.0f;

				info = new SKImageInfo((int)(w * scale), (int)(h * scale), SKColorType.Rgba8888, SKAlphaType.Premul);
			}
			else
			{
				info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (designMode)
				return;

			if (info.Width == 0 || info.Height == 0 || Visibility != Visibility.Visible || !isVisible)
			{
				FreeBitmap();
				return;
			}

			// create the bitmap data if we need it
			if (bitmap == null || bitmap.Handle == IntPtr.Zero || bitmap.Width != info.Width || bitmap.Height != info.Height)
			{
				FreeBitmap();
				bitmap = Bitmap.CreateBitmap(info.Width, info.Height, Bitmap.Config.Argb8888);
			}

			// create a surface
			using (var surface = SKSurface.Create(info, bitmap.LockPixels(), info.RowBytes))
			{
				// draw using SkiaSharp
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

				surface.Canvas.Flush();
			}
			bitmap.UnlockPixels();

			// draw bitmap to canvas
			if (IgnorePixelScaling)
			{
				var src = new Rect(0, 0, info.Rect.Width, info.Rect.Height);
				var dst = new RectF(0, 0, (float)Width, (float)Height);
				canvas.DrawBitmap(bitmap, src, dst, null);
			}
			else
			{
				canvas.DrawBitmap(bitmap, 0, 0, null);
			}
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				// free and recycle the bitmap data
				if (bitmap.Handle != IntPtr.Zero && !bitmap.IsRecycled)
					bitmap.Recycle();
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}

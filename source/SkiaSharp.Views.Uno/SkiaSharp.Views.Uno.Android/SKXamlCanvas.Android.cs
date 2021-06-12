using Android.Graphics;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas
	{
		private SurfaceFactory surfaceFactory;

		public SKXamlCanvas()
		{
			surfaceFactory = new SurfaceFactory();
			Initialize();
			SetWillNotDraw(false);
		}

		partial void DoUnloaded() =>
			surfaceFactory.Dispose();

		private SKSize GetCanvasSize() =>
			surfaceFactory.Info.Size;

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

				surfaceFactory.UpdateCanvasSize((int)(w * scale), (int)(h * scale));
			}
			else
			{
				surfaceFactory.UpdateCanvasSize(w, h);
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (designMode)
				return;

			// bail out if the view is not actually visible
			if (Visibility != Visibility.Visible || !isVisible)
			{
				surfaceFactory.FreeBitmap();
				return;
			}

			// create a skia surface
			var surface = surfaceFactory.CreateSurface(out var info);
			if (surface == null)
				return;

			// draw using SkiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

			// draw the surface to the view
			surfaceFactory.DrawSurface(surface, canvas);
		}
	}
}

using Android.Graphics;
using Windows.Graphics.Display;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
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

		private void DoInvalidate()
		{
			surfaceFactory.UpdateCanvasSize((int)(ActualWidth * Dpi), (int)(ActualHeight * Dpi));
			base.Invalidate();
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
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var userVisibleSize = IgnorePixelScaling
				? new SKSizeI((int)ActualWidth, (int)ActualHeight)
				: info.Size;
			CanvasSize = userVisibleSize;

			if (IgnorePixelScaling)
			{
				var skiaCanvas = surface.Canvas;
				skiaCanvas.Scale((float)Dpi);
				skiaCanvas.Save();
			}

			// draw using SkiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

			// draw the surface to the view
			surfaceFactory.DrawSurface(surface, canvas);
		}
	}
}

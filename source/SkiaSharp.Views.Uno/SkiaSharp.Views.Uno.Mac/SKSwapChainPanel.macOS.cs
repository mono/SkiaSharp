using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		private SKGLView glView;

		public SKSwapChainPanel()
		{
			Initialize();
		}

		private SKSize GetCanvasSize() =>
			glView?.CanvasSize ?? SKSize.Empty;

		private GRContext GetGRContext() =>
			glView?.GRContext;

		partial void DoLoaded()
		{
			glView = new SKGLView(Bounds);
			glView.PaintSurface += OnPaintSurface;
			AddSubview(glView);
		}

		partial void DoUnloaded()
		{
			if (glView != null)
			{
				glView.RemoveFromSuperview();
				glView.PaintSurface -= OnPaintSurface;
				glView.Dispose();
				glView = null;
			}
		}

		private void DoInvalidate() =>
			glView?.Display();

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);
	}
}

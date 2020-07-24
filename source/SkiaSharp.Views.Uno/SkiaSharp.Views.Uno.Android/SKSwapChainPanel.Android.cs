using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		private SKGLTextureView glTextureView;

		public SKSwapChainPanel()
		{
			Initialize();
		}

		private SKSize GetCanvasSize() =>
			glTextureView?.CanvasSize ?? SKSize.Empty;

		private GRContext GetGRContext() =>
			glTextureView?.GRContext;

		partial void DoLoaded()
		{
			glTextureView = new SKGLTextureView(Context);
			glTextureView.PaintSurface += OnPaintSurface;
			AddView(glTextureView);
		}

		partial void DoUnloaded()
		{
			if (glTextureView != null)
			{
				RemoveView(glTextureView);
				glTextureView.PaintSurface -= OnPaintSurface;
				glTextureView.Dispose();
				glTextureView = null;
			}
		}

		private void DoInvalidate() =>
			glTextureView?.RequestRender();

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);
	}
}

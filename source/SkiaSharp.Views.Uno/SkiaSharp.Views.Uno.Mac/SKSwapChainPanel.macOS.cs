using CoreVideo;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		private SKGLView glView;
		private CVDisplayLink displayLink;

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
			DoEnableRenderLoop(false);

			if (glView != null)
			{
				glView.RemoveFromSuperview();
				glView.PaintSurface -= OnPaintSurface;
				glView.Dispose();
				glView = null;
			}
		}

		private void DoInvalidate() =>
			DoEnableRenderLoop(true);

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);

		partial void DoEnableRenderLoop(bool enable)
		{
			// stop the render loop
			if (!enable)
			{
				if (displayLink != null)
				{
					displayLink.Stop();
					displayLink.Dispose();
					displayLink = null;
				}
				return;
			}

			// only start if we haven't already
			if (displayLink != null)
				return;

			// create the loop
			displayLink = new CVDisplayLink();
			displayLink.SetOutputCallback(delegate
			{
				// redraw the view
				glView?.BeginInvokeOnMainThread(() =>
				{
					if (glView != null)
						glView.NeedsDisplay = true;
				});

				// stop the render loop if it has been disabled or the views are disposed
				if (glView == null || !EnableRenderLoop)
					DoEnableRenderLoop(false);

				return CVReturn.Success;
			});
			displayLink.Start();
		}
	}
}

#if !__MACCATALYST__
using CoreAnimation;
using Foundation;
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
		private CADisplayLink displayLink;

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
					displayLink.Invalidate();
					displayLink.Dispose();
					displayLink = null;
				}
				return;
			}

			// only start if we haven't already
			if (displayLink != null)
				return;

			// create the loop
			displayLink = CADisplayLink.Create(delegate
			{
				// redraw the view
				glView?.BeginInvokeOnMainThread(() => glView?.Display());

				// stop the render loop if it has been disabled or the views are disposed
				if (glView == null || !EnableRenderLoop)
					DoEnableRenderLoop(false);
			});

#if NET6_0_OR_GREATER
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
#else
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
#endif
		}
	}
}
#endif

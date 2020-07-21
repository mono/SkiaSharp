using System.Windows.Threading;
using Xamarin.Forms.Platform.WPF;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Forms.SKHostedGLControl;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		private DispatcherTimer renderTimer;

		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKGLViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();

		protected override void SetupRenderLoop(bool oneShot)
		{
			// only start if we haven't already
			if (renderTimer != null)
				return;

			// bail out if we are requesting something that the view doesn't want to
			if (!oneShot && !Element.HasRenderLoop)
				return;

			// if this is a one shot request, don't bother with the timer
			if (oneShot)
			{
				Control.Invalidate();
				return;
			}

			// create the render loop at 60 fps
			renderTimer = new DispatcherTimer(DispatcherPriority.Render);
			renderTimer.Tick += OnRenderTick;
			renderTimer.Interval = TimeSpan.FromMilliseconds(16);
			renderTimer.Start();
		}

		protected override void Dispose(bool disposing)
		{
			// stop the render loop
			if (renderTimer != null)
			{
				renderTimer.Tick -= OnRenderTick;
				renderTimer = null;
			}

			base.Dispose(disposing);
		}

		private void OnRenderTick(object sender, EventArgs e)
		{
			var nativeView = Control;
			var formsView = Element;

			// redraw the view
			nativeView?.Invalidate();

			// stop the render loop if this was a one-shot, or the views are disposed
			if (nativeView == null || formsView == null || !formsView.HasRenderLoop)
			{
				renderTimer.Tick -= OnRenderTick;
				renderTimer.Stop();
				renderTimer = null;
			}
		}
	}
}

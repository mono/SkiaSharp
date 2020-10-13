namespace SkiaSharp.Views.Forms
{
	internal class SKGLViewRenderer
	{
		public SKGLViewRenderer()
		{
			throw new System.PlatformNotSupportedException("SKGLView is not yet supported on GTK.");
		}
	}
}

/*
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Forms.SKGLWidget;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		private uint renderTimer;

		protected override void SetupRenderLoop(bool oneShot)
		{
			// only start if we haven't already
			if (renderTimer != 0)
				return;

			// bail out if we are requesting something that the view doesn't want to
			if (!oneShot && !Element.HasRenderLoop)
				return;

			// if this is a one shot request, don't bother with the timer
			if (oneShot)
			{
				Control.QueueDraw();
				return;
			}

			// create the render loop at 60 fps
			renderTimer = GLib.Timeout.Add(16, new GLib.TimeoutHandler(OnRenderTick));
		}

		protected override void Dispose(bool disposing)
		{
			// stop the render loop
			if (renderTimer != 0)
			{
				GLib.Source.Remove(renderTimer);
				renderTimer = 0;
			}

			base.Dispose(disposing);
		}

		private bool OnRenderTick()
		{
			var nativeView = Control;
			var formsView = Element;

			// redraw the view
			nativeView?.QueueDraw();

			// stop the render loop if this was a one-shot, or the views are disposed
			var done = nativeView == null || formsView == null || !formsView.HasRenderLoop;

			if (done)
				renderTimer = 0;

			return !done;
		}
	}
}
*/

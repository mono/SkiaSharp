using CoreVideo;
using Foundation;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.Mac.SKGLView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		private CVDisplayLink displayLink;

		protected override SKNativeView CreateNativeControl()
		{
			var view = base.CreateNativeControl();
			return view;
		}

		protected override void Dispose(bool disposing)
		{
			// stop the render loop
			if (displayLink != null)
			{
				displayLink.Stop();
				displayLink.Dispose();
				displayLink = null;
			}

			base.Dispose(disposing);
		}

		protected override void SetupRenderLoop(bool oneShot)
		{
			// only start if we haven't already
			if (displayLink != null)
				return;

			// bail out if we are requesting something that the view doesn't want to
			if (!oneShot && !Element.HasRenderLoop)
				return;

			// create the loop
			displayLink = new CVDisplayLink();
			displayLink.SetOutputCallback(delegate
			{
				var formsView = Control;
				var nativeView = Element;

				// redraw the view
				formsView?.Display();

				// stop the render loop if this was a one-shot, or the views are disposed
				if (formsView == null || nativeView == null || !nativeView.HasRenderLoop)
				{
					displayLink.Stop();
					displayLink.Dispose();
					displayLink = null;
				}

				return CVReturn.Success;
			});
			displayLink.Start();
		}
	}
}

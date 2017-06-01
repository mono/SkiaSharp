using CoreAnimation;
using Foundation;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		private CADisplayLink displayLink;

		protected override SKNativeView CreateNativeControl()
		{
			var view = base.CreateNativeControl();

			// Force the opacity to false for consistency with the other platforms
			view.Opaque = false;

			return view;
		}

		protected override void Dispose(bool disposing)
		{
			// stop the render loop
			if (displayLink != null)
			{
				displayLink.Invalidate();
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
			displayLink = CADisplayLink.Create(() =>
			{
				var formsView = Control;
				var nativeView = Element;

				// redraw the view
				formsView?.Display();

				// stop the render loop if this was a one-shot, or the views are disposed
				if (formsView == null || nativeView == null || !nativeView.HasRenderLoop)
				{
					displayLink.Invalidate();
					displayLink.Dispose();
					displayLink = null;
				}
			});
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
		}
	}
}

#if !__MACCATALYST__
using System;
using CoreAnimation;
using Foundation;

#if __MAUI__
using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;
#else
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public class SKGLViewRenderer : SKGLViewRendererBase<SKFormsView, SKNativeView>
	{
		private CADisplayLink displayLink;

		public SKGLViewRenderer()
		{
			SetDisablesUserInteraction(true);
		}

		protected override SKNativeView CreateNativeControl()
		{
			var view = GetType() == typeof(SKGLViewRenderer)
				? new SKNativeView()
				: base.CreateNativeControl();

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

			// if this is a one shot request, don't bother with the display link
			if (oneShot)
			{
				var nativeView = Control;
				nativeView?.BeginInvokeOnMainThread(() =>
				{
					if (nativeView.Handle != IntPtr.Zero)
						nativeView.Display();
				});
				return;
			}

			// create the loop
			displayLink = CADisplayLink.Create(() =>
			{
				var nativeView = Control;
				var formsView = Element;

				// stop the render loop if this was a one-shot, or the views are disposed
				if (nativeView == null || formsView == null || nativeView.Handle == IntPtr.Zero || !formsView.HasRenderLoop)
				{
					displayLink.Invalidate();
					displayLink.Dispose();
					displayLink = null;
					return;
				}

				// redraw the view
				nativeView.Display();
			});
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
		}
	}
}
#endif

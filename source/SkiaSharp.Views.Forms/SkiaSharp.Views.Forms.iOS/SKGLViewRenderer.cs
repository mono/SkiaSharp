using System;
using System.ComponentModel;
using CoreAnimation;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKGLViewRenderer : ViewRenderer<SKFormsView, SKNativeView>
	{
		private CADisplayLink displayLink;

		protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKGLViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
				oldController.GetCanvasSize -= OnGetCanvasSize;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKGLViewController)e.NewElement;

				// create the native view
				var view = new InternalView(newController);
				SetNativeControl(view);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;
				newController.GetCanvasSize += OnGetCanvasSize;

				// start the rendering
				SetupRenderLoop(false);
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			// refresh the render loop
			if (e.PropertyName == SKFormsView.HasRenderLoopProperty.PropertyName)
			{
				SetupRenderLoop(false);
			}
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

			// detach all events before disposing
			var controller = (ISKGLViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			base.Dispose(disposing);
		}

		private void SetupRenderLoop(bool oneShot)
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

		// the user asked to repaint
		private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
		{
			// if we aren't in a loop, then refresh once
			if (!Element.HasRenderLoop)
			{
				SetupRenderLoop(true);
			}
		}

		// the user asked for the size
		private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
		{
			e.CanvasSize = Control?.CanvasSize ?? SKSize.Empty;
		}

		private class InternalView : SKNativeView
		{
			private readonly ISKGLViewController controller;

			public InternalView(ISKGLViewController controller)
			{
				UserInteractionEnabled = false;

				this.controller = controller;

				// Force the opacity to false for consistency with the other platforms
				Opaque = false;
			}

			public override void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				base.DrawInSurface(surface, renderTarget);

				// the control is being repainted, let the user know
				controller.OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));
			}
		}
	}
}

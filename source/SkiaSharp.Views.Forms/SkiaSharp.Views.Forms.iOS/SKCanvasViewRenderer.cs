using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.iOS.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	internal class SKCanvasViewRenderer : ViewRenderer<SKFormsView, SKNativeView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKCanvasViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKCanvasViewController)e.NewElement;

				// create the native view
				var view = new InternalView(newController);
				SetNativeControl(view);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;

				// paint for the first time
				Control.SetNeedsDisplay();
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKCanvasViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			base.Dispose(disposing);
		}

		private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
		{
			// repaint the native control
			Control.SetNeedsDisplay();
		}

		private class InternalView : SKNativeView
		{
			private readonly ISKCanvasViewController controller;

			public InternalView(ISKCanvasViewController controller)
			{
				UserInteractionEnabled = false;

				this.controller = controller;
			}

			public override void DrawInSurface(SKSurface surface, SKImageInfo info)
			{
				base.DrawInSurface(surface, info);

				// the control is being repainted, let the user know
				controller.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}
		}
	}
}

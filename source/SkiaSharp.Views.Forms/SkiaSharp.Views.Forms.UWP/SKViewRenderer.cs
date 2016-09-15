using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using SKFormsView = SkiaSharp.Views.Forms.SKView;
using SKNativeView = SkiaSharp.Views.SKXamlCanvas;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	internal class SKViewRenderer : ViewRenderer<SKFormsView, SKNativeView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKViewController)e.NewElement;

				// create the native view
				var view = new InternalView(newController);
				SetNativeControl(view);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;

				// paint for the first time
				Control.Invalidate();
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			base.Dispose(disposing);
		}

		private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
		{
			// repaint the native control
			Control.Invalidate();
		}

		private class InternalView : SKNativeView
		{
			private readonly ISKViewController controller;

			public InternalView(ISKViewController controller)
			{
				this.controller = controller;
			}

			protected override void OnPaintSurface(SkiaSharp.Views.SKPaintSurfaceEventArgs e)
			{
				base.OnPaintSurface(e);

				// the control is being repainted, let the user know
				controller.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
			}
		}
	}
}

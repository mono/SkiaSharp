using System;
using System.ComponentModel;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using SKNativeView = SkiaSharp.Views.Android.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Android.SKPaintSurfaceEventArgs;
#elif __IOS__
using Xamarin.Forms.Platform.iOS;
using SKNativeView = SkiaSharp.Views.iOS.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs;
#elif WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
using SKNativeView = SkiaSharp.Views.UWP.SKXamlCanvas;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs;
#endif

namespace SkiaSharp.Views.Forms
{
	public abstract class SKCanvasViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
		where TFormsView : SKFormsView
		where TNativeView : SKNativeView
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKCanvasViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
				oldController.GetCanvasSize -= OnGetCanvasSize;
			}

			if (Control != null)
			{
				var control = Control;
				control.PaintSurface -= OnPaintSurface;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKCanvasViewController)e.NewElement;

				// create the native view
				var view = CreateNativeControl();
				view.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
				view.PaintSurface += OnPaintSurface;
				SetNativeControl(view);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;
				newController.GetCanvasSize += OnGetCanvasSize;

				// paint for the first time
				OnSurfaceInvalidated(newController, EventArgs.Empty);
			}

			base.OnElementChanged(e);
		}
		
#if __ANDROID__
		protected override TNativeView CreateNativeControl()
		{
			return (TNativeView)Activator.CreateInstance(typeof(TNativeView), new[] { Context });
		}
#else
		protected virtual TNativeView CreateNativeControl()
		{
			return (TNativeView)Activator.CreateInstance(typeof(TNativeView));
		}
#endif

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(SKFormsView.IgnorePixelScaling))
			{
				Control.IgnorePixelScaling = Element.IgnorePixelScaling;
			}
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKCanvasViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
				controller.GetCanvasSize -= OnGetCanvasSize;
			}

			var control = Control;
			if (control != null)
			{
				control.PaintSurface -= OnPaintSurface;
			}

			base.Dispose(disposing);
		}

		private void OnPaintSurface(object sender, SKNativePaintSurfaceEventArgs e)
		{
			var controller = Element as ISKCanvasViewController;

			// the control is being repainted, let the user know
			controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info));
		}

		private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
		{
			// repaint the native control
#if __IOS__
			Control.SetNeedsDisplay();
#else
			Control.Invalidate();
#endif
		}

		// the user asked for the size
		private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
		{
			e.CanvasSize = Control?.CanvasSize ?? SKSize.Empty;
		}
	}
}

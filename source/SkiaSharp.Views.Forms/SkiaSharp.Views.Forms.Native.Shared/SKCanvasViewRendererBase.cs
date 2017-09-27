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
using Windows.Graphics.Display;
using Xamarin.Forms.Platform.UWP;
using SKNativeView = SkiaSharp.Views.UWP.SKXamlCanvas;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs;
#elif __MACOS__
using Xamarin.Forms.Platform.MacOS;
using SKNativeView = SkiaSharp.Views.Mac.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Mac.SKPaintSurfaceEventArgs;
#endif

namespace SkiaSharp.Views.Forms
{
	public abstract class SKCanvasViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
		where TFormsView : SKFormsView
		where TNativeView : SKNativeView
	{
		private readonly SKTouchHandler touchHandler;

		public SKCanvasViewRendererBase()
		{
#if __ANDROID__
			touchHandler = new SKTouchHandler(
				args => ((ISKCanvasViewController)Element).OnTouch(args),
				coord => Element.IgnorePixelScaling ? (float)Context.FromPixels(coord) : coord);
#elif __IOS__
			touchHandler = new SKTouchHandler(
				args => ((ISKCanvasViewController)Element).OnTouch(args),
				coord => Element.IgnorePixelScaling ? coord : coord * Control.ContentScaleFactor);
#elif __MACOS__
			touchHandler = new SKTouchHandler(
				args => ((ISKCanvasViewController)Element).OnTouch(args),
				coord => Element.IgnorePixelScaling ? coord : coord * Control.Window.BackingScaleFactor);
#elif WINDOWS_UWP
			touchHandler = new SKTouchHandler(
				args => ((ISKCanvasViewController)Element).OnTouch(args),
				coord => Element.IgnorePixelScaling ? coord : (float)(coord * Control.Dpi));
#endif
		}

#if __IOS__
		protected void SetDisablesUserInteraction(bool disablesUserInteraction)
		{
			touchHandler.DisablesUserInteraction = disablesUserInteraction;
		}
#endif

		protected override void OnElementChanged(ElementChangedEventArgs<TFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKCanvasViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
				oldController.GetCanvasSize -= OnGetCanvasSize;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKCanvasViewController)e.NewElement;

				// create the native view
				if (Control == null)
				{
					var view = CreateNativeControl();
					view.PaintSurface += OnPaintSurface;
					SetNativeControl(view);
				}

				// set the initial values
				touchHandler.SetEnabled(Control, e.NewElement.EnableTouchEvents);
				Control.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;

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

			if (e.PropertyName == SKFormsView.IgnorePixelScalingProperty.PropertyName)
			{
				Control.IgnorePixelScaling = Element.IgnorePixelScaling;
			}
			else if (e.PropertyName == SKFormsView.EnableTouchEventsProperty.PropertyName)
			{
				touchHandler.SetEnabled(Control, Element.EnableTouchEvents);
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

			// detach, regardless of state
			touchHandler.Detach(control);

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
#elif __MACOS__
			Control.NeedsDisplay = true;
#else
			Control.Invalidate();
#endif
		}

		// the user asked for the size
		private void OnGetCanvasSize(object sender, GetPropertyValueEventArgs<SKSize> e)
		{
			e.Value = Control?.CanvasSize ?? SKSize.Empty;
		}
	}
}

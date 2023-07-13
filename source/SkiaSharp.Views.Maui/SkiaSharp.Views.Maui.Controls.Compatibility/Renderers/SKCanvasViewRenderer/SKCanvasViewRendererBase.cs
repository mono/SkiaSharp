#if !NETSTANDARD
using System;
using System.ComponentModel;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using SkiaSharp.Views.Maui.Platform;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;

#if __ANDROID__
using Android.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using SKNativeView = SkiaSharp.Views.Android.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Android.SKPaintSurfaceEventArgs;
#elif __IOS__
using Microsoft.Maui.Controls.Handlers.Compatibility;
using SKNativeView = SkiaSharp.Views.iOS.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.iOS.SKPaintSurfaceEventArgs;
#elif WINDOWS
using Windows.Graphics.Display;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using SKNativeView = SkiaSharp.Views.Windows.SKXamlCanvas;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs;
using WVisibility = Microsoft.UI.Xaml.Visibility;
#elif __TIZEN__
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using SKNativeView = SkiaSharp.Views.Tizen.NUI.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs;
#endif

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	[Obsolete("View renderers are obsolete in .NET MAUI. Use the handlers instead.")]
	public abstract class SKCanvasViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
		where TFormsView : SKFormsView
		where TNativeView : SKNativeView
	{
		private SKTouchHandler touchHandler;

#if __ANDROID__
		protected SKCanvasViewRendererBase(Context context)
			: base(context)
		{
			Initialize();
		}
#else
		protected SKCanvasViewRendererBase()
		{
			Initialize();
		}
#endif

		private void Initialize()
		{
			touchHandler = new SKTouchHandler(
				args => ((ISKCanvasViewController)Element).OnTouch(args),
				(x, y) => GetScaledCoord(x, y));
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

				// TODO: implement this if it is actually supported
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
#elif __TIZEN__
		protected virtual TNativeView CreateNativeControl()
		{
			TNativeView ret = (TNativeView)Activator.CreateInstance(typeof(TNativeView));
			return ret;
		}
#elif __IOS__
		protected override TNativeView CreateNativeControl()
		{
			return (TNativeView)Activator.CreateInstance(typeof(TNativeView));
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
				// TODO: implement this if it is actually supported
				Control.IgnorePixelScaling = Element.IgnorePixelScaling;
			}
			else if (e.PropertyName == SKFormsView.EnableTouchEventsProperty.PropertyName)
			{
				touchHandler.SetEnabled(Control, Element.EnableTouchEvents);
			}
#if WINDOWS
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
			{
				// pass the visibility down to the view do disable drawing
				Control.Visibility = Element.IsVisible
					? WVisibility.Visible
					: WVisibility.Collapsed;
			}
#endif
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

		private SKPoint GetScaledCoord(double x, double y)
		{
			if (Element.IgnorePixelScaling)
			{
#if __ANDROID__
				x = Context.FromPixels(x);
				y = Context.FromPixels(y);
#elif __TIZEN__
				x = Tizen.ScalingInfo.FromPixel(x);
				y = Tizen.ScalingInfo.FromPixel(y);
#elif __IOS__ || WINDOWS
				// Tizen and Android are the reverse of the other platforms
#else
#error Missing platform logic
#endif
			}
			else
			{
#if __ANDROID__ || __TIZEN__
				// Tizen and Android are the reverse of the other platforms
#elif __IOS__
				x = x * Control.ContentScaleFactor;
				y = y * Control.ContentScaleFactor;
#elif WINDOWS
				x = x * Control.Dpi;
				y = y * Control.Dpi;
#else
#error Missing platform logic
#endif
			}

			return new SKPoint((float)x, (float)y);
		}

		private void OnPaintSurface(object sender, SKNativePaintSurfaceEventArgs e)
		{
			var controller = Element as ISKCanvasViewController;

			// the control is being repainted, let the user know
			controller?.OnPaintSurface(new SKPaintSurfaceEventArgs(e.Surface, e.Info, e.RawInfo));
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
		private void OnGetCanvasSize(object sender, GetPropertyValueEventArgs<SKSize> e)
		{
			e.Value = Control?.CanvasSize ?? SKSize.Empty;
		}
	}
}
#endif

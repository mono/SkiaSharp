#if !NETSTANDARD
using System;
using System.ComponentModel;

#if __MAUI__

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
#endif

#else

using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;

#if __ANDROID__
using Android.Content;
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
using WVisibility = Windows.UI.Xaml.Visibility;
#elif __MACOS__
using Xamarin.Forms.Platform.MacOS;
using SKNativeView = SkiaSharp.Views.Mac.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Mac.SKPaintSurfaceEventArgs;
#elif __TIZEN__
using Xamarin.Forms.Platform.Tizen;
using SKNativeView = SkiaSharp.Views.Tizen.SKCanvasView;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs;
using TForms = Xamarin.Forms.Forms;
#elif __WPF__
using System.Windows;
using Xamarin.Forms.Platform.WPF;
using SKNativeView = SkiaSharp.Views.WPF.SKElement;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs;
#elif __GTK__
using Xamarin.Forms.Platform.GTK;
using SKNativeView = SkiaSharp.Views.Gtk.SKWidget;
using SKNativePaintSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs;
#endif

#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
{
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
#if !__GTK__
				// TODO: implement this if it is actually supported
				Control.IgnorePixelScaling = e.NewElement.IgnorePixelScaling;
#endif

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
			TNativeView ret = (TNativeView)Activator.CreateInstance(typeof(TNativeView), new[] { TForms.NativeParent });
			return ret;
		}
#elif __IOS__ || __MACOS__
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
#if !__GTK__
				// TODO: implement this if it is actually supported
				Control.IgnorePixelScaling = Element.IgnorePixelScaling;
#endif
			}
			else if (e.PropertyName == SKFormsView.EnableTouchEventsProperty.PropertyName)
			{
				touchHandler.SetEnabled(Control, Element.EnableTouchEvents);
			}
#if WINDOWS_UWP || WINDOWS
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
#elif __IOS__ || __MACOS__ || WINDOWS_UWP || __WPF__ || WINDOWS
				// Tizen and Android are the reverse of the other platforms
#elif __GTK__
				// TODO: implement this if it is actually supported
				// GTK does not yet support IgnorePixelScaling
#else
#error Missing platform logic
#endif
			}
			else
			{
#if __ANDROID__ || __TIZEN__
				// Tizen and Android are the reverse of the other platforms
#elif __GTK__
				// TODO: implement this if it is actually supported
				// GTK does not yet support IgnorePixelScaling
#elif __IOS__
				x = x * Control.ContentScaleFactor;
				y = y * Control.ContentScaleFactor;
#elif __MACOS__
				x = x * Control.Window.BackingScaleFactor;
				y = y * Control.Window.BackingScaleFactor;
#elif WINDOWS_UWP || WINDOWS
				x = x * Control.Dpi;
				y = y * Control.Dpi;
#elif __WPF__
				var m = PresentationSource.FromVisual(Control).CompositionTarget.TransformToDevice;
				x = x * m.M11;
				y = y * m.M22;
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
#elif __MACOS__
			Control.NeedsDisplay = true;
#elif __WPF__
			Control.InvalidateVisual();
#elif __GTK__
			Control.QueueDraw();
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

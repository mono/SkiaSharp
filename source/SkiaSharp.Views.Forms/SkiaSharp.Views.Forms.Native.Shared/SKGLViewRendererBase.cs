#if !__GTK__ && !WINDOWS && !__MACCATALYST__ && !NETSTANDARD
using System;
using System.ComponentModel;

#if __MAUI__

using Microsoft.Maui.Controls.Platform;
using SkiaSharp.Views.Maui.Platform;

using SKFormsView = SkiaSharp.Views.Maui.Controls.SKGLView;

#if __ANDROID__
using Android.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using SKNativeView = SkiaSharp.Views.Android.SKGLTextureView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs;
#elif __IOS__
using Microsoft.Maui.Controls.Handlers.Compatibility;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs;
#endif

#else

using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;

#if __ANDROID__
using Android.Content;
using Xamarin.Forms.Platform.Android;
using SKNativeView = SkiaSharp.Views.Android.SKGLTextureView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs;
#elif __IOS__
using Xamarin.Forms.Platform.iOS;
using SKNativeView = SkiaSharp.Views.iOS.SKGLView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs;
#elif WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
using SKNativeView = SkiaSharp.Views.UWP.SKSwapChainPanel;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.UWP.SKPaintGLSurfaceEventArgs;
#elif __MACOS__
using Xamarin.Forms.Platform.MacOS;
using SKNativeView = SkiaSharp.Views.Mac.SKGLView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Mac.SKPaintGLSurfaceEventArgs;
#elif __TIZEN__
using Xamarin.Forms.Platform.Tizen;
using SKNativeView = SkiaSharp.Views.Tizen.SKGLSurfaceView;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs;
using TForms = Xamarin.Forms.Forms;
#elif __WPF__
using System.Windows;
using Xamarin.Forms.Platform.WPF;
using SKNativeView = SkiaSharp.Views.Forms.SKHostedGLControl;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs;
#elif __GTK__
using Xamarin.Forms.Platform.GTK;
using SKNativeView = SkiaSharp.Views.Forms.SKGLWidget;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs;
#endif

#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
{
	public abstract class SKGLViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
		where TFormsView : SKFormsView
		where TNativeView : SKNativeView
	{
		private SKTouchHandler touchHandler;

#if __ANDROID__
		protected SKGLViewRendererBase(Context context)
			: base(context)
		{
			Initialize();
		}
#else
		protected SKGLViewRendererBase()
		{
			Initialize();
		}
#endif

		private void Initialize()
		{
			touchHandler = new SKTouchHandler(
				args => ((ISKGLViewController)Element).OnTouch(args),
				(x, y) => GetScaledCoord(x, y));
		}

		public GRContext GRContext => Control.GRContext;

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
				var oldController = (ISKGLViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
				oldController.GetCanvasSize -= OnGetCanvasSize;
				oldController.GetGRContext -= OnGetGRContext;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKGLViewController)e.NewElement;

				// create the native view
				if (Control == null)
				{
					var view = CreateNativeControl();
					view.PaintSurface += OnPaintSurface;
					SetNativeControl(view);
				}

				touchHandler.SetEnabled(Control, e.NewElement.EnableTouchEvents);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;
				newController.GetCanvasSize += OnGetCanvasSize;
				newController.GetGRContext += OnGetGRContext;

				// start the rendering
				SetupRenderLoop(false);
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

			// refresh the render loop
			if (e.PropertyName == SKFormsView.HasRenderLoopProperty.PropertyName)
			{
				SetupRenderLoop(false);
			}
			else if (e.PropertyName == SKFormsView.EnableTouchEventsProperty.PropertyName)
			{
				touchHandler.SetEnabled(Control, Element.EnableTouchEvents);
			}
#if WINDOWS_UWP
			else if (e.PropertyName == Xamarin.Forms.VisualElement.IsVisibleProperty.PropertyName)
			{
				// pass the visibility down to the view do disable drawing
				Control.Visibility = Element.IsVisible
					? Windows.UI.Xaml.Visibility.Visible
					: Windows.UI.Xaml.Visibility.Collapsed;
			}
#endif
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKGLViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
				controller.GetCanvasSize -= OnGetCanvasSize;
				controller.GetGRContext -= OnGetGRContext;
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

		protected abstract void SetupRenderLoop(bool oneShot);

		private SKPoint GetScaledCoord(double x, double y)
		{
#if __ANDROID__ || __TIZEN__
			// Android and Tizen are the reverse of the other platforms
#elif __IOS__
			x = x * Control.ContentScaleFactor;
			y = y * Control.ContentScaleFactor;
#elif __MACOS__
			x = x * Control.Window.BackingScaleFactor;
			y = y * Control.Window.BackingScaleFactor;
#elif WINDOWS_UWP
			x = x * Control.ContentsScale;
			y = y * Control.ContentsScale;
#elif __WPF__
			// TODO: implement this if it is actually supported
			// WPF does not scale for GL as it is using Windows.Forms
#elif __GTK__
			// TODO: implement this if it is actually supported
			// GTK does not yet support IgnorePixelScaling
#else
#error Missing platform logic
#endif

			return new SKPoint((float)x, (float)y);
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
		private void OnGetCanvasSize(object sender, GetPropertyValueEventArgs<SKSize> e)
		{
			e.Value = Control?.CanvasSize ?? SKSize.Empty;
		}

		// the user asked for the current GRContext
		private void OnGetGRContext(object sender, GetPropertyValueEventArgs<GRContext> e)
		{
			e.Value = Control?.GRContext;
		}

		private void OnPaintSurface(object sender, SKNativePaintGLSurfaceEventArgs e)
		{
			var controller = Element as ISKGLViewController;

			// the control is being repainted, let the user know
			controller?.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget));
		}
	}
}
#endif

using System;
using System.ComponentModel;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using SKNativeView = SkiaSharp.Views.Android.SKGLSurfaceView;
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
#endif

namespace SkiaSharp.Views.Forms
{
	public abstract class SKGLViewRendererBase<TFormsView, TNativeView> : ViewRenderer<TFormsView, TNativeView>
		where TFormsView : SKFormsView
		where TNativeView : SKNativeView
	{
		private readonly SKTouchHandler touchHandler;

		public SKGLViewRendererBase()
		{
#if __ANDROID__
			touchHandler = new SKTouchHandler(
				args => ((ISKGLViewController)Element).OnTouch(args),
				coord => coord);
#elif __IOS__
			touchHandler = new SKTouchHandler(
				args => ((ISKGLViewController)Element).OnTouch(args),
				coord => coord * Control.ContentScaleFactor);
#elif __MACOS__
			touchHandler = new SKTouchHandler(
				args => ((ISKGLViewController)Element).OnTouch(args),
				coord => coord * Control.Window.BackingScaleFactor);
#elif WINDOWS_UWP
			touchHandler = new SKTouchHandler(
				args => ((ISKGLViewController)Element).OnTouch(args),
				coord => (float)(coord * Control.ContentsScale));
#endif
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
#if __ANDROID__
					view.SetRenderer(new Renderer(newController));
#else
					view.PaintSurface += OnPaintSurface;
#endif
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
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKGLViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			var control = Control;
			if (control != null)
			{
#if __ANDROID__
				control.SetRenderer(null);
#else
				control.PaintSurface -= OnPaintSurface;
#endif
			}

			// detach, regardless of state
			touchHandler.Detach(control);

			base.Dispose(disposing);
		}

		protected abstract void SetupRenderLoop(bool oneShot);

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
			controller?.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.RenderTarget));
		}

#if __ANDROID__
		private class Renderer : SKNativeView.ISKRenderer
		{
			private readonly ISKGLViewController controller;

			public Renderer(ISKGLViewController controller)
			{
				this.controller = controller;
			}

			public void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
			{
				controller.OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));
			}
		}
#endif
	}
}

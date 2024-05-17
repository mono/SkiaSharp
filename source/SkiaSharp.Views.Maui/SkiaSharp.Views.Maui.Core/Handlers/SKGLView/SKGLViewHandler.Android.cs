using Android.Content;
using Android.Opengl;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKGLTextureView>
	{
		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;
		private SKTouchHandler? touchHandler;

		protected override SKGLTextureView CreatePlatformView()
		{
			var view = new MauiSKGLTextureView(Context);
			view.SetOpaque(false);
			return view;
		}

		protected override void ConnectHandler(SKGLTextureView platformView)
		{
			platformView.PaintSurface += OnPaintSurface;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(SKGLTextureView platformView)
		{
			touchHandler?.Detach(platformView);
			touchHandler = null;

			platformView.PaintSurface -= OnPaintSurface;

			base.DisconnectHandler(platformView);
		}

		// Mapper actions / properties

		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args)
		{
			if (handler.PlatformView.RenderMode == Rendermode.WhenDirty)
				handler.PlatformView.RequestRender();
		}

		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view)
		{
			if (handler.PlatformView is not MauiSKGLTextureView pv)
				return;

			pv.IgnorePixelScaling = view.IgnorePixelScaling;
			pv.RequestRender();
		}

		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view)
		{
			handler.PlatformView.RenderMode = view.HasRenderLoop
				? Rendermode.Continuously
				: Rendermode.WhenDirty;
		}

		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view)
		{
			handler.touchHandler ??= new SKTouchHandler(
				args => view.OnTouch(args),
				(x, y) => handler.OnGetScaledCoord(x, y));

			handler.touchHandler?.SetEnabled(handler.PlatformView, view.EnableTouchEvents);
		}

		// helper methods

		private void OnPaintSurface(object? sender, Android.SKPaintGLSurfaceEventArgs e)
		{
			var newCanvasSize = e.Info.Size;
			if (lastCanvasSize != newCanvasSize)
			{
				lastCanvasSize = newCanvasSize;
				VirtualView?.OnCanvasSizeChanged(newCanvasSize);
			}
			if (sender is SKGLTextureView platformView)
			{
				var newGRContext = platformView.GRContext;
				if (lastGRContext != newGRContext)
				{
					lastGRContext = newGRContext;
					VirtualView?.OnGRContextChanged(newGRContext);
				}
			}

			VirtualView?.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
		}

		private SKPoint OnGetScaledCoord(double x, double y)
		{
			if (VirtualView?.IgnorePixelScaling == true && Context != null)
			{
				x = Context.FromPixels(x);
				y = Context.FromPixels(y);
			}

			return new SKPoint((float)x, (float)y);
		}

		private class MauiSKGLTextureView : SKGLTextureView
		{
			private float density;

			public MauiSKGLTextureView(Context context)
				: base(context)
			{
				density = Resources?.DisplayMetrics?.Density ?? 1;
			}

			public bool IgnorePixelScaling { get; set; }

			protected override void OnPaintSurface(Android.SKPaintGLSurfaceEventArgs e)
			{
				if (IgnorePixelScaling)
				{
					var userVisibleSize = new SKSizeI((int)(e.Info.Width / density), (int)(e.Info.Height / density));
					var canvas = e.Surface.Canvas;
					canvas.Scale(density);
					canvas.Save();

					e = new Android.SKPaintGLSurfaceEventArgs(e.Surface, e.BackendRenderTarget, e.Origin, e.Info.WithSize(userVisibleSize), e.Info);
				}

				base.OnPaintSurface(e);
			}
		}
	}
}

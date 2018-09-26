using System;
using Windows.Foundation;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.UWP
{
	public class SKSwapChainPanel : AngleSwapChainPanel
	{
		private GRGlInterface glInterface;
		private GRContext context;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

		public SKSwapChainPanel()
		{
		}

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		public GRContext GRContext => context;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void OnRenderFrame(Rect rect)
		{
			// clear everything
			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.CreateNativeAngleInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);
			}

			// manage the drawing surface
			if (renderTarget == null || surface == null || renderTarget.Width != (int)rect.Width || renderTarget.Height != (int)rect.Height)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				renderTarget = SKGLDrawable.CreateRenderTarget((int)rect.Width, (int)rect.Height);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888));
			}

			// update the control
			surface.Canvas.Flush();
			context.Flush();
		}
	}
}

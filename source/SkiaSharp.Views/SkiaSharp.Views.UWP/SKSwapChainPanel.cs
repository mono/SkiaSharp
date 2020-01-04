using System;
using Windows.Foundation;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.UWP
{
	public class SKSwapChainPanel : AngleSwapChainPanel
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

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
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;

				var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget((int)rect.Width, (int)rect.Height, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
			}

			// update the control
			surface.Canvas.Flush();
			context.Flush();
		}
	}
}

using System;
using System.ComponentModel;
using CoreAnimation;
using CoreVideo;
using OpenGL;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.Mac
{
	public class SKGLLayer : CAOpenGLLayer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		public SKGLLayer()
		{
			Opaque = true;
			NeedsDisplayOnBoundsChange = true;
		}

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		public override void DrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			CGLContext.CurrentContext = glContext;

			if (context == null)
			{
				// get the bits for SkiaSharp
				var glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);
			}

			// manage the drawing surface
			var surfaceWidth = (int)(Bounds.Width * ContentsScale);
			var surfaceHeight = (int)(Bounds.Height * ContentsScale);
			var newSize = new SKSizeI(surfaceWidth, surfaceHeight);
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				// read the info from the buffer
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
			}

			// flush the SkiaSharp context to the GL context
			canvas.Flush();
			context.Flush();

			base.DrawInCGLContext(glContext, pixelFormat, timeInterval, ref timeStamp);
		}

		public override void Release(CGLContext glContext)
		{
			context.Dispose();

			base.Release(glContext);
		}
	}
}

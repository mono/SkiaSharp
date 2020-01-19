using System;
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
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

		public SKGLLayer()
		{
			Opaque = true;
			NeedsDisplayOnBoundsChange = true;
		}

		[Obsolete("Use PaintSurface instead.")]
		public ISKGLLayerDelegate SKDelegate { get; set; }

		public SKSize CanvasSize => renderTarget.Size;

		public GRContext GRContext => context;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		public override void DrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			CGLContext.CurrentContext = glContext;

			if (context == null)
			{
				// get the bits for SkiaSharp
				var glInterface = GRGlInterface.CreateNativeGlInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);
			}

			// manage the drawing surface
			var surfaceWidth = (int)(Bounds.Width * ContentsScale);
			var surfaceHeight = (int)(Bounds.Height * ContentsScale);
			if (renderTarget == null || surface == null || renderTarget.Width != surfaceWidth || renderTarget.Height != surfaceHeight)
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
				renderTarget = new GRBackendRenderTarget(surfaceWidth, surfaceHeight, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
#pragma warning disable CS0618 // Type or member is obsolete
				DrawInSurface(e.Surface, e.RenderTarget);
				SKDelegate?.DrawInSurface(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// flush the SkiaSharp context to the GL context
			surface.Canvas.Flush();
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

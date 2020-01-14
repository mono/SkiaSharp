#if !__WATCHOS__

using System;
using CoreAnimation;
using CoreGraphics;
using OpenGLES;
using SkiaSharp.Views.GlesInterop;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	public class SKGLLayer : CAEAGLLayer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private EAGLContext glContext;
		private uint renderBuffer;
		private uint framebuffer;

		private GRContext context;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

		public SKGLLayer()
		{
			Opaque = true;
		}

		[Obsolete("Use PaintSurface instead.")]
		public ISKGLLayerDelegate SKDelegate { get; set; }

		public SKSize CanvasSize => renderTarget.Size;

		public GRContext GRContext => context;

		public virtual void Render()
		{
			if (glContext == null)
			{
				PrepareGLContexts();
			}

			EAGLContext.SetCurrentContext(glContext);

			// manage the drawing surface
			if (renderTarget == null || surface == null)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out var bufferWidth);
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out var bufferHeight);
				var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget(bufferWidth, bufferHeight, samples, stencil, glInfo);

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

			// present the GL buffers
			glContext.PresentRenderBuffer(Gles.GL_RENDERBUFFER);
			EAGLContext.SetCurrentContext(null);
		}

		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				if (glContext != null)
				{
					ResizeGLContexts();
					renderTarget?.Dispose();
					renderTarget = null;
				}
				Render();
			}
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		private void PrepareGLContexts()
		{
			// create GL context
			glContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			EAGLContext.SetCurrentContext(glContext);

			// create render buffer
			Gles.glGenRenderbuffers(1, ref renderBuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderBuffer);
			glContext.RenderBufferStorage(Gles.GL_RENDERBUFFER, this);

			// create frame buffer
			Gles.glGenFramebuffers(1, ref framebuffer);
			Gles.glBindFramebuffer(Gles.GL_FRAMEBUFFER, framebuffer);
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderBuffer);

			// get the bits for SkiaSharp
			var glInterface = GRGlInterface.CreateNativeGlInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);

			// finished
			EAGLContext.SetCurrentContext(null);
		}

		private void ResizeGLContexts()
		{
			// nuke old buffers
			Gles.glDeleteRenderbuffers(1, ref renderBuffer);

			// re-create render buffer
			Gles.glGenRenderbuffers(1, ref renderBuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderBuffer);
			glContext.RenderBufferStorage(Gles.GL_RENDERBUFFER, this);

			// re-link
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderBuffer);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context.Dispose();
			glContext.Dispose();
		}
	}
}

#endif

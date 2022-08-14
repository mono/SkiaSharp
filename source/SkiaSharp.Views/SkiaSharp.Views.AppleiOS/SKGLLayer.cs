#if !__WATCHOS__ && !__MACCATALYST__

using System;
using System.ComponentModel;
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
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;
		private bool recreateSurface = true;

		public SKGLLayer()
		{
			Opaque = true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use PaintSurface instead.")]
		public ISKGLLayerDelegate SKDelegate { get; set; }

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		public virtual void Render()
		{
			if (glContext == null)
			{
				PrepareGLContexts();
			}

			EAGLContext.SetCurrentContext(glContext);

			// get the new surface size
			var newSize = lastSize;
			if (recreateSurface)
			{
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out var bufferWidth);
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out var bufferHeight);
				newSize = new SKSizeI(bufferWidth, bufferHeight);
			}

			// manage the drawing surface
			if (recreateSurface || renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
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
#pragma warning disable CS0618 // Type or member is obsolete
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType, glInfo);
				OnPaintSurface(e);
				DrawInSurface(e.Surface, e.RenderTarget);
				SKDelegate?.DrawInSurface(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// flush the SkiaSharp context to the GL context
			canvas.Flush();
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
				}
				Render();
			}
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
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
			var glInterface = GRGlInterface.Create();
			context = GRContext.CreateGl(glInterface);

			// finished
			EAGLContext.SetCurrentContext(null);

			recreateSurface = true;
		}

		private void ResizeGLContexts()
		{
			// delete old buffers
			Gles.glDeleteRenderbuffers(1, ref renderBuffer);

			// re-create render buffer
			Gles.glGenRenderbuffers(1, ref renderBuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderBuffer);
			glContext.RenderBufferStorage(Gles.GL_RENDERBUFFER, this);

			// re-link
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderBuffer);

			recreateSurface = true;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context?.Dispose();
			glContext?.Dispose();
		}
	}
}

#endif

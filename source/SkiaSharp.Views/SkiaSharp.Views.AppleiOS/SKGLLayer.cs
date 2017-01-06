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
		private EAGLContext glContext;
		private uint renderBuffer;
		private uint framebuffer;

		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		public SKGLLayer()
		{
			Opaque = true;
		}

		public ISKGLLayerDelegate SKDelegate { get; set; }

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		public virtual void Render()
		{
			if (glContext == null)
			{
				PrepareGLContexts();
			}

			EAGLContext.SetCurrentContext(glContext);

			// create the surface
			if (renderTarget.Width == 0 || renderTarget.Height == 0)
			{
				renderTarget = SKGLDrawable.CreateRenderTarget();
			}
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw on the surface
				DrawInSurface(surface, renderTarget);
				SKDelegate?.DrawInSurface(surface, renderTarget);

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp context to the GL context
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
					renderTarget = SKGLDrawable.CreateRenderTarget();
				}
				Render();
			}
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
		
		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			PaintSurface?.Invoke(this, new SKPaintGLSurfaceEventArgs(surface, renderTarget));
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

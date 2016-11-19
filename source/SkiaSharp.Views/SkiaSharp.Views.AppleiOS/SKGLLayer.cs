using System;
using CoreAnimation;
using CoreGraphics;
using OpenGLES;
using OpenTK.Graphics.ES20;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	public class SKGLLayer : CAEAGLLayer
	{
		private EAGLContext glContext;
		private int renderBuffer;
		private int framebuffer;

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
			glContext.PresentRenderBuffer((uint)RenderbufferTarget.Renderbuffer);
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
			GL.GenRenderbuffers(1, out renderBuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
			glContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, this);

			// create frame buffer
			GL.GenFramebuffers(1, out framebuffer);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderBuffer);

			// get the bits for SkiaSharp
			var glInterface = GRGlInterface.CreateNativeGlInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);

			// finished
			EAGLContext.SetCurrentContext(null);
		}

		private void ResizeGLContexts()
		{
			// nuke old buffers
			GL.DeleteRenderbuffers(1, ref renderBuffer);

			// re-create render buffer
			GL.GenRenderbuffers(1, out renderBuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
			glContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, this);

			// re-link
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderBuffer);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context.Dispose();
			glContext.Dispose();
		}
	}
}

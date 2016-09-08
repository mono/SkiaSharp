using System;
using CoreAnimation;
using OpenGLES;
using OpenTK.Graphics.ES20;

namespace SkiaSharp.Views
{
	public class SKGLLayer : CAEAGLLayer
	{
		private EAGLContext glContext;
		private uint colorRenderBuffer;

		private GRContext context;
		private int framebuffer;
		private int bufferWidth;
		private int bufferHeight;

		public SKGLLayer()
		{
			Opaque = true;

		}

		public ISKGLLayerDelegate SKDelegate { get; set; }

		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		public override void LayoutSublayers()
		{
			base.LayoutSublayers();

			if (glContext == null)
				CreateGlContext();
			if (context == null)
				CreateSkiaContext();

			// create the surface
			var renderTarget = new GRBackendRenderTargetDesc
			{
				Width = bufferWidth,
				Height = bufferHeight,
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.TopLeft,
				SampleCount = 0,
				StencilBits = 8,
				RenderTargetHandle = (IntPtr)framebuffer,
			};
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw on the surface
				DrawInSurface(surface, renderTarget);
				SKDelegate.DrawInSurface(surface, renderTarget);

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp context to the GL context
			context.Flush();

			// present the GL buffers
			glContext.PresentRenderBuffer((uint)RenderbufferTarget.Renderbuffer);
		}

		private void CreateSkiaContext()
		{
			EAGLContext.SetCurrentContext(glContext);

			// get the bits for SkiaSharp
			var glInterface = GRGlInterface.CreateNativeInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);
		}

		private void CreateGlContext()
		{
			// create GL context
			glContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			EAGLContext.SetCurrentContext(glContext);

			// create render buffer
			GL.GenRenderbuffers(1, out colorRenderBuffer);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRenderBuffer);
			glContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, this);

			// get dimensions from buffer
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out bufferWidth);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out bufferHeight);

			// create frame buffer
			GL.GenFramebuffers(1, out framebuffer);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorRenderBuffer);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context.Dispose();
			glContext.Dispose();
		}
	}
}

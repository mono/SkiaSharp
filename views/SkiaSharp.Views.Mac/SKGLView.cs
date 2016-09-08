using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using OpenTK.Graphics.OpenGL;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKGLView : NSOpenGLView
	{
		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		public SKGLView()
		{
			Initialize();
		}

		private void Initialize()
		{
			WantsBestResolutionOpenGLSurface = true;
		}

		public override void PrepareOpenGL()
		{
			base.PrepareOpenGL();

			// create the context
			var glInterface = GRGlInterface.CreateNativeInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);

			// get the current frame buffer
			int framebuffer;
			GL.GetInteger(GetPName.FramebufferBinding, out framebuffer);

			// get samples
			var screen = OpenGLContext.CurrentVirtualScreen;
			int samples = 0;
			OpenGLContext.PixelFormat.GetValue(ref samples, NSOpenGLPixelFormatAttribute.Samples, screen);
			int stencils = 0;
			OpenGLContext.PixelFormat.GetValue(ref stencils, NSOpenGLPixelFormatAttribute.StencilSize, screen);

			// create the surface
			renderTarget = new GRBackendRenderTargetDesc
			{
				Width = 0, // set later
				Height = 0, // set later
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.TopLeft,
				SampleCount = samples,
				StencilBits = stencils,
				RenderTargetHandle = (IntPtr)framebuffer,
			};
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			var size = ConvertSizeToBacking(Bounds.Size);
			renderTarget.Width = (int)size.Width;
			renderTarget.Height = (int)size.Height;

			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw on the surface
				DrawInSurface(surface, renderTarget);

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp contents to GL
			context.Flush();

			GL.Flush();
		}

		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}
	}
}

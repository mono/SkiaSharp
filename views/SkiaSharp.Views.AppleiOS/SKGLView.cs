using System;
using System.ComponentModel;
using CoreGraphics;
using GLKit;
using OpenGLES;
using OpenTK.Graphics.ES20;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKGLView : GLKView, IGLKViewDelegate
	{
		private GRContext context;
		private int framebuffer;

		public SKGLView()
		{
			Initialize();
		}

		private void Initialize()
		{
			// create the GL context
			Context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			DrawableColorFormat = GLKViewDrawableColorFormat.RGBA8888;
			DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;
			DrawableStencilFormat = GLKViewDrawableStencilFormat.Format8;
			DrawableMultisample = GLKViewDrawableMultisample.Sample4x;

			// hook up the drawing 
			Delegate = this;
		}

		public new void DrawInRect(GLKView view, CGRect rect)
		{
			// get the bits for SkiaSharp
			if (context == null)
			{
				// create the context
				var glInterface = GRGlInterface.CreateNativeInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);

				// get the current frame buffer
				GL.GetInteger(GetPName.FramebufferBinding, out framebuffer);
			}

			// create the surface
			var renderTarget = new GRBackendRenderTargetDesc
			{
				Width = (int)DrawableWidth,
				Height = (int)DrawableHeight,
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

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp contents to GL
			context.Flush();
		}

		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
	
			SetNeedsDisplay();
		}
	}
}

using System.ComponentModel;
using CoreGraphics;
using GLKit;
using OpenGLES;

namespace SkiaSharp.Views
{
	[DesignTimeVisible(true)]
	public class SKGLView : GLKView, IGLKViewDelegate
	{
		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

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
			if (context == null)
			{
				var glInterface = GRGlInterface.CreateNativeInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);

				// get the initial details
				renderTarget = SKGLDrawable.CreateRenderTarget();
			}

			// set the dimensions as they might have changed
			renderTarget.Width = (int)DrawableWidth;
			renderTarget.Height = (int)DrawableHeight;

			// create the surface
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

		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				SetNeedsDisplay();
			}
		}
	}
}

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

			renderTarget = SKGLDrawable.CreateRenderTarget();
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

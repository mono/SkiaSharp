using CoreAnimation;
using CoreVideo;
using OpenGL;

namespace SkiaSharp.Views
{
	public class SKGLLayer : CAOpenGLLayer
	{
		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		public SKGLLayer()
		{
			Opaque = true;
			NeedsDisplayOnBoundsChange = true;
		}

		public ISKGLLayerDelegate SKDelegate { get; set; }

		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		public override void DrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			CGLContext.CurrentContext = glContext;

			if (context == null)
			{
				// get the bits for SkiaSharp
				var glInterface = GRGlInterface.CreateNativeInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);
			}

			// create the surface
			renderTarget = SKGLDrawable.CreateRenderTarget();
			renderTarget.Width = (int)(Bounds.Width * ContentsScale);
			renderTarget.Height = (int)(Bounds.Height * ContentsScale);
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw on the surface
				DrawInSurface(surface, renderTarget);
				SKDelegate?.DrawInSurface(surface, renderTarget);

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp context to the GL context
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

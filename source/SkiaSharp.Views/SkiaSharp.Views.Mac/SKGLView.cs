using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using OpenTK.Graphics.OpenGL;

namespace SkiaSharp.Views.Mac
{
	[Register(nameof(SKGLView))]
	[DesignTimeVisible(true)]
	public class SKGLView : NSOpenGLView
	{
		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		// created in code
		public SKGLView()
		{
			Initialize();
		}

		// created in code
		public SKGLView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		public SKGLView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		public override void AwakeFromNib()
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
			var glInterface = GRGlInterface.CreateNativeGlInterface();
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

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
			PaintSurface?.Invoke(this, new SKPaintGLSurfaceEventArgs(surface, renderTarget));
		}
	}
}

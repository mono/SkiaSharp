using System;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace SkiaSharp.Views.Android
{
	public abstract class SKGLTextureViewRenderer : Java.Lang.Object, GLTextureView.IRenderer
	{
		private GRContext context;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private int surfaceWidth;
		private int surfaceHeight;

		public SKSize CanvasSize => renderTarget.Size;

		public GRContext GRContext => context;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
		}

		[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
		protected virtual void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

		public void OnDrawFrame(IGL10 gl)
		{
			GLES10.GlClear(GLES10.GlColorBufferBit | GLES10.GlDepthBufferBit | GLES10.GlStencilBufferBit);

			// create the contexts if not done already
			if (context == null)
			{
				var glInterface = GRGlInterface.CreateNativeGlInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);
			}

			// manage the drawing surface
			if (renderTarget == null || surface == null || renderTarget.Width != surfaceWidth || renderTarget.Height != surfaceHeight)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				renderTarget = SKGLDrawable.CreateRenderTarget(surfaceWidth, surfaceHeight);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
				OnPaintSurface(e);
#pragma warning disable CS0618 // Type or member is obsolete
				OnDrawFrame(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// flush the SkiaSharp contents to GL
			surface.Canvas.Flush();
			context.Flush();
		}

		public void OnSurfaceChanged(IGL10 gl, int width, int height)
		{
			GLES20.GlViewport(0, 0, width, height);

			surfaceWidth = width;
			surfaceHeight = height;
		}

		public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
		{
			FreeContext();
		}

		public void OnSurfaceDestroyed()
		{
			FreeContext();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FreeContext();
			}
			base.Dispose(disposing);
		}

		private void FreeContext()
		{
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			context?.Dispose();
			context = null;
		}
	}
}

using System;
using System.ComponentModel;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;

using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#else
namespace SkiaSharp.Views.Android
#endif
{
#if HAS_UNO
	internal
#else
	public
#endif
	abstract partial class SKGLTextureViewRenderer : Java.Lang.Object, GLTextureView.IRenderer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;
		private SKSizeI newSize;

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
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
				var glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);
			}

			// manage the drawing surface
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				// read the info from the buffer
				var buffer = new int[3];
				GLES20.GlGetIntegerv(GLES20.GlFramebufferBinding, buffer, 0);
				GLES20.GlGetIntegerv(GLES20.GlStencilBits, buffer, 1);
				GLES20.GlGetIntegerv(GLES20.GlSamples, buffer, 2);
				var samples = buffer[2];
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				glInfo = new GRGlFramebufferInfo((uint)buffer[0], colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, buffer[1], glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
#pragma warning disable CS0618 // Type or member is obsolete
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType, glInfo);
				OnPaintSurface(e);
				OnDrawFrame(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// flush the SkiaSharp contents to GL
			canvas.Flush();
			context.Flush();
		}

		public void OnSurfaceChanged(IGL10 gl, int width, int height)
		{
			GLES20.GlViewport(0, 0, width, height);

			// get the new surface size
			newSize = new SKSizeI(width, height);
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

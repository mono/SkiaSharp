using System;
using System.ComponentModel;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;

using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// An abstract implementation of <see cref="global::Android.Opengl.GLSurfaceView.IRenderer" /> that provides a <see cref="SKSurface" /> for drawing.
	/// </summary>
	public abstract class SKGLSurfaceViewRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
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

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => lastSize;

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext GRContext => context;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
		}

		/// <summary>
		/// Called to draw the current frame.
		/// </summary>
		/// <param name="gl">The GL interface.</param>
		public void OnDrawFrame(IGL10 gl)
		{
			GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit | GLES20.GlStencilBufferBit);

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
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
			}

			// flush the SkiaSharp contents to GL
			canvas.Flush();
			context.Flush();
		}

		/// <summary>
		/// Called when the surface changed size.
		/// </summary>
		/// <param name="gl">The GL interface.</param>
		/// <param name="width">The new surface width.</param>
		/// <param name="height">The new surface height.</param>
		public void OnSurfaceChanged(IGL10 gl, int width, int height)
		{
			GLES20.GlViewport(0, 0, width, height);

			// get the new surface size
			newSize = new SKSizeI(width, height);
		}

		/// <summary>
		/// Called when the surface is created or recreated.
		/// </summary>
		/// <param name="gl">The GL interface.</param>
		/// <param name="config">The EGLConfig of the created surface.</param>
		public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
		{
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

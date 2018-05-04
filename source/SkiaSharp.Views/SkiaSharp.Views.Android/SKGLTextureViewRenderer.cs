using System;
using Android.Opengl;
using Android.Runtime;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

namespace SkiaSharp.Views.Android
{
	public abstract class SKGLTextureViewRenderer : Java.Lang.Object, GLTextureView.IRenderer
	{
		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		public GRContext GRContext => context;

		protected abstract void OnDrawFrame(SKSurface surface, GRBackendRenderTargetDesc renderTarget);

		public void OnDrawFrame(IGL10 gl)
		{
			GLES10.GlClear(GLES10.GlStencilBufferBit);

			// create the surface
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw using SkiaSharp
				OnDrawFrame(surface, renderTarget);

				surface.Canvas.Flush();
			}

			// flush the SkiaSharp contents to GL
			context.Flush();
		}

		public void OnSurfaceChanged(IGL10 gl, int width, int height)
		{
			GLES20.GlViewport(0, 0, width, height);

			renderTarget.Width = width;
			renderTarget.Height = height;

			CreateContext();
		}

		public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
		{
			FreeContext();

			// get the config
			var egl = EGLContext.EGL.JavaCast<IEGL10>();
			var disp = egl.EglGetCurrentDisplay();

			// stencil buffers
			int[] stencilbuffers = new int[1];
			egl.EglGetConfigAttrib(disp, config, EGL10.EglStencilSize, stencilbuffers);

			// samples
			int[] samples = new int[1];
			egl.EglGetConfigAttrib(disp, config, EGL10.EglSamples, samples);

			// get the frame buffer
			int[] framebuffers = new int[1];
			gl.GlGetIntegerv(GLES20.GlFramebufferBinding, framebuffers, 0);

			// create the render target
			renderTarget = new GRBackendRenderTargetDesc
			{
				Width = 0, // set later
				Height = 0, // set later
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.BottomLeft,
				SampleCount = samples[0],
				StencilBits = stencilbuffers[0],
				RenderTargetHandle = (IntPtr)framebuffers[0],
			};

			CreateContext();
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
			if (context != null)
			{
				context.Dispose();
				context = null;
			}
		}

		private void CreateContext()
		{
			if (context == null)
			{
				var glInterface = GRGlInterface.CreateNativeGlInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);
			}
		}
	}
}

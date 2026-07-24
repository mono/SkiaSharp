using System;
using SkiaSharp.Views.GlesInterop;
using Windows.Foundation;

#if WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	public partial class SKSwapChainPanel : AngleSwapChainPanel
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRGlInterface glInterface;
		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		public SKSwapChainPanel()
		{
		}

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override unsafe void OnRenderFrame(Rect rect)
		{
			// clear everything
			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// create the SkiaSharp context
			if (context == null)
			{
#if WINDOWS_UWP
				// TODO: on uwp SkiaApi.gr_glinterface_create_native_interface ()) throws
				// Indirect call guard check detected invalid control transfer
				// if app was idle in the background 
				glInterface = GRGlInterface.CreateAngle();
#else
				glInterface = GRGlInterface.Create();
#endif
				context = GRContext.CreateGl(glInterface);
			}

			// get the new surface size
			var newSize = new SKSizeI((int)rect.Width, (int)rect.Height);

			// manage the drawing surface
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				// read the info from the buffer
				int framebuffer, stencil, samples;
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, &framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, &stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, &samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;

				glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, glInfo);
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
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
			}

			// update the control
			canvas?.Flush();
			context.Flush();
		}

		protected override void OnDestroyingContext()
		{
			base.OnDestroyingContext();

			lastSize = default;

			canvas?.Dispose();
			canvas = null;

			surface?.Dispose();
			surface = null;

			renderTarget?.Dispose();
			renderTarget = null;

			glInfo = default;

			context?.AbandonContext(false);
			context?.Dispose();
			context = null;

			glInterface?.Dispose();
			glInterface = null;
		}
	}
}

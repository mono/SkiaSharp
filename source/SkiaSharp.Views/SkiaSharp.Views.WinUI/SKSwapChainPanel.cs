using System;
using SkiaSharp.Views.GlesInterop;
using Windows.Foundation;

#if WINDOWS
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>
	/// A SwapChainPanel that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public class SKSwapChainPanel : AngleSwapChainPanel
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

		/// <summary>
		/// Creates a new instance of <see cref="SKSwapChainPanel" />.
		/// </summary>
		public SKSwapChainPanel()
		{
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize => lastSize;

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext GRContext => context;

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKSwapChainPanel.OnPaintSurface(SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKSwapChainPanel.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.BackendRenderTarget.Width;
		///     var surfaceHeight = e.BackendRenderTarget.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// </code>
		/// </example>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void OnRenderFrame(Rect rect)
		{
			// clear everything
			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.Create();
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
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
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
			canvas.Flush();
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

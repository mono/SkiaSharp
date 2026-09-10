using System;
using SkiaSharp.Views.GlesInterop;
using Windows.Foundation;

#if WINDOWS
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	/// <summary>A XAML control that uses hardware-accelerated rendering via ANGLE to draw using SkiaSharp.</summary>
	/// <remarks>This control uses an OpenGL ES context via ANGLE to provide GPU-accelerated SkiaSharp drawing. It inherits from <see cref="T:SkiaSharp.Views.Windows.AngleSwapChainPanel" /> and provides SkiaSharp-specific rendering functionality.</remarks>
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKSwapChainPanel" /> class.</summary>
		/// <remarks />
		public SKSwapChainPanel()
		{
		}

		/// <summary>Gets the current canvas size in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks />
		public SKSize CanvasSize => lastSize;

		/// <summary>Gets the GPU context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRContext" /> used for GPU-accelerated rendering.</value>
		/// <remarks />
		public GRContext GRContext => context;

		/// <summary>Occurs when the surface needs to be repainted.</summary>
		/// <remarks>Handle this event to perform drawing operations on the GPU-accelerated surface.</remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</summary>
		/// <param name="e">The event arguments containing the surface and render target information.</param>
		/// <remarks>Override this method to perform custom drawing on the surface without subscribing to the <see cref="E:SkiaSharp.Views.Windows.SKSwapChainPanel.PaintSurface" /> event.</remarks>
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		/// <param name="rect">The rectangle defining the render area dimensions.</param>
		/// <summary>Called when a frame should be rendered.</summary>
		/// <remarks>This method creates the SkiaSharp context and surface, then invokes <see cref="M:SkiaSharp.Views.Windows.SKSwapChainPanel.OnPaintSurface(SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs)" /> to perform the actual drawing.</remarks>
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

		/// <summary>Called when the OpenGL ES context is being destroyed.</summary>
		/// <remarks>Override this method to clean up SkiaSharp and OpenGL resources before the context is destroyed.</remarks>
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

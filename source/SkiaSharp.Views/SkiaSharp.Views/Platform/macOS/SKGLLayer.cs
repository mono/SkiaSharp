using System;
using System.ComponentModel;
using CoreAnimation;
using CoreVideo;
using OpenGL;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.Mac
{
	/// <summary>
	/// A CoreAnimation OpenGL layer that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public class SKGLLayer : CAOpenGLLayer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKGLLayer" />.
		/// </summary>
		public SKGLLayer()
		{
			Opaque = true;
			NeedsDisplayOnBoundsChange = true;
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
		/// Occurs when the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKGLLayer.OnPaintSurface(SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKGLLayer.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myLayer.PaintSurface += (sender, e) => 
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
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>
		/// Draws the OpenGL content for the specified time.
		/// </summary>
		/// <param name="glContext">The rendering context in to which the OpenGL content should be rendered.</param>
		/// <param name="pixelFormat">The pixel format used when the context was created.</param>
		/// <param name="timeInterval">The current layer time.</param>
		/// <param name="timeStamp">The display timestamp associated with the time interval. Can be <see langword="null" />.</param>
		public override void DrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			CGLContext.CurrentContext = glContext;

			if (context == null)
			{
				// get the bits for SkiaSharp
				var glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);
			}

			// manage the drawing surface
			var surfaceWidth = (int)(Bounds.Width * ContentsScale);
			var surfaceHeight = (int)(Bounds.Height * ContentsScale);
			var newSize = new SKSizeI(surfaceWidth, surfaceHeight);
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
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
			}

			// flush the SkiaSharp context to the GL context
			canvas.Flush();
			context.Flush();

			base.DrawInCGLContext(glContext, pixelFormat, timeInterval, ref timeStamp);
		}

		/// <summary>
		/// Releases the specified rendering context.
		/// </summary>
		/// <param name="glContext">The rendering context to release.</param>
		public override void Release(CGLContext glContext)
		{
			context.Dispose();

			base.Release(glContext);
		}
	}
}

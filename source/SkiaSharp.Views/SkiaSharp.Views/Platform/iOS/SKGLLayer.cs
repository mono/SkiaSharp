#if !__MACCATALYST__

using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using CoreAnimation;
using CoreGraphics;
using OpenGLES;
using SkiaSharp.Views.GlesInterop;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	/// <summary>
	/// A CoreAnimation OpenGL layer that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[ObsoletedOSPlatform("tvos12.0", "Use 'Metal' instead.")]
	[ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")]
	[SupportedOSPlatform("ios")]
	[SupportedOSPlatform("tvos")]
	[UnsupportedOSPlatform("macos")]
	[UnsupportedOSPlatform("maccatalyst")]
	public class SKGLLayer : CAEAGLLayer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private EAGLContext glContext;
		private uint renderBuffer;
		private uint framebuffer;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;
		private bool recreateSurface = true;

		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKGLLayer" />.
		/// </summary>
		public SKGLLayer()
		{
			Opaque = true;
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
		/// Redraws the layer's contents.
		/// </summary>
		public virtual void Render()
		{
			if (glContext == null)
			{
				PrepareGLContexts();
			}

			EAGLContext.SetCurrentContext(glContext);

			// get the new surface size
			var newSize = lastSize;
			if (recreateSurface)
			{
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_WIDTH, out var bufferWidth);
				Gles.glGetRenderbufferParameteriv(Gles.GL_RENDERBUFFER, Gles.GL_RENDERBUFFER_HEIGHT, out var bufferHeight);
				newSize = new SKSizeI(bufferWidth, bufferHeight);
			}

			// manage the drawing surface
			if (recreateSurface || renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
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

			// present the GL buffers
			glContext.PresentRenderBuffer(Gles.GL_RENDERBUFFER);
			EAGLContext.SetCurrentContext(null);
		}

		/// <summary>
		/// Gets or sets the layer's frame rectangle.
		/// </summary>
		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				if (glContext != null)
				{
					ResizeGLContexts();
				}
				Render();
			}
		}

		/// <summary>
		/// Occurs when the the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="OnPaintSurface(SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="PaintSurface" />
		/// event.
		/// ## Examples
		/// ```csharp
		/// myLayer.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.BackendRenderTarget.Width;
		/// var surfaceHeight = e.BackendRenderTarget.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush ();
		/// };
		/// ```
		/// </remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private void PrepareGLContexts()
		{
			// create GL context
			glContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			EAGLContext.SetCurrentContext(glContext);

			// create render buffer
			Gles.glGenRenderbuffers(1, ref renderBuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderBuffer);
			glContext.RenderBufferStorage(Gles.GL_RENDERBUFFER, this);

			// create frame buffer
			Gles.glGenFramebuffers(1, ref framebuffer);
			Gles.glBindFramebuffer(Gles.GL_FRAMEBUFFER, framebuffer);
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderBuffer);

			// get the bits for SkiaSharp
			var glInterface = GRGlInterface.Create();
			context = GRContext.CreateGl(glInterface);

			// finished
			EAGLContext.SetCurrentContext(null);

			recreateSurface = true;
		}

		private void ResizeGLContexts()
		{
			// delete old buffers
			Gles.glDeleteRenderbuffers(1, ref renderBuffer);

			// re-create render buffer
			Gles.glGenRenderbuffers(1, ref renderBuffer);
			Gles.glBindRenderbuffer(Gles.GL_RENDERBUFFER, renderBuffer);
			glContext.RenderBufferStorage(Gles.GL_RENDERBUFFER, this);

			// re-link
			Gles.glFramebufferRenderbuffer(Gles.GL_FRAMEBUFFER, Gles.GL_COLOR_ATTACHMENT0, Gles.GL_RENDERBUFFER, renderBuffer);

			recreateSurface = true;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context?.Dispose();
			glContext?.Dispose();
		}
	}
}

#endif

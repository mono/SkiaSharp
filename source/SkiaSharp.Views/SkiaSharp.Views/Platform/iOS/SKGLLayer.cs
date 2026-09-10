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
	/// <summary>A CoreAnimation OpenGL layer that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKGLLayer" /> class.</summary>
		/// <remarks />
		public SKGLLayer()
		{
			Opaque = true;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => lastSize;

		/// <summary>Gets the current GPU context.</summary>
		/// <value>The current GPU context.</value>
		/// <remarks />
		public GRContext GRContext => context;

		/// <summary>Redraws the layer's contents.</summary>
		/// <remarks />
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

		/// <summary>Gets or sets the layer's frame rectangle.</summary>
		/// <value>The layer's frame rectangle.</value>
		/// <remarks />
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

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.iOS.SKGLLayer.OnPaintSurface(SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.iOS.SKGLLayer.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myLayer.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.BackendRenderTarget.Width;
		///     var surfaceHeight = e.BackendRenderTarget.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// };
		/// ```
		/// ]]></format>
		///         </remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <summary>Implement this to draw on the canvas.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.iOS.SKGLLayer.OnPaintSurface(SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.iOS.SKGLLayer.PaintSurface>
		/// event.
		///
		/// > [!IMPORTANT]
		/// > If this method is overridden, then the base must be called, otherwise the
		/// > event will not be fired.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface (SKPaintGLSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface (e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.BackendRenderTarget.Width;
		///     var surfaceHeight = e.BackendRenderTarget.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// }
		/// ```
		/// ]]></format>
		///         </remarks>
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

		/// <param name="disposing">
		///           <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.Views.iOS.SKGLLayer" /> and optionally releases the managed resources.</summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.Views.iOS.SKGLLayer" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			context?.Dispose();
			glContext?.Dispose();
		}
	}
}

#endif

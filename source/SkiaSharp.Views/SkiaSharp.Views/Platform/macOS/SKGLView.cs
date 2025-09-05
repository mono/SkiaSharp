using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.GlesInterop;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#else
namespace SkiaSharp.Views.Mac
#endif
{
	/// <summary>
	/// A hardware-accelerated view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[Register(nameof(SKGLView))]
	[DesignTimeVisible(true)]
#if HAS_UNO
	internal
#else
	public
#endif
	partial class SKGLView : NSOpenGLView
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

		// created in code
		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKGLView" />.
		/// </summary>
		public SKGLView()
		{
			Initialize();
		}

		// created in code
		/// <summary>
		/// Initializes the <see cref="SKGLView" /> with the specified frame.
		/// </summary>
		/// <param name="frame">The frame used by the view, expressed in Mac points.</param>
		public SKGLView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		/// <summary>
		/// A constructor used when creating managed representations of unmanaged objects; Called by the runtime.
		/// </summary>
		/// <param name="p">The pointer (handle) to the unmanaged object.</param>
		public SKGLView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		/// <summary>
		/// Called after the object has been loaded from the nib file. Overriders must call the base method.
		/// </summary>
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			WantsBestResolutionOpenGLSurface = true;

			var attrs = new NSOpenGLPixelFormatAttribute[]
			{
				//NSOpenGLPixelFormatAttribute.OpenGLProfile, (NSOpenGLPixelFormatAttribute)NSOpenGLProfile.VersionLegacy,
				NSOpenGLPixelFormatAttribute.Accelerated,
				NSOpenGLPixelFormatAttribute.DoubleBuffer,
				NSOpenGLPixelFormatAttribute.Multisample,

				NSOpenGLPixelFormatAttribute.ColorSize, (NSOpenGLPixelFormatAttribute)32,
				NSOpenGLPixelFormatAttribute.AlphaSize, (NSOpenGLPixelFormatAttribute)8,
				NSOpenGLPixelFormatAttribute.DepthSize, (NSOpenGLPixelFormatAttribute)24,
				NSOpenGLPixelFormatAttribute.StencilSize, (NSOpenGLPixelFormatAttribute)8,
				NSOpenGLPixelFormatAttribute.SampleBuffers, (NSOpenGLPixelFormatAttribute)1,
				NSOpenGLPixelFormatAttribute.Samples, (NSOpenGLPixelFormatAttribute)4,
				(NSOpenGLPixelFormatAttribute)0,
			};
			PixelFormat = new NSOpenGLPixelFormat(attrs);
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
		/// Used by subclasses to initialize OpenGL state.
		/// </summary>
		public override void PrepareOpenGL()
		{
			base.PrepareOpenGL();

			// create the context
			var glInterface = GRGlInterface.Create();
			context = GRContext.CreateGl(glInterface);
		}

		/// <summary>
		/// Called by Cocoa when the view's visible rectangle or bounds change.
		/// </summary>
		public override void Reshape()
		{
			base.Reshape();

			// get the new surface size
			var size = ConvertSizeToBacking(Bounds.Size);
			newSize = new SKSizeI((int)size.Width, (int)size.Height);
		}

		private nfloat lastBackingScaleFactor = 0;

		/// <summary>
		/// Draws the view within the passed-in rectangle.
		/// </summary>
		/// <param name="dirtyRect">The rectangle to draw.</param>
		public override void DrawRect(CGRect dirtyRect)
		{
			// Track if the scale of the display has changed and if so force the SKGLView to reshape itself.
			// If this is not done, the output will scale correctly when the window is dragged from a non-retina to a retina display.
			if (Window != null && lastBackingScaleFactor != Window.BackingScaleFactor)
			{
				bool isFirstDraw = lastBackingScaleFactor == 0;
				lastBackingScaleFactor = Window.BackingScaleFactor;
				if (!isFirstDraw)
				{
					Reshape();
					// A redraw will also be necessary. Invoke later or the request will be ignored
					Invoke(() => {
						NeedsDisplay = true;
					}, 0);
					// do not proceed at the wrong scale
					return;
				}
			}

			base.DrawRect(dirtyRect);

			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// create the render target
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

			// flush the SkiaSharp contents to GL
			canvas.Flush();
			context.Flush();

			OpenGLContext.FlushBuffer();
		}

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKGLView.OnPaintSurface(SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKGLView.PaintSurface" />
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
			PaintSurface?.Invoke(this, e);
		}
	}
}

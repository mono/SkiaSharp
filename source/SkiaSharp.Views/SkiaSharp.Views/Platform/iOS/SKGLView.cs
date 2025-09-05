#if !__MACCATALYST__

using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using GLKit;
using OpenGLES;
using SkiaSharp.Views.GlesInterop;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif HAS_UNO
namespace SkiaSharp.Views.UWP
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	/// <summary>
	/// A hardware-accelerated view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")]
	[ObsoletedOSPlatform("tvos12.0", "Use 'Metal' instead.")]
	[SupportedOSPlatform("ios")]
	[SupportedOSPlatform("tvos")]
	[UnsupportedOSPlatform("macos")]
	[DesignTimeVisible(true)]
#if HAS_UNO
	internal
#else
	[Register(nameof(SKGLView))]
	public
#endif
	class SKGLView : GLKView, IGLKViewDelegate, IComponent
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		// for IComponent
#pragma warning disable 67
		private event EventHandler DisposedInternal;
#pragma warning restore 67
		ISite IComponent.Site { get; set; }
		event EventHandler IComponent.Disposed
		{
			add { DisposedInternal += value; }
			remove { DisposedInternal -= value; }
		}

		private bool designMode;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

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
		/// <param name="frame">The frame used by the view, expressed in tvOS points.</param>
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
			designMode = ((IComponent)this).Site?.DesignMode == true || !EnvironmentExtensions.IsValidEnvironment;

			if (designMode)
				return;

			// create the GL context
			Context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
			DrawableColorFormat = GLKViewDrawableColorFormat.RGBA8888;
			DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;
			DrawableStencilFormat = GLKViewDrawableStencilFormat.Format8;
			DrawableMultisample = GLKViewDrawableMultisample.Sample4x;

			// hook up the drawing 
			Delegate = this;
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
		/// Draws the view within the passed-in rectangle.
		/// </summary>
		/// <param name="view">The view to draw on.</param>
		/// <param name="rect">The rectangle to draw.</param>
		public new void DrawInRect(GLKView view, CGRect rect)
		{
			if (designMode)
				return;

			// create the contexts if not done already
			if (context == null)
			{
				var glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);
			}

			// get the new surface size
			var newSize = new SKSizeI((int)DrawableWidth, (int)DrawableHeight);

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
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
			}

			// flush the SkiaSharp contents to GL
			canvas.Flush();
			context.Flush();
		}

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKGLView.OnPaintSurface(SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKGLView.PaintSurface" />
		/// event.
		/// ## Examples
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
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

		/// <summary>
		/// Gets or sets the view's frame rectangle.
		/// </summary>
		public override CGRect Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				SetNeedsDisplay();
			}
		}
	}
}

#endif

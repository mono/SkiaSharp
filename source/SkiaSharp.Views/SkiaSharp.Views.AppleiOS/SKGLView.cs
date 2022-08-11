#if !__WATCHOS__ && !__MACCATALYST__

using System;
using System.ComponentModel;
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
	[Register(nameof(SKGLView))]
	[DesignTimeVisible(true)]
#if HAS_UNO
	internal
#else
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
		public SKGLView()
		{
			Initialize();
		}

		// created in code
		public SKGLView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		public SKGLView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			designMode = ((IComponent)this).Site?.DesignMode == true || !Extensions.IsValidEnvironment;

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

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

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
#pragma warning disable CS0618 // Type or member is obsolete
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType, glInfo);
				OnPaintSurface(e);
				DrawInSurface(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// flush the SkiaSharp contents to GL
			canvas.Flush();
			context.Flush();
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}

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

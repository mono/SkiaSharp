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
			WantsBestResolutionOpenGLSurface = true;

			// Match C++ sk_app pixel format attributes exactly
			var attrs = new NSOpenGLPixelFormatAttribute[]
			{
				NSOpenGLPixelFormatAttribute.Accelerated,
				NSOpenGLPixelFormatAttribute.ClosestPolicy,
				NSOpenGLPixelFormatAttribute.DoubleBuffer,
				NSOpenGLPixelFormatAttribute.OpenGLProfile, (NSOpenGLPixelFormatAttribute)NSOpenGLProfile.Version3_2Core,
				NSOpenGLPixelFormatAttribute.ColorSize, (NSOpenGLPixelFormatAttribute)24,
				NSOpenGLPixelFormatAttribute.AlphaSize, (NSOpenGLPixelFormatAttribute)8,
				NSOpenGLPixelFormatAttribute.DepthSize, (NSOpenGLPixelFormatAttribute)0,
				NSOpenGLPixelFormatAttribute.StencilSize, (NSOpenGLPixelFormatAttribute)8,
				NSOpenGLPixelFormatAttribute.SampleBuffers, (NSOpenGLPixelFormatAttribute)0,
				(NSOpenGLPixelFormatAttribute)0,
			};
			PixelFormat = new NSOpenGLPixelFormat(attrs);
		}

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		public override void PrepareOpenGL()
		{
			base.PrepareOpenGL();

			// Disable VSync to match C++ behavior (allows 200+ FPS)
			var swapInterval = 0;
			OpenGLContext.SetValues(swapInterval, NSOpenGLContextParameter.SwapInterval);

			// create the context
			var glInterface = GRGlInterface.Create();
			context = GRContext.CreateGl(glInterface);
		}

		public override void Reshape()
		{
			base.Reshape();

			// get the new surface size
			var size = ConvertSizeToBacking(Bounds.Size);
			newSize = new SKSizeI((int)size.Width, (int)size.Height);
		}

		private nfloat lastBackingScaleFactor = 0;

		// Direct render method for tight loops (matches C++ onPaint pattern)
		public void RenderDirect()
		{
			if (OpenGLContext == null || context == null)
				return;

			// Make GL context current
			OpenGLContext.MakeCurrentContext();

			// Track scale changes
			if (Window != null && lastBackingScaleFactor != Window.BackingScaleFactor)
			{
				lastBackingScaleFactor = Window.BackingScaleFactor;
				Reshape();
			}

			RenderFrame();
		}

		private void RenderFrame()
		{
			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// create the render target - only recreate on size change (match C++ getBackbufferSurface)
			if (renderTarget == null || lastSize != newSize)
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
				
				// create the surface immediately (match C++ getBackbufferSurface pattern)
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			if (canvas == null)
				return;

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
				OnPaintSurface(e);
			}

			// flush the SkiaSharp contents to GL (match C++ onSwapBuffers)
			canvas.Flush();
			context.Flush(surface);
			OpenGLContext.FlushBuffer();
		}

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
			RenderFrame();
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}
	}
}

using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.Mac
{
	[Register(nameof(SKGLView))]
	[DesignTimeVisible(true)]
	public class SKGLView : NSOpenGLView
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext context;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

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

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		public GRContext GRContext => context;

		public override void PrepareOpenGL()
		{
			base.PrepareOpenGL();

			// create the context
			var glInterface = GRGlInterface.CreateNativeGlInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT | Gles.GL_DEPTH_BUFFER_BIT | Gles.GL_STENCIL_BUFFER_BIT);

			// manage the drawing surface
			var size = ConvertSizeToBacking(Bounds.Size);
			if (renderTarget == null || surface == null || renderTarget.Width != size.Width || renderTarget.Height != size.Height)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
				renderTarget = new GRBackendRenderTarget((int)size.Width, (int)size.Height, samples, stencil, glInfo);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
#pragma warning disable CS0618 // Type or member is obsolete
				DrawInSurface(e.Surface, e.RenderTarget);
#pragma warning restore CS0618 // Type or member is obsolete
				OnPaintSurface(e);
			}

			// flush the SkiaSharp contents to GL
			surface.Canvas.Flush();
			context.Flush();

			OpenGLContext.FlushBuffer();
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[Obsolete("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, GRBackendRenderTargetDesc renderTarget)
		{
		}
	}
}

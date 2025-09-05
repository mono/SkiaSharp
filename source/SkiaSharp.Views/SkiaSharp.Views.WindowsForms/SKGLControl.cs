using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
#if WINDOWS
using OpenTK.GLControl;
#endif
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

namespace SkiaSharp.Views.Desktop
{
	/// <summary>
	/// A hardware-accelerated control that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKGLControl : GLControl
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private bool designMode;

		private GRContext grContext;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

#if WINDOWS
		public SKGLControl()
			: base(new GLControlSettings { AlphaBits = 8, RedBits = 8, GreenBits = 8, BlueBits = 8, DepthBits = 24, StencilBits = 8 })
		{
			Initialize();
		}

		public SKGLControl(GLControlSettings settings)
			: base(settings)
		{
			Initialize();
		}
#else
		/// <summary>
		/// Creates a new instance of the <see cref="SKGLControl" /> view.
		/// </summary>
		public SKGLControl()
			: base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8))
		{
			Initialize();
		}

		public SKGLControl(GraphicsMode mode)
			: base(mode)
		{
			Initialize();
		}

		public SKGLControl(GraphicsMode mode, int major, int minor, GraphicsContextFlags flags)
			: base(mode, major, minor, flags)
		{
			Initialize();
		}
#endif

		private void Initialize()
		{
			designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

			ResizeRedraw = true;
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => lastSize;

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext GRContext => grContext;

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Desktop.SKGLControl.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Desktop.SKGLControl.PaintSurface" />
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
		/// ```</remarks>
		[Category("Appearance")]
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected override void OnPaint(PaintEventArgs e)
		{
			if (designMode)
			{
				e.Graphics.Clear(BackColor);
				return;
			}

			base.OnPaint(e);

			MakeCurrent();

			// create the contexts if not done already
			if (grContext == null)
			{
				var glInterface = GRGlInterface.Create();
				grContext = GRContext.CreateGl(glInterface);
			}

			// get the new surface size
			var newSize = new SKSizeI(Width, Height);

			// manage the drawing surface
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
				GL.GetInteger(GetPName.StencilBits, out var stencil);
				GL.GetInteger(GetPName.Samples, out var samples);
				var maxSamples = grContext.GetMaxSurfaceSampleCount(colorType);
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
				surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
			}

			// update the control
			canvas.Flush();
			SwapBuffers();
		}

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// clean up
			canvas = null;
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			grContext?.Dispose();
			grContext = null;
		}
	}
}

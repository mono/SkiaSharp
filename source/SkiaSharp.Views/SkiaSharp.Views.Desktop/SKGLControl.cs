using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace SkiaSharp.Views.Desktop
{
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKGLControl : GLControl
	{
		private bool designMode;

		private GRContext grContext;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;

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

		private void Initialize()
		{
			designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

			ResizeRedraw = true;
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public GRContext GRContext => grContext;

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

			// create the contexts if not done already
			if (grContext == null)
			{
				var glInterface = GRGlInterface.CreateNativeGlInterface();
				grContext = GRContext.Create(GRBackend.OpenGL, glInterface);
			}

			// manage the drawing surface
			if (renderTarget == null || surface == null || renderTarget.Width != Width || renderTarget.Height != Height)
			{
				// create or update the dimensions
				renderTarget?.Dispose();
				renderTarget = SKGLDrawable.CreateRenderTarget(Width, Height);

				// create the surface
				surface?.Dispose();
				surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
			}

			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888));
			}

			// update the control
			surface.Canvas.Flush();
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
			surface?.Dispose();
			surface = null;
			renderTarget?.Dispose();
			renderTarget = null;
			grContext?.Dispose();
			grContext = null;
		}
	}
}

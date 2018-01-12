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
		private GRBackendRenderTargetDesc renderTarget;

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

				// get initial details
				renderTarget = SKGLDrawable.CreateRenderTarget();
			}

			// update to the latest dimensions
			renderTarget.Width = Width;
			renderTarget.Height = Height;

			// create the surface
			using (var surface = SKSurface.Create(grContext, renderTarget))
			{
				// start drawing
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));

				surface.Canvas.Flush();
			}

			// update the control
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
			if (grContext != null)
			{
				grContext.Dispose();
				grContext = null;
			}
		}
	}
}

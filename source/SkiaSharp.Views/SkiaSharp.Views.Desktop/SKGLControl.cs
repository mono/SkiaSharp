using System;
using System.ComponentModel;
using System.Windows.Forms;
using OpenTK;

namespace SkiaSharp.Views.Desktop
{
	public class SKGLControl : GLControl
	{
		private readonly bool designMode;

		private GRContext grContext;
		private GRBackendRenderTargetDesc renderTarget;

		public SKGLControl()
		{
			designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

			ResizeRedraw = true;
		}

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

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

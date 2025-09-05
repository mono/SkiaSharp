using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SkiaSharp.Views.Desktop
{
	/// <summary>
	/// A control that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKControl : Control
	{
		private readonly bool designMode;

		private Bitmap bitmap;

		/// <summary>
		/// Creates a new instance of the <see cref="SKControl" /> view.
		/// </summary>
		public SKControl()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.ResizeRedraw, true);

			designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.Width, bitmap.Height);

		/// <summary>
		/// Occurs when the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Desktop.SKControl.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Desktop.SKControl.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// </code>
		/// </example>
		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override void OnPaint(PaintEventArgs e)
		{
			if (designMode)
				return;

			base.OnPaint(e);

			// get the bitmap
			var info = CreateBitmap();

			if (info.Width == 0 || info.Height == 0)
				return;

			var data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

			// create the surface
			using (var surface = SKSurface.Create(info, data.Scan0, data.Stride))
			{
				// start drawing
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

				surface.Canvas.Flush();
			}

			// write the bitmap to the graphics
			bitmap.UnlockBits(data);
			e.Graphics.DrawImage(bitmap, 0, 0);
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			FreeBitmap();
		}

		private SKImageInfo CreateBitmap()
		{
			var info = new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (bitmap == null || bitmap.Width != info.Width || bitmap.Height != info.Height)
			{
				FreeBitmap();

				if (info.Width != 0 && info.Height != 0)
					bitmap = new Bitmap(info.Width, info.Height, PixelFormat.Format32bppPArgb);
			}

			return info;
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}

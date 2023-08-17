using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace SkiaSharp.Views.Mac
{
	[Register(nameof(SKCanvasView))]
	[DesignTimeVisible(true)]
	public class SKCanvasView : NSView
	{
		private SKCGSurfaceFactory drawable;
		private bool ignorePixelScaling;

		// created in code
		public SKCanvasView()
		{
			Initialize();
		}

		// created in code
		public SKCanvasView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		// created via designer
		public SKCanvasView(IntPtr p)
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
			drawable = new SKCGSurfaceFactory();
		}

		public SKSize CanvasSize { get; private set; }

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				NeedsDisplay = true;
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			// create the skia context
			using (var surface = drawable.CreateSurface(Bounds, Window.BackingScaleFactor, out var info))
			{
				if (info.Width == 0 || info.Height == 0)
				{
					CanvasSize = SKSize.Empty;
					return;
				}

				var userVisibleSize = IgnorePixelScaling
					? new SKSizeI((int)Bounds.Width, (int)Bounds.Height)
					: info.Size;

				CanvasSize = userVisibleSize;

				if (IgnorePixelScaling)
				{
					var skiaCanvas = surface.Canvas;
					skiaCanvas.Scale((float)Window.BackingScaleFactor);
					skiaCanvas.Save();
				}

				using (var ctx = NSGraphicsContext.CurrentContext.CGContext)
				{
					// draw on the image using SKiaSharp
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

					// draw the surface to the context
					drawable.DrawSurface(ctx, Bounds, info, surface);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

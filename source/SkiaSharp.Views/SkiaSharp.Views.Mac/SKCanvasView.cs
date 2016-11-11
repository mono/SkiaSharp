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
		private SKDrawable drawable;
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
			drawable = new SKDrawable();
		}

		public SKSize CanvasSize => drawable.Info.Size;

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				NeedsDisplay = true;
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			PaintSurface?.Invoke(this, new SKPaintSurfaceEventArgs(surface, info));
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			var ctx = NSGraphicsContext.CurrentContext.CGContext;

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : Window.BackingScaleFactor, out info);

			// draw on the image using SKiaSharp
			DrawInSurface(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

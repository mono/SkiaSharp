using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SkiaSharp.Views
{
	[Register(nameof(SKCanvasView))]
	[DesignTimeVisible(true)]
	public class SKCanvasView : UIView, IComponent
	{
		// for IComponent
		public ISite Site { get; set; }
		public event EventHandler Disposed;
		private bool designMode;

		private SKDrawable drawable;

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
			designMode = Site?.DesignMode == true;

			Initialize();
		}

		private void Initialize()
		{
			drawable = new SKDrawable();
		}

		public override void Draw(CGRect rect)
		{
			base.Draw(rect);

			var ctx = UIGraphics.GetCurrentContext();

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, ContentScaleFactor, out info);

			// draw on the image using SKiaSharp
			DrawInSurface(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			PaintSurface?.Invoke(this, new SKPaintSurfaceEventArgs(surface, info));
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			Layer.SetNeedsDisplay();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

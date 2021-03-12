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

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			// create the skia context
			using (var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : Window.BackingScaleFactor, out var info))
			{
				if (info.Width == 0 || info.Height == 0)
					return;

				using (var ctx = NSGraphicsContext.CurrentContext.CGContext)
				{
					// draw on the image using SKiaSharp
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
#pragma warning disable CS0618 // Type or member is obsolete
					DrawInSurface(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

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

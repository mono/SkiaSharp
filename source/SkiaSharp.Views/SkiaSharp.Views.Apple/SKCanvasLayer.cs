using System;
using CoreAnimation;
using CoreGraphics;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	public class SKCanvasLayer : CALayer
	{
		private readonly SKDrawable drawable;
		private bool ignorePixelScaling;

		public SKCanvasLayer()
		{
			drawable = new SKDrawable();

			SetNeedsDisplay();
			NeedsDisplayOnBoundsChange = true;
		}

		public ISKCanvasLayerDelegate SKDelegate { get; set; }

		public SKSize CanvasSize => drawable.Info.Size;

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				SetNeedsDisplay();
			}
		}

		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : ContentsScale, out info);

			// draw on the image using SKiaSharp
			DrawInSurface(surface, info);
			SKDelegate?.DrawInSurface(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			PaintSurface?.Invoke(this, new SKPaintSurfaceEventArgs(surface, info));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

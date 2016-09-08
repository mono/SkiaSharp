using CoreAnimation;
using CoreGraphics;

namespace SkiaSharp.Views
{
	public class SKLayer : CALayer
	{
		private readonly SKDrawable drawable;

		public SKLayer()
		{
			drawable = new SKDrawable();

			SetNeedsDisplay();
			NeedsDisplayOnBoundsChange = true;
		}

		public ISKLayerDelegate SKDelegate { get; set; }

		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);

			// create the skia context
			SKImageInfo info;
			var surface = drawable.CreateSurface(Bounds, ContentsScale, out info);

			// draw on the image using SKiaSharp
			DrawInSurface(surface, info);
			SKDelegate?.DrawInSurface(surface, info);

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public virtual void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

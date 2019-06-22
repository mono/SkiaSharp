#if !__WATCHOS__

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
		private readonly SKCGSurfaceFactory drawable;
		private bool ignorePixelScaling;

		public SKCanvasLayer()
		{
			drawable = new SKCGSurfaceFactory();

			SetNeedsDisplay();
			NeedsDisplayOnBoundsChange = true;
		}

		[Obsolete("Use PaintSurface instead.")]
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
			using (var surface = drawable.CreateSurface(Bounds, IgnorePixelScaling ? 1 : ContentsScale, out var info))
			{
				if (info.Width == 0 || info.Height == 0)
					return;

				// draw on the image using SKiaSharp
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
#pragma warning disable CS0618 // Type or member is obsolete
				DrawInSurface(surface, info);
				SKDelegate?.DrawInSurface(surface, info);
#pragma warning restore CS0618 // Type or member is obsolete

				// draw the surface to the context
				drawable.DrawSurface(ctx, Bounds, info, surface);
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[Obsolete("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
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

#endif

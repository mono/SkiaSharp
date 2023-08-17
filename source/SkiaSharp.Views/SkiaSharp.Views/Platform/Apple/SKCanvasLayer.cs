using System;
using System.ComponentModel;
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

		public SKSize CanvasSize { get; private set; }

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
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
			using var surface = drawable.CreateSurface(Bounds, ContentsScale, out var info);

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
				skiaCanvas.Scale((float)ContentsScale);
				skiaCanvas.Save();
			}

			// draw on the image using SKiaSharp
			OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));

			// draw the surface to the context
			drawable.DrawSurface(ctx, Bounds, info, surface);
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			drawable.Dispose();
		}
	}
}

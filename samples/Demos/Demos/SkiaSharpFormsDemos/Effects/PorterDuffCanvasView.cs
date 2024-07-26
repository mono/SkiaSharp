using System;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    class PorterDuffCanvasView : SKCanvasView
    {
        static SKBitmap srcBitmap, dstBitmap;

        static PorterDuffCanvasView()
        {
            dstBitmap = new SKBitmap(300, 300);
            srcBitmap = new SKBitmap(300, 300);

            using (SKPaint paint = new SKPaint())
            {
                using (SKCanvas canvas = new SKCanvas(dstBitmap))
                {
                    canvas.Clear();
                    paint.Color = new SKColor(0xC0, 0x80, 0x00);
                    canvas.DrawRect(new SKRect(0, 0, 200, 200), paint);
                }
                using (SKCanvas canvas = new SKCanvas(srcBitmap))
                {
                    canvas.Clear();
                    paint.Color = new SKColor(0x00, 0x80, 0xC0);
                    canvas.DrawRect(new SKRect(100, 100, 300, 300), paint);
                }
            }
        }

        SKBlendMode blendMode;

        public PorterDuffCanvasView(SKBlendMode blendMode)
        {
            this.blendMode = blendMode;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Find largest square that fits
            float rectSize = Math.Min(info.Width, info.Height);
            float x = (info.Width - rectSize) / 2;
            float y = (info.Height - rectSize) / 2;
            SKRect rect = new SKRect(x, y, x + rectSize, y + rectSize);

            // Draw destination bitmap
            canvas.DrawBitmap(dstBitmap, rect);

            // Draw source bitmap
            using (SKPaint paint = new SKPaint())
            {
                paint.BlendMode = blendMode;
                canvas.DrawBitmap(srcBitmap, rect, paint);
            }

            // Draw outline
            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 2;
                rect.Inflate(-1, -1);
                canvas.DrawRect(rect, paint);
            }
        }
    }
}

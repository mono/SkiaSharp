using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class HelloBitmapPage : ContentPage
    {
        const string TEXT = "Hello, Bitmap!";
        SKBitmap helloBitmap;

        public HelloBitmapPage()
        {
            Title = TEXT;

            // Create bitmap and draw on it
            using (SKPaint textPaint = new SKPaint { TextSize = 48 })
            {
                SKRect bounds = new SKRect();
                textPaint.MeasureText(TEXT, ref bounds);

                helloBitmap = new SKBitmap((int)bounds.Right,
                                           (int)bounds.Height);

                using (SKCanvas bitmapCanvas = new SKCanvas(helloBitmap))
                {
                    bitmapCanvas.Clear();
                    bitmapCanvas.DrawText(TEXT, 0, -bounds.Top, textPaint);
                }
            }

            // Create SKCanvasView to view result
            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Aqua);

            for (float y = 0; y < info.Height; y += helloBitmap.Height)
                for (float x = 0; x < info.Width; x += helloBitmap.Width)
                {
                    canvas.DrawBitmap(helloBitmap, x, y);
                }
        }
    }
}
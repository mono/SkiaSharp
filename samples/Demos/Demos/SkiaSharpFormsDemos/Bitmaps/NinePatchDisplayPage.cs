using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public class NinePatchDisplayPage : ContentPage
    {
        static NinePatchDisplayPage()
        {
            using (SKCanvas canvas = new SKCanvas(FiveByFiveBitmap))
            using (SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 10
            })
            {
                for (int x = 50; x < 500; x += 100)
                    for (int y = 50; y < 500; y += 100)
                    {
                        canvas.DrawCircle(x, y, 40, paint);
                    }
            }
        }

        public static SKBitmap FiveByFiveBitmap { get; } = new SKBitmap(500, 500);

        public NinePatchDisplayPage()
        {
            Title = "Nine-Patch Display";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRectI centerRect = new SKRectI(100, 100, 400, 400);
            canvas.DrawBitmapNinePatch(FiveByFiveBitmap, centerRect, info.Rect);
        }
    }
}
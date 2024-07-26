using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class PathDataCatPage : ContentPage
    {
        SKPath catPath = SKPath.ParseSvgPathData(
            "M 160 140 L 150 50 220 103" +              // Left ear
            "M 320 140 L 330 50 260 103" +              // Right ear
            "M 215 230 L 40 200" +                      // Left whiskers
            "M 215 240 L 40 240" +
            "M 215 250 L 40 280" +
            "M 265 230 L 440 200" +                     // Right whiskers
            "M 265 240 L 440 240" +
            "M 265 250 L 440 280" +
            "M 240 100" +                               // Head
            "A 100 100 0 0 1 240 300" +
            "A 100 100 0 0 1 240 100 Z" +
            "M 180 170" +                               // Left eye
            "A 40 40 0 0 1 220 170" +
            "A 40 40 0 0 1 180 170 Z" +
            "M 300 170" +                               // Right eye
            "A 40 40 0 0 1 260 170" +
            "A 40 40 0 0 1 300 170 Z");

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Orange,
            StrokeWidth = 5
        };

        public PathDataCatPage()
        {
            Title = "Path Data Cat";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            SKRect bounds;
            catPath.GetBounds(out bounds);

            canvas.Translate(info.Width / 2, info.Height / 2);

            canvas.Scale(0.9f * Math.Min(info.Width / bounds.Width,
                                         info.Height / bounds.Height));

            canvas.Translate(-bounds.MidX, -bounds.MidY);

            canvas.DrawPath(catPath, paint);
        }
    }
}
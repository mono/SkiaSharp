using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class PathDataHelloPage : ContentPage
    {
        SKPath helloPath = SKPath.ParseSvgPathData(
            "M 0 0 L 0 100 M 0 50 L 50 50 M 50 0 L 50 100" +                // H
            "M 125 0 C 60 -10, 60 60, 125 50, 60 40, 60 110, 125 100" +     // E
            "M 150 0 L 150 100, 200 100" +                                  // L
            "M 225 0 L 225 100, 275 100" +                                  // L
            "M 300 50 A 25 50 0 1 0 300 49.9 Z");                           // O

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public PathDataHelloPage()
        {
            Title = "Path Data Hello";

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

            SKRect bounds;
            helloPath.GetTightBounds(out bounds);

            canvas.Translate(info.Width / 2, info.Height / 2);

            canvas.Scale(info.Width / (bounds.Width + paint.StrokeWidth),
                         info.Height / (bounds.Height + paint.StrokeWidth));

            canvas.Translate(-bounds.MidX, -bounds.MidY);

            canvas.DrawPath(helloPath, paint);
        }
    }
}
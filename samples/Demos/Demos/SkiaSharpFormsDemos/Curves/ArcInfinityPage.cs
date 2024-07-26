using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class ArcInfinityPage : ContentPage
    {
        public ArcInfinityPage()
        {
            Title = "Arc Infinity";

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

            using (SKPath path = new SKPath())
            {
                path.LineTo(83, 75);
                path.ArcTo(100, 100, 0, SKPathArcSize.Large, SKPathDirection.CounterClockwise, 83, -75);
                path.LineTo(-83, 75);
                path.ArcTo(100, 100, 0, SKPathArcSize.Large, SKPathDirection.Clockwise, -83, -75);
                path.Close();

                // Use path.TightBounds for coordinates without control points
                SKRect pathBounds = path.Bounds;

                canvas.Translate(info.Width / 2, info.Height / 2);
                canvas.Scale(Math.Min(info.Width / pathBounds.Width, 
                                      info.Height / pathBounds.Height));

                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Blue;
                    paint.StrokeWidth = 5;

                    canvas.DrawPath(path, paint);
                }
            }
        }
    }
}
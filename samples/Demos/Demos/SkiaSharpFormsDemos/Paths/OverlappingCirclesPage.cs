using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Paths
{
    public class OverlappingCirclesPage : ContentPage
    {
        public OverlappingCirclesPage()
        {
            Title = "Overlapping Circles";

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

            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);
            float radius = Math.Min(info.Width, info.Height) / 4;

            SKPath path = new SKPath
            {
                FillType = SKPathFillType.EvenOdd
            };

            path.AddCircle(center.X - radius / 2, center.Y - radius / 2, radius);
            path.AddCircle(center.X - radius / 2, center.Y + radius / 2, radius);
            path.AddCircle(center.X + radius / 2, center.Y - radius / 2, radius);
            path.AddCircle(center.X + radius / 2, center.Y + radius / 2, radius);

            SKPaint paint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Cyan
            };

            canvas.DrawPath(path, paint);

            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 10;
            paint.Color = SKColors.Magenta;

            canvas.DrawPath(path, paint);
        }
    }
}
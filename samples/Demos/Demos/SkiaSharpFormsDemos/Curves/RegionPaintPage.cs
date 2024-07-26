using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class RegionPaintPage : ContentPage
    {
        public RegionPaintPage()
        {
            Title = "Region Paint";

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

            int radius = 10;

            // Create circular path
            using (SKPath circlePath = new SKPath())
            {
                circlePath.AddCircle(0, 0, radius);

                // Create circular region
                using (SKRegion circleRegion = new SKRegion())
                {
                    circleRegion.SetRect(new SKRectI(-radius, -radius, radius, radius));
                    circleRegion.SetPath(circlePath);

                    // Set transform to move it to center and scale up
                    canvas.Translate(info.Width / 2, info.Height / 2);
                    canvas.Scale(Math.Min(info.Width / 2, info.Height / 2) / radius);

                    // Fill region
                    using (SKPaint fillPaint = new SKPaint())
                    {
                        fillPaint.Style = SKPaintStyle.Fill;
                        fillPaint.Color = SKColors.Orange;

                        canvas.DrawRegion(circleRegion, fillPaint);
                    }

                    // Stroke path for comparison
                    using (SKPaint strokePaint = new SKPaint())
                    {
                        strokePaint.Style = SKPaintStyle.Stroke;
                        strokePaint.Color = SKColors.Blue;
                        strokePaint.StrokeWidth = 0.1f;

                        canvas.DrawPath(circlePath, strokePaint);
                    }
                }
            }
        }
    }
}

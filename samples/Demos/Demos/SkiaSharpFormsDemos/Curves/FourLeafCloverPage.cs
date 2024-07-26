using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class FourLeafCloverPage : ContentPage
    {
        public FourLeafCloverPage()
        {
            Title = "Four-Leaf Clover";

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

            float xCenter = info.Width / 2;
            float yCenter = info.Height / 2;
            float radius = 0.24f * Math.Min(info.Width, info.Height);

            using (SKRegion wholeScreenRegion = new SKRegion())
            {
                wholeScreenRegion.SetRect(new SKRectI(0, 0, info.Width, info.Height));

                using (SKRegion leftRegion = new SKRegion(wholeScreenRegion))
                using (SKRegion rightRegion = new SKRegion(wholeScreenRegion))
                using (SKRegion topRegion = new SKRegion(wholeScreenRegion))
                using (SKRegion bottomRegion = new SKRegion(wholeScreenRegion))
                {
                    using (SKPath circlePath = new SKPath())
                    {
                        // Make basic circle path
                        circlePath.AddCircle(xCenter, yCenter, radius);

                        // Left leaf
                        circlePath.Transform(SKMatrix.MakeTranslation(-radius, 0));
                        leftRegion.SetPath(circlePath);

                        // Right leaf
                        circlePath.Transform(SKMatrix.MakeTranslation(2 * radius, 0));
                        rightRegion.SetPath(circlePath);

                        // Make union of right with left
                        leftRegion.Op(rightRegion, SKRegionOperation.Union);

                        // Top leaf
                        circlePath.Transform(SKMatrix.MakeTranslation(-radius, -radius));
                        topRegion.SetPath(circlePath);

                        // Combine with bottom leaf
                        circlePath.Transform(SKMatrix.MakeTranslation(0, 2 * radius));
                        bottomRegion.SetPath(circlePath);

                        // Make union of top with bottom
                        bottomRegion.Op(topRegion, SKRegionOperation.Union);

                        // Exclusive-OR left and right with top and bottom
                        leftRegion.Op(bottomRegion, SKRegionOperation.XOR);

                        // Set that as clip region
                        canvas.ClipRegion(leftRegion);

                        // Set transform for drawing lines from center
                        canvas.Translate(xCenter, yCenter);

                        // Draw 360 lines
                        for (double angle = 0; angle < 360; angle++)
                        {
                            float x = 2 * radius * (float)Math.Cos(Math.PI * angle / 180);
                            float y = 2 * radius * (float)Math.Sin(Math.PI * angle / 180);

                            using (SKPaint strokePaint = new SKPaint())
                            {
                                strokePaint.Color = SKColors.Green;
                                strokePaint.StrokeWidth = 2;

                                canvas.DrawLine(0, 0, x, y, strokePaint);
                            }
                        }
                    }
                }
            }
        }
    }
}

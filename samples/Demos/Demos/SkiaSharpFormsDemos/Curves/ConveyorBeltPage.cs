using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class ConveyorBeltPage : ContentPage
    {
        SKCanvasView canvasView;
        bool pageIsActive = false;

        SKPaint conveyerPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 20,
            Color = SKColors.DarkGray
        };

        SKPath bucketPath = new SKPath();

        SKPaint bucketsPaint = new SKPaint
        {
            Color = SKColors.BurlyWood,
        };

        public ConveyorBeltPage()
        {
            Title = "Conveyor Belt";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Create the path for the bucket starting with the handle
            bucketPath.AddRect(new SKRect(-5, -3, 25, 3));

            // Sides
            bucketPath.AddRoundedRect(new SKRect(25, -19, 27, 18), 10, 10, 
                                      SKPathDirection.CounterClockwise);
            bucketPath.AddRoundedRect(new SKRect(63, -19, 65, 18), 10, 10, 
                                      SKPathDirection.CounterClockwise);

            // Five slats
            for (int i = 0; i < 5; i++)
            {
                bucketPath.MoveTo(25, -19 + 8 * i);
                bucketPath.LineTo(25, -13 + 8 * i);
                bucketPath.ArcTo(50, 50, 0, SKPathArcSize.Small, 
                                 SKPathDirection.CounterClockwise, 65, -13 + 8 * i);
                bucketPath.LineTo(65, -19 + 8 * i);
                bucketPath.ArcTo(50, 50, 0, SKPathArcSize.Small, 
                                 SKPathDirection.Clockwise, 25, -19 + 8 * i);
                bucketPath.Close();
            }

            // Arc to suggest the hidden side
            bucketPath.MoveTo(25, -17);
            bucketPath.ArcTo(50, 50, 0, SKPathArcSize.Small, 
                                SKPathDirection.Clockwise, 65, -17);
            bucketPath.LineTo(65, -19);
            bucketPath.ArcTo(50, 50, 0, SKPathArcSize.Small, 
                                SKPathDirection.CounterClockwise, 25, -19);
            bucketPath.Close();

            // Make it a little bigger and correct the orientation
            bucketPath.Transform(SKMatrix.MakeScale(-2, 2));
            bucketPath.Transform(SKMatrix.MakeRotationDegrees(90));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;

            Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
            {
                canvasView.InvalidateSurface();
                return pageIsActive;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float width = info.Width / 3;
            float verticalMargin = width / 2 + 150;

            using (SKPath conveyerPath = new SKPath())
            {
                // Straight verticals capped by semicircles on top and bottom
                conveyerPath.MoveTo(width, verticalMargin);
                conveyerPath.ArcTo(width / 2, width / 2, 0, SKPathArcSize.Large, 
                                   SKPathDirection.Clockwise, 2 * width, verticalMargin);
                conveyerPath.LineTo(2 * width, info.Height - verticalMargin);
                conveyerPath.ArcTo(width / 2, width / 2, 0, SKPathArcSize.Large, 
                                   SKPathDirection.Clockwise, width, info.Height - verticalMargin);
                conveyerPath.Close();

                // Draw the conveyor belt itself
                canvas.DrawPath(conveyerPath, conveyerPaint);

                // Calculate spacing based on length of conveyer path
                float length = 2 * (info.Height - 2 * verticalMargin) +
                               2 * ((float)Math.PI * width / 2);

                // Value will be somewhere around 200
                float spacing = length / (float)Math.Round(length / 200);

                // Now animate the phase; t is 0 to 1 every 2 seconds
                TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
                float t = (float)(timeSpan.TotalSeconds % 2 / 2);
                float phase = -t * spacing;

                // Create the buckets PathEffect
                using (SKPathEffect bucketsPathEffect = 
                            SKPathEffect.Create1DPath(bucketPath, spacing, phase, 
                                                      SKPath1DPathEffectStyle.Rotate))
                {
                    // Set it to the Paint object and draw the path again
                    bucketsPaint.PathEffect = bucketsPathEffect;
                    canvas.DrawPath(conveyerPath, bucketsPaint);
                }
            }
        }
    }
}
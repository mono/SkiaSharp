using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class PrettyAnalogClockPage : ContentPage
    {
        SKCanvasView canvasView;
        bool pageIsActive;

        // Clock hands pointing straight up
        SKPath hourHandPath = SKPath.ParseSvgPathData(
            "M 0 -60 C   0 -30 20 -30  5 -20 L  5   0" +
                    "C   5 7.5 -5 7.5 -5   0 L -5 -20" +
                    "C -20 -30  0 -30  0 -60 Z");

        SKPath minuteHandPath = SKPath.ParseSvgPathData(
            "M 0 -80 C   0 -75  0 -70  2.5 -60 L  2.5   0" +
                    "C   2.5 5 -2.5 5 -2.5   0 L -2.5 -60" +
                    "C 0 -70  0 -75  0 -80 Z");

        SKPath secondHandPath = SKPath.ParseSvgPathData(
            "M 0 10 L 0 -80");

        // SKPaint objects
        SKPaint handStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 2,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint handFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Gray
        };

        SKPaint minuteMarkPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3,
            StrokeCap = SKStrokeCap.Round,
            PathEffect = SKPathEffect.CreateDash(new float[] { 0, 3 * 3.14159f }, 0)
        };

        SKPaint hourMarkPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 6,
            StrokeCap = SKStrokeCap.Round,
            PathEffect = SKPathEffect.CreateDash(new float[] { 0, 15 * 3.14159f }, 0)
        };

        public PrettyAnalogClockPage()
        {
            Title = "Pretty Analog Clock";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
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

            // Transform for 100-radius circle in center
            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(Math.Min(info.Width / 200, info.Height / 200));

            // Draw circles for hour and minute marks
            SKRect rect = new SKRect(-90, -90, 90, 90);
            canvas.DrawOval(rect, minuteMarkPaint);
            canvas.DrawOval(rect, hourMarkPaint);

            // Get time
            DateTime dateTime = DateTime.Now;

            // Draw hour hand
            canvas.Save();
            canvas.RotateDegrees(30 * dateTime.Hour + dateTime.Minute / 2f);
            canvas.DrawPath(hourHandPath, handStrokePaint);
            canvas.DrawPath(hourHandPath, handFillPaint);
            canvas.Restore();

            // Draw minute hand
            canvas.Save();
            canvas.RotateDegrees(6 * dateTime.Minute + dateTime.Second / 10f);
            canvas.DrawPath(minuteHandPath, handStrokePaint);
            canvas.DrawPath(minuteHandPath, handFillPaint);
            canvas.Restore();

            // Draw second hand
            double t = dateTime.Millisecond / 1000.0;

            if (t < 0.5)
            {
                t = 0.5 * Easing.SpringIn.Ease(t / 0.5);
            }
            else
            {
                t = 0.5 * (1 + Easing.SpringOut.Ease((t - 0.5) / 0.5));
            }

            canvas.Save();
            canvas.RotateDegrees(6 * (dateTime.Second + (float)t));
            canvas.DrawPath(secondHandPath, handStrokePaint);
            canvas.Restore();
        }
    }
}

using System;
using System.Diagnostics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public class CodeMoreCodePage : ContentPage
    {
        SKCanvasView canvasView;
        bool isAnimating;
        Stopwatch stopwatch = new Stopwatch();
        double transparency;

	    public CodeMoreCodePage ()
	    {
            Title = "Code More Code";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
	    }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            isAnimating = true;
            stopwatch.Start();
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            stopwatch.Stop();
            isAnimating = false;
        }

        bool OnTimerTick()
        {
            const int duration = 5;     // seconds
            double progress = stopwatch.Elapsed.TotalSeconds % duration / duration;
            transparency = 0.5 * (1 + Math.Sin(progress * 2 * Math.PI));
            canvasView.InvalidateSurface();

            return isAnimating;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            const string TEXT1 = "CODE";
            const string TEXT2 = "MORE";

            using (SKPaint paint = new SKPaint())
            using (SKFont font = new SKFont())
            {
                // Set text width to fit in width of canvas
                font.Size = 100;
                float textWidth = font.MeasureText(TEXT1);
                font.Size *= 0.9f * info.Width / textWidth;

                // Center first text string
                font.MeasureText(TEXT1, out SKRect textBounds);

                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                paint.Color = SKColors.Blue.WithAlpha((byte)(0xFF * (1 - transparency)));
                canvas.DrawText(TEXT1, xText, yText, SKTextAlign.Left, font, paint);

                // Center second text string
                font.MeasureText(TEXT2, out textBounds);

                xText = info.Width / 2 - textBounds.MidX;
                yText = info.Height / 2 - textBounds.MidY;

                paint.Color = SKColors.Blue.WithAlpha((byte)(0xFF * transparency));
                canvas.DrawText(TEXT2, xText, yText, SKTextAlign.Left, font, paint);
            }
        }
    }
}
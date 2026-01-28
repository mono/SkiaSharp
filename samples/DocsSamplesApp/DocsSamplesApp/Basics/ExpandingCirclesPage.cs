using System;
using System.Diagnostics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public class ExpandingCirclesPage : ContentPage
    {
        const double cycleTime = 1000;       // in milliseconds

        SKCanvasView canvasView;
        Stopwatch stopwatch = new Stopwatch();
        bool pageIsActive;
        float t;
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke
        };

        public ExpandingCirclesPage()
        {
            Title = "Expanding Circles";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;
            stopwatch.Start();

            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromMilliseconds(33), () =>
            {
                t = (float)(stopwatch.Elapsed.TotalMilliseconds % cycleTime / cycleTime);
                canvasView.InvalidateSurface();

                if (!pageIsActive)
                {
                    stopwatch.Stop();
                }
                return pageIsActive;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);
            float baseRadius = Math.Min(info.Width, info.Height) / 12;

            for (int circle = 0; circle < 5; circle++)
            {
                float radius = baseRadius * (circle + t);

                paint.StrokeWidth = baseRadius / 2 * (circle == 0 ? t : 1);
                paint.Color = new SKColor(0, 0, 255,
                    (byte)(255 * (circle == 4 ? (1 - t) : 1)));

                canvas.DrawCircle(center.X, center.Y, radius, paint);
            }
        }
    }
}
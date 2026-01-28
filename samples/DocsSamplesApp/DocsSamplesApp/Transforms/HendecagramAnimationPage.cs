using System;
using System.Diagnostics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Transforms
{
    public class HendecagramAnimationPage : ContentPage
    {
        const double cycleTime = 5000;      // in milliseconds

        SKCanvasView canvasView;
        Stopwatch stopwatch = new Stopwatch();
        bool pageIsActive;
        float angle;

        public HendecagramAnimationPage()
        {
            Title = "Hedecagram Animation";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;
            stopwatch.Start();

            // TODO Xamarin.Forms.Dispatcher.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(33), () =>
            {
                double t = stopwatch.Elapsed.TotalMilliseconds % cycleTime / cycleTime;
                angle = (float)(360 * t);
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
            canvas.Translate(info.Width / 2, info.Height / 2);
            float radius = (float)Math.Min(info.Width, info.Height) / 2 - 100;

            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = SKColor.FromHsl(angle, 100, 50);

                float x = radius * (float)Math.Sin(Math.PI * angle / 180);
                float y = -radius * (float)Math.Cos(Math.PI * angle / 180);
                canvas.Translate(x, y);
                canvas.DrawPath(HendecagramArrayPage.HendecagramPath, paint);
            }
        }
    }
}

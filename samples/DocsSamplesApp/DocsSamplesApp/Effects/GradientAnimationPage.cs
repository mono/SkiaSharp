using System;
using System.Diagnostics;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Effects
{
    public class GradientAnimationPage : ContentPage
    {
        SKCanvasView canvasView;
        bool isAnimating;
        double angle;
        Stopwatch stopwatch = new Stopwatch();

        public GradientAnimationPage()
        {
            Title = "Gradient Animation";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            isAnimating = true;
            stopwatch.Start();
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            stopwatch.Stop();
            isAnimating = false;
        }

        bool OnTimerTick()
        {
            const int duration = 3000;
            angle = 2 * Math.PI * (stopwatch.ElapsedMilliseconds % duration) / duration;
            canvasView.InvalidateSurface();

            return isAnimating;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(0, 0),
                                    info.Width < info.Height ? new SKPoint(info.Width, 0) : 
                                                               new SKPoint(0, info.Height),
                                    new SKColor[] { SKColors.White, SKColors.Black },
                                    null,
                                    SKShaderTileMode.Mirror,
                                    SKMatrix.CreateRotation((float)angle, info.Rect.MidX, info.Rect.MidY));

                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}

using System;
using System.Diagnostics;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
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
                                    SKMatrix.MakeRotation((float)angle, info.Rect.MidX, info.Rect.MidY));

                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}

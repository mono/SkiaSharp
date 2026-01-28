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
	public class AnimatedBitmapTilePage : ContentPage
	{
        const int SIZE = 64;

        SKCanvasView canvasView;
        SKBitmap bitmap = new SKBitmap(SIZE, SIZE);
        float angle;

        // For animation
        bool isAnimating;
        Stopwatch stopwatch = new Stopwatch();

		public AnimatedBitmapTilePage ()
		{
            Title = "Animated Bitmap Tile";

            // Initialize bitmap prior to animation
            DrawBitmap();

            // Create SKCanvasView 
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
            const int duration = 10;     // seconds
            angle = (float)(360f * (stopwatch.Elapsed.TotalSeconds % duration) / duration);
            DrawBitmap();
            canvasView.InvalidateSurface();

            return isAnimating;
        }

        void DrawBitmap()
        {
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Blue;
                paint.StrokeWidth = SIZE / 8;

                canvas.Clear();
                canvas.Translate(SIZE / 2, SIZE / 2);
                canvas.RotateDegrees(angle);
                canvas.DrawLine(-SIZE, -SIZE, SIZE, SIZE, paint);
                canvas.DrawLine(-SIZE, SIZE, SIZE, -SIZE, paint);
            }
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateBitmap(bitmap,
                                                     SKShaderTileMode.Mirror,
                                                     SKShaderTileMode.Mirror);
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}

using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public class SweepGradientPage : ContentPage
	{
        bool drawBackground;

		public SweepGradientPage ()
		{
            Title = "Sweep Gradient";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += (sender, args) =>
            {
                drawBackground ^= true;
                canvasView.InvalidateSurface();
            };
            canvasView.GestureRecognizers.Add(tap);
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                // Define an array of rainbow colors
                SKColor[] colors = new SKColor[8];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = SKColor.FromHsl(i * 360f / 7, 100, 50);
                }

                SKPoint center = new SKPoint(info.Rect.MidX, info.Rect.MidY);

                // Create sweep gradient based on center of canvas
                paint.Shader = SKShader.CreateSweepGradient(center, colors, null);

                // Draw a circle with a wide line
                const int strokeWidth = 50;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = strokeWidth;

                float radius = (Math.Min(info.Width, info.Height) - strokeWidth) / 2;
                canvas.DrawCircle(center, radius, paint);

                if (drawBackground)
                {
                    // Draw the gradient on the whole canvas
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawRect(info.Rect, paint);
                }
            }
        }
    }
}
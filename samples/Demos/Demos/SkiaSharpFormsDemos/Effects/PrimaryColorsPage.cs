using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class PrimaryColorsPage : ContentPage
    {
        bool isSubtractive;

	    public PrimaryColorsPage ()
	    {
            Title = "Primary Colors";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;

            // Switch between additive and subtractive primaries at tap
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += (sender, args) =>
            {
                isSubtractive ^= true;
                canvasView.InvalidateSurface();
            };
            canvasView.GestureRecognizers.Add(tap);

            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPoint center = new SKPoint(info.Rect.MidX, info.Rect.MidY);
            float radius = Math.Min(info.Width, info.Height) / 4;
            float distance = 0.8f * radius;     // from canvas center to circle center
            SKPoint center1 = center + 
                new SKPoint(distance * (float)Math.Cos(9 * Math.PI / 6),
                            distance * (float)Math.Sin(9 * Math.PI / 6));
            SKPoint center2 = center +
                new SKPoint(distance * (float)Math.Cos(1 * Math.PI / 6),
                            distance * (float)Math.Sin(1 * Math.PI / 6));
            SKPoint center3 = center +
                new SKPoint(distance * (float)Math.Cos(5 * Math.PI / 6),
                            distance * (float)Math.Sin(5 * Math.PI / 6));

            using (SKPaint paint = new SKPaint())
            {
                if (!isSubtractive)
                {
                    paint.BlendMode = SKBlendMode.Plus; 
                    System.Diagnostics.Debug.WriteLine(paint.BlendMode);

                    paint.Color = SKColors.Red;
                    canvas.DrawCircle(center1, radius, paint);

                    paint.Color = SKColors.Lime;    // == (00, FF, 00)
                    canvas.DrawCircle(center2, radius, paint);

                    paint.Color = SKColors.Blue;
                    canvas.DrawCircle(center3, radius, paint);
                }
                else
                {
                    paint.BlendMode = SKBlendMode.Multiply;
                    System.Diagnostics.Debug.WriteLine(paint.BlendMode);

                    paint.Color = SKColors.Cyan;
                    canvas.DrawCircle(center1, radius, paint);

                    paint.Color = SKColors.Magenta;
                    canvas.DrawCircle(center2, radius, paint);

                    paint.Color = SKColors.Yellow;
                    canvas.DrawCircle(center3, radius, paint);
                }
            }
        }
    }
}
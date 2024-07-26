using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class JitterTextPage : ContentPage
    {
        public JitterTextPage()
        {
            Title = "Jitter Text";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            string text = "FUZZY";

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Color = SKColors.Purple;
                textPaint.PathEffect = SKPathEffect.CreateDiscrete(3f, 10f);

                // Adjust TextSize property so text is 95% of screen width
                float textWidth = textPaint.MeasureText(text);
                textPaint.TextSize *= 0.95f * info.Width / textWidth;

                // Find the text bounds
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);

                // Calculate offsets to center the text on the screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                canvas.DrawText(text, xText, yText, textPaint);
            }
        }
    }
}
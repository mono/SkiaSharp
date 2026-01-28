using System;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Curves
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

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            string text = "FUZZY";

            using (SKPaint textPaint = new SKPaint())
            using (SKFont textFont = new SKFont())
            {
                textPaint.Color = SKColors.Purple;
                textPaint.PathEffect = SKPathEffect.CreateDiscrete(3f, 10f);

                // Adjust TextSize property so text is 95% of screen width
                float textWidth = textFont.MeasureText(text);
                textFont.Size *= 0.95f * info.Width / textWidth;

                // Find the text bounds
                textFont.MeasureText(text, out SKRect textBounds);

                // Calculate offsets to center the text on the screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                canvas.DrawText(text, xText, yText, SKTextAlign.Left, textFont, textPaint);
            }
        }
    }
}
using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public class OutlinedTextPage : ContentPage
    {
        public OutlinedTextPage()
        {
            Title = "Outlined Text";

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

            string text = "OUTLINE";

            // Create an SKPaint object to display the text
            SKPaint textPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                Color = SKColors.Blue
            };

            // Create an SKFont object for text measurement and rendering
            using SKFont font = new SKFont
            {
                Embolden = true
            };

            // Adjust TextSize property so text is 95% of screen width
            float textWidth = font.MeasureText(text);
            font.Size = 0.95f * info.Width * font.Size / textWidth;

            // Find the text bounds
            font.MeasureText(text, out SKRect textBounds);

            // Calculate offsets to center the text on the screen
            float xText = info.Width / 2 - textBounds.MidX;
            float yText = info.Height / 2 - textBounds.MidY;

            // And draw the text
            canvas.DrawText(text, xText, yText, SKTextAlign.Left, font, textPaint);
        }
    }
}

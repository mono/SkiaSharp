using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public class FramedTextPage : ContentPage
    {
        public FramedTextPage()
        {
            Title = "Framed Text";

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

            string str = "Hello SkiaSharp!";

            // Create an SKPaint object to display the text
            SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Chocolate
            };

            // Create an SKFont object for text measurement and rendering
            using SKFont font = new SKFont();

            // Adjust TextSize property so text is 90% of screen width
            float textWidth = font.MeasureText(str);
            font.Size = 0.9f * info.Width * font.Size / textWidth;

            // Find the text bounds
            font.MeasureText(str, out SKRect textBounds);

            // Calculate offsets to center the text on the screen
            float xText = info.Width / 2 - textBounds.MidX;
            float yText = info.Height / 2 - textBounds.MidY;

            // And draw the text
            canvas.DrawText(str, xText, yText, SKTextAlign.Left, font, textPaint);

            // Create a new SKRect object for the frame around the text
            SKRect frameRect = textBounds;
            frameRect.Offset(xText, yText);
            frameRect.Inflate(10, 10);

            // Create an SKPaint object to display the frame
            SKPaint framePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 5,
                Color = SKColors.Blue
            };

            // Draw one frame
            canvas.DrawRoundRect(frameRect, 20, 20, framePaint);

            // Inflate the frameRect and draw another
            frameRect.Inflate(10, 10);
            framePaint.Color = SKColors.DarkBlue;
            canvas.DrawRoundRect(frameRect, 30, 30, framePaint);
        }
    }
}

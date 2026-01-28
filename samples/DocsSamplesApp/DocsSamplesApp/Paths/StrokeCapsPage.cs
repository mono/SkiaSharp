using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Paths
{
    public class StrokeCapsPage : ContentPage
    {
        public StrokeCapsPage()
        {
            Title = "Stroke Caps";

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

            using SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black
            };

            using SKFont textFont = new SKFont
            {
                Size = 75
            };

            using SKPaint thickLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Orange,
                StrokeWidth = 50
            };

            using SKPaint thinLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2
            };

            float xText = info.Width / 2;
            float xLine1 = 100;
            float xLine2 = info.Width - xLine1;
            float y = textFont.Spacing;

            foreach (SKStrokeCap strokeCap in Enum.GetValues(typeof(SKStrokeCap)))
            {
                // Display text
                canvas.DrawText(strokeCap.ToString(), xText, y, SKTextAlign.Center, textFont, textPaint);
                y += textFont.Spacing;

                // Display thick line
                thickLinePaint.StrokeCap = strokeCap;
                canvas.DrawLine(xLine1, y, xLine2, y, thickLinePaint);

                // Display thin line
                canvas.DrawLine(xLine1, y, xLine2, y, thinLinePaint);
                y += 2 * textFont.Spacing;
            }
        }
    }
}

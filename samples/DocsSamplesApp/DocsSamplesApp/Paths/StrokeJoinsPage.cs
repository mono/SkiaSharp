using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Paths
{
    public class StrokeJoinsPage : ContentPage
    {
        public StrokeJoinsPage()
        {
            Title = "Stroke Joins";

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

            float xText = info.Width - 100;
            float xLine1 = 100;
            float xLine2 = info.Width - xLine1;
            float y = 2 * textFont.Spacing;
            string[] strStrokeJoins = { "Miter", "Round", "Bevel" };

            foreach (string strStrokeJoin in strStrokeJoins)
            {
                // Display text
                canvas.DrawText(strStrokeJoin, xText, y, SKTextAlign.Right, textFont, textPaint);

                // Get stroke-join value
                SKStrokeJoin strokeJoin;
                Enum.TryParse(strStrokeJoin, out strokeJoin);

                // Create path
                SKPath path = new SKPath();
                path.MoveTo(xLine1, y - 80);
                path.LineTo(xLine1, y + 80);
                path.LineTo(xLine2, y + 80);

                // Display thick line
                thickLinePaint.StrokeJoin = strokeJoin;
                canvas.DrawPath(path, thickLinePaint);

                // Display thin line
                canvas.DrawPath(path, thinLinePaint);
                y += 3 * textFont.Spacing;
            }
        }
    }
}

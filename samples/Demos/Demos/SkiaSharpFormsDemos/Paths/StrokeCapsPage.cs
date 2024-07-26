using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Paths
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

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 75, 
                TextAlign = SKTextAlign.Center
            };

            SKPaint thickLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Orange,
                StrokeWidth = 50
            };

            SKPaint thinLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2
            };

            float xText = info.Width / 2;
            float xLine1 = 100;
            float xLine2 = info.Width - xLine1;
            float y = textPaint.FontSpacing;

            foreach (SKStrokeCap strokeCap in Enum.GetValues(typeof(SKStrokeCap)))
            {
                // Display text
                canvas.DrawText(strokeCap.ToString(), xText, y, textPaint);
                y += textPaint.FontSpacing;

                // Display thick line
                thickLinePaint.StrokeCap = strokeCap;
                canvas.DrawLine(xLine1, y, xLine2, y, thickLinePaint);

                // Display thin line
                canvas.DrawLine(xLine1, y, xLine2, y, thinLinePaint);
                y += 2 * textPaint.FontSpacing;
            }
        }
    }
}

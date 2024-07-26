using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class ClipOperationsPage : ContentPage
    {
        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,
            TextSize = 40,
            TextAlign = SKTextAlign.Center
        };

        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Pink
        };

        public ClipOperationsPage()
        {
            Title = "Clip Operations";

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

            float x = 0;
            float y = 0;

            foreach (SKClipOperation clipOp in Enum.GetValues(typeof(SKClipOperation)))
            {
                // Portrait mode
                if (info.Height > info.Width)
                {
                    DisplayClipOp(canvas, new SKRect(x, y, x + info.Width, y + info.Height / 2), clipOp);
                    y += info.Height / 2;
                }
                // Landscape mode
                else
                {
                    DisplayClipOp(canvas, new SKRect(x, y, x + info.Width / 2, y + info.Height), clipOp);
                    x += info.Width / 2;
                }
            }
        }

        void DisplayClipOp(SKCanvas canvas, SKRect rect, SKClipOperation clipOp)
        {
            float textSize = textPaint.TextSize;
            canvas.DrawText(clipOp.ToString(), rect.MidX, rect.Top + textSize, textPaint);
            rect.Top += textSize;

            float radius = 0.9f * Math.Min(rect.Width / 3, rect.Height / 2);
            float xCenter = rect.MidX;
            float yCenter = rect.MidY;

            canvas.Save();

            using (SKPath path1 = new SKPath())
            {
                path1.AddCircle(xCenter - radius / 2, yCenter, radius);
                canvas.ClipPath(path1);

                using (SKPath path2 = new SKPath())
                {
                    path2.AddCircle(xCenter + radius / 2, yCenter, radius);
                    canvas.ClipPath(path2, clipOp);

                    canvas.DrawPaint(fillPaint);
                }
            }

            canvas.Restore();
        }
    }
}

using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class RotatedTextPage : ContentPage
    {
        static readonly string text = "    ROTATE";

        public RotatedTextPage()
        {
            Title = "Rotated Text";

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

            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 72
            })
            {
                float xCenter = info.Width / 2;
                float yCenter = info.Height / 2;

                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float yText = yCenter - textBounds.Height / 2 - textBounds.Top;

                for (int degrees = 0; degrees < 360; degrees += 30)
                {
                    canvas.Save();
                    canvas.RotateDegrees(degrees, xCenter, yCenter);
                    canvas.DrawText(text, xCenter, yText, textPaint);
                    canvas.Restore();
                }
            }
        }
    }
}
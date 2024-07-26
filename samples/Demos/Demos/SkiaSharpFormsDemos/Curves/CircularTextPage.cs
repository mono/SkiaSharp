using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class CircularTextPage : ContentPage
    {
        const string text = "xt in a circle that shapes the te";

        public CircularTextPage()
        {
            Title = "Circular Text";

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

            using (SKPath circularPath = new SKPath())
            {
                float radius = 0.35f * Math.Min(info.Width, info.Height);
                circularPath.AddCircle(info.Width / 2, info.Height / 2, radius);

                using (SKPaint textPaint = new SKPaint())
                {
                    textPaint.TextSize = 100;
                    float textWidth = textPaint.MeasureText(text);
                    textPaint.TextSize *= 2 * 3.14f * radius / textWidth;

                    canvas.DrawTextOnPath(text, circularPath, 0, 0, textPaint);
                }
            }
        }
    }
}
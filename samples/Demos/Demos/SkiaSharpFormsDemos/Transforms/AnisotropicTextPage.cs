using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class AnisotropicTextPage : ContentPage
    {
        public AnisotropicTextPage()
        {
            Title = "Anisotropic Text";

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
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 0.1f,
                StrokeJoin = SKStrokeJoin.Round
            })
            {
                SKRect textBounds = new SKRect();
                textPaint.MeasureText("HELLO", ref textBounds);

                // Inflate bounds by the stroke width
                textBounds.Inflate(textPaint.StrokeWidth / 2, 
                                   textPaint.StrokeWidth / 2);

                canvas.Scale(info.Width / textBounds.Width,
                             info.Height / textBounds.Height);
                canvas.Translate(-textBounds.Left, -textBounds.Top);

                canvas.DrawText("HELLO", 0, 0, textPaint);
            }
        }
    }
}

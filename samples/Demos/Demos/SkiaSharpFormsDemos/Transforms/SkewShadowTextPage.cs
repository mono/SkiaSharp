using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class SkewShadowTextPage : ContentPage
    {
        public SkewShadowTextPage()
        {
            Title = "Shadow Text";

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

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.TextSize = info.Width / 6;   // empirically determined

                // Common to shadow and text
                string text = "shadow";
                float xText = 20;
                float yText = info.Height / 2;

                // Shadow
                textPaint.Color = SKColors.LightGray;
                canvas.Save();
                canvas.Translate(xText, yText);
                canvas.Skew((float)Math.Tan(-Math.PI / 4), 0);
                canvas.Scale(1, 3);
                canvas.Translate(-xText, -yText);
                canvas.DrawText(text, xText, yText, textPaint);
                canvas.Restore();

                // Text
                textPaint.Color = SKColors.Blue;
                canvas.DrawText(text, xText, yText, textPaint);
            }
        }
    }
}

using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class TranslateTextEffectsPage : ContentPage
    {
        public TranslateTextEffectsPage()
        {
            Title = "Translate Text Effects";

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

            float textSize = 150;

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.TextSize = textSize;
                textPaint.FakeBoldText = true;

                float x = 10;
                float y = textSize;

                // Shadow
                canvas.Translate(10, 10);
                textPaint.Color = SKColors.Black;
                canvas.DrawText("SHADOW", x, y, textPaint);
                canvas.Translate(-10, -10);
                textPaint.Color = SKColors.Pink;
                canvas.DrawText("SHADOW", x, y, textPaint);

                y += 2 * textSize;

                // Engrave
                canvas.Translate(-5, -5);
                textPaint.Color = SKColors.Black;
                canvas.DrawText("ENGRAVE", x, y, textPaint);
                canvas.ResetMatrix();
                textPaint.Color = SKColors.White;
                canvas.DrawText("ENGRAVE", x, y, textPaint);

                y += 2 * textSize;

                // Emboss
                canvas.Save();
                canvas.Translate(5, 5);
                textPaint.Color = SKColors.Black;
                canvas.DrawText("EMBOSS", x, y, textPaint);
                canvas.Restore();
                textPaint.Color = SKColors.White;
                canvas.DrawText("EMBOSS", x, y, textPaint);
            }
        }
    }
}

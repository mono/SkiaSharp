using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class BlurryReflectionPage : ContentPage
    {
        const string TEXT = "Reflection";

        public BlurryReflectionPage()
        {
            Title = "Blurry Reflection";

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

            using (SKPaint paint = new SKPaint())
            {
                // Set text color to blue
                paint.Color = SKColors.Blue;

                // Set text size to fill 90% of width
                paint.TextSize = 100;
                float width = paint.MeasureText(TEXT);
                float scale = 0.9f * info.Width / width;
                paint.TextSize *= scale;

                // Get text bounds
                SKRect textBounds = new SKRect();
                paint.MeasureText(TEXT, ref textBounds);

                // Calculate offsets to position text above center
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2;

                // Draw unreflected text
                canvas.DrawText(TEXT, xText, yText, paint);

                // Shift textBounds to match displayed text
                textBounds.Offset(xText, yText);

                // Use those offsets to create a gradient for the reflected text
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(0, textBounds.Top),
                                    new SKPoint(0, textBounds.Bottom),
                                    new SKColor[] { paint.Color.WithAlpha(0),
                                                    paint.Color.WithAlpha(0x80) },
                                    null,
                                    SKShaderTileMode.Clamp);

                // Create a blur mask filter
                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, paint.TextSize / 36);

                // Scale the canvas to flip upside-down around the vertical center
                canvas.Scale(1, -1, 0, yText);

                // Draw reflected text
                canvas.DrawText(TEXT, xText, yText, paint);
            }
        }
    }
}
using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class TextPathEffectPage : ContentPage
    {
        const string character = "@";
        const float littleSize = 50;

        SKPathEffect pathEffect;

        SKPaint textPathPaint = new SKPaint
        {
            TextSize = littleSize
        };

        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black
        };

        public TextPathEffectPage()
        {
            Title = "Text Path Effect";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Get the bounds of textPathPaint
            SKRect textPathPaintBounds = new SKRect();
            textPathPaint.MeasureText(character, ref textPathPaintBounds);

            // Create textPath centered around (0, 0)
            SKPath textPath = textPathPaint.GetTextPath(character, 
                                                        -textPathPaintBounds.MidX,
                                                        -textPathPaintBounds.MidY);
            // Create the path effect
            pathEffect = SKPathEffect.Create1DPath(textPath, littleSize, 0,
                                                   SKPath1DPathEffectStyle.Translate);
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Set textPaint TextSize based on screen size
            textPaint.TextSize = Math.Min(info.Width, info.Height);

            // Do not measure the text with PathEffect set!
            SKRect textBounds = new SKRect();
            textPaint.MeasureText(character, ref textBounds);

            // Coordinates to center text on screen
            float xText = info.Width / 2 - textBounds.MidX;
            float yText = info.Height / 2 - textBounds.MidY;

            // Set the PathEffect property and display text
            textPaint.PathEffect = pathEffect;
            canvas.DrawText(character, xText, yText, textPaint);
        }
    }
}


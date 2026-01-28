using System;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Curves
{
    public class TextPathEffectPage : ContentPage
    {
        const string character = "@";
        const float littleSize = 50;

        SKPathEffect pathEffect;

        SKFont textPathFont = new SKFont
        {
            Size = littleSize
        };

        SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black
        };

        SKFont textFont = new SKFont();

        public TextPathEffectPage()
        {
            Title = "Text Path Effect";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Get the bounds of textPathFont
            textPathFont.MeasureText(character, out SKRect textPathPaintBounds);

            // Create textPath centered around (0, 0)
            SKPath textPath = textPathFont.GetTextPath(character, 
                                                        new SKPoint(-textPathPaintBounds.MidX, -textPathPaintBounds.MidY));
            // Create the path effect
            pathEffect = SKPathEffect.Create1DPath(textPath, littleSize, 0,
                                                   SKPath1DPathEffectStyle.Translate);
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Set textFont Size based on screen size
            textFont.Size = Math.Min(info.Width, info.Height);

            // Do not measure the text with PathEffect set!
            textFont.MeasureText(character, out SKRect textBounds);

            // Coordinates to center text on screen
            float xText = info.Width / 2 - textBounds.MidX;
            float yText = info.Height / 2 - textBounds.MidY;

            // Set the PathEffect property and display text
            textPaint.PathEffect = pathEffect;
            canvas.DrawText(character, xText, yText, SKTextAlign.Left, textFont, textPaint);
        }
    }
}


using System;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Curves
{
    public class CharacterOutlineOutlinesPage : ContentPage
    {
        public CharacterOutlineOutlinesPage()
        {
            Title = "Character Outline Outlines";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint textPaint = new SKPaint())
            using (SKFont textFont = new SKFont())
            {
                // Set Style for the character outlines
                textPaint.Style = SKPaintStyle.Stroke;

                // Set TextSize based on screen size
                textFont.Size = Math.Min(info.Width, info.Height);

                // Measure the text
                textFont.MeasureText("@", out SKRect textBounds);

                // Coordinates to center text on screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                // Get the path for the character outlines
                using (SKPath textPath = textFont.GetTextPath("@", new SKPoint(xText, yText)))
                {
                    // Create a new path for the outlines of the path
                    using (SKPath outlinePath = new SKPath())
                    {
                        // Convert the path to the outlines of the stroked path
                        textPaint.StrokeWidth = 25;
                        textPaint.GetFillPath(textPath, outlinePath);

                        // Stroke that new path
                        using (SKPaint outlinePaint = new SKPaint())
                        {
                            outlinePaint.Style = SKPaintStyle.Stroke;
                            outlinePaint.StrokeWidth = 5;
                            outlinePaint.Color = SKColors.Red;

                            canvas.DrawPath(outlinePath, outlinePaint);
                        }
                    }
                }
            }
        }
    }
}
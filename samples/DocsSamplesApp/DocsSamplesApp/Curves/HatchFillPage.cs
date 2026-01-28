using System;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Curves
{
    public class HatchFillPage : ContentPage
    {
        SKPaint fillPaint = new SKPaint();

        SKPathEffect horzLinesPath = SKPathEffect.Create2DLine(3, SKMatrix.CreateScale(6, 6));

        SKPathEffect vertLinesPath = SKPathEffect.Create2DLine(6, 
            Multiply(SKMatrix.CreateRotationDegrees(90), SKMatrix.CreateScale(24, 24)));

        SKPathEffect diagLinesPath = SKPathEffect.Create2DLine(12, 
            Multiply(SKMatrix.CreateScale(36, 36), SKMatrix.CreateRotationDegrees(45)));

        SKPaint strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            Color = SKColors.Black
        };

        public HatchFillPage()
        {
            Title = "Hatch Fill";

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

            using (SKPath roundRectPath = new SKPath())
            {
                // Create a path 
                roundRectPath.AddRoundRect(
                    new SKRect(50, 50, info.Width - 50, info.Height - 50), 100, 100);

                // Horizontal hatch marks
                fillPaint.PathEffect = horzLinesPath;
                fillPaint.Color = SKColors.Red;
                canvas.DrawPath(roundRectPath, fillPaint); 

                // Vertical hatch marks
                fillPaint.PathEffect = vertLinesPath;
                fillPaint.Color = SKColors.Blue;
                canvas.DrawPath(roundRectPath, fillPaint);

                // Diagonal hatch marks -- use clipping
                fillPaint.PathEffect = diagLinesPath;
                fillPaint.Color = SKColors.Green;

                canvas.Save();
                canvas.ClipPath(roundRectPath);
                canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), fillPaint);
                canvas.Restore();

                // Outline the path
                canvas.DrawPath(roundRectPath, strokePaint);
            }
        }

        static SKMatrix Multiply(SKMatrix first, SKMatrix second)
        {
            SKMatrix target = SKMatrix.Identity;
            SKMatrix.Concat(ref target, first, second);
            return target;
        }
    }
}
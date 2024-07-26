using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class CatsInFramePage : ContentPage
    {
        // From PathDataCatPage.cs
        SKPath catPath = SKPath.ParseSvgPathData(
            "M 160 140 L 150 50 220 103" +              // Left ear
            "M 320 140 L 330 50 260 103" +              // Right ear
            "M 215 230 L 40 200" +                      // Left whiskers
            "M 215 240 L 40 240" +
            "M 215 250 L 40 280" +
            "M 265 230 L 440 200" +                     // Right whiskers
            "M 265 240 L 440 240" +
            "M 265 250 L 440 280" +
            "M 240 100" +                               // Head
            "A 100 100 0 0 1 240 300" +
            "A 100 100 0 0 1 240 100 Z" +
            "M 180 170" +                               // Left eye
            "A 40 40 0 0 1 220 170" +
            "A 40 40 0 0 1 180 170 Z" +
            "M 300 170" +                               // Right eye
            "A 40 40 0 0 1 260 170" +
            "A 40 40 0 0 1 300 170 Z");

        SKPaint catStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };

        SKPath scallopPath = 
            SKPath.ParseSvgPathData("M 0 0 L 50 0 A 60 60 0 0 1 -50 0 Z");

        SKPaint framePaint = new SKPaint
        {
            Color = SKColors.Black
        };

        public CatsInFramePage()
        {
            Title = "Cats in Frame";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Move (0, 0) point to center of cat path
            catPath.Transform(SKMatrix.MakeTranslation(-240, -175));

            // Now catPath is 400 by 250
            // Scale it down to 160 by 100
            catPath.Transform(SKMatrix.MakeScale(0.40f, 0.40f));

            // Get the outlines of the contours of the cat path
            SKPath outlinedCatPath = new SKPath();
            catStroke.GetFillPath(catPath, outlinedCatPath);

            // Create a 2D path effect from those outlines
            SKPathEffect fillEffect = SKPathEffect.Create2DPath(
                new SKMatrix { ScaleX = 170, ScaleY = 110,
                               TransX = 75, TransY = 80,
                               Persp2 = 1 },
                outlinedCatPath);

            // Create a 1D path effect from the scallop path
            SKPathEffect strokeEffect = 
                SKPathEffect.Create1DPath(scallopPath, 75, 0, SKPath1DPathEffectStyle.Rotate);

            // Set the sum the effects to frame paint
            framePaint.PathEffect = SKPathEffect.CreateSum(fillEffect, strokeEffect);
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect rect = new SKRect(50, 50, info.Width - 50, info.Height - 50);
            canvas.ClipRect(rect);
            canvas.DrawRect(rect, framePaint);
        }
    }
}
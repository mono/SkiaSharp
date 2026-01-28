using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Transforms
{
    public class PathTransformPage : ContentPage
    {
        SKPath transformedPath = HendecagramArrayPage.HendecagramPath;

        public PathTransformPage()
        {
            Title = "Path Transform";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            SKMatrix matrix = SKMatrix.CreateScale(3, 3);
            matrix = matrix.PostConcat(SKMatrix.CreateRotationDegrees(360f / 22));
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(300, 300));

            transformedPath.Transform(matrix);
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Magenta;
                paint.StrokeWidth = 5;

                canvas.DrawPath(transformedPath, paint);
            }
        }
    }
}
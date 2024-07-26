using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
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

            SKMatrix matrix = SKMatrix.MakeScale(3, 3);
            SKMatrix.PostConcat(ref matrix, SKMatrix.MakeRotationDegrees(360f / 22));
            SKMatrix.PostConcat(ref matrix, SKMatrix.MakeTranslation(300, 300));

            transformedPath.Transform(matrix);
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
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
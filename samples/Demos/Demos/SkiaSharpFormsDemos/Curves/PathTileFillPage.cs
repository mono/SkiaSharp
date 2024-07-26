using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class PathTileFillPage : ContentPage
    {
        SKPath tilePath = SKPath.ParseSvgPathData(
            "M -20 -20 L 2 -20, 2 -40, 18 -40, 18 -20, 40 -20, " + 
            "40 -12, 20 -12, 20 12, 40 12, 40 40, 22 40, 22 20, " + 
            "-2 20, -2 40, -20 40, -20 8, -40 8, -40 -8, -20 -8 Z");

        public PathTileFillPage()
        {
            Title = "Path Tile Fill";

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
                paint.Color = SKColors.Red;

                using (SKPathEffect pathEffect =
                       SKPathEffect.Create2DPath(SKMatrix.MakeScale(64, 64), tilePath))
                {
                    paint.PathEffect = pathEffect;

                    canvas.DrawRoundRect(
                        new SKRect(50, 50, info.Width - 50, info.Height - 50), 
                        100, 100, paint);
                }
            }
        }
    }
}
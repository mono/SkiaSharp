using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class DashedHatchLinesPage : ContentPage
    {
        static SKPathEffect dashEffect = 
            SKPathEffect.CreateDash(new float[] { 30, 30 }, 0);

        static SKPathEffect hatchEffect = SKPathEffect.Create2DLine(20,
            Multiply(SKMatrix.MakeScale(60, 60), 
                     SKMatrix.MakeRotationDegrees(45)));

        SKPaint paint = new SKPaint()
        {
            PathEffect = SKPathEffect.CreateCompose(dashEffect, hatchEffect),
            StrokeCap = SKStrokeCap.Round,
            Color = SKColors.Blue
        };

        public DashedHatchLinesPage()
        {
            Title = "Dashed Hatch Lines";

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

            canvas.DrawOval(info.Width / 2, info.Height / 2, 
                            0.45f * info.Width, 0.45f * info.Height, 
                            paint);
        }

        static SKMatrix Multiply(SKMatrix first, SKMatrix second)
        {
            SKMatrix target = SKMatrix.MakeIdentity();
            SKMatrix.Concat(ref target, first, second);
            return target;
        }
    }
}
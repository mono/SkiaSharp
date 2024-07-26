using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class ObliqueTextPage : ContentPage
    {
        public ObliqueTextPage()
        {
            Title = "Oblique Text";

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

            using (SKPaint textPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Maroon,
                TextAlign = SKTextAlign.Center,
                TextSize = info.Width / 8   // empirically determined
            })
            {
                canvas.Translate(info.Width / 2, info.Height / 2);
                SkewDegrees(canvas, -20, 0);
                canvas.DrawText(Title, 0, 0, textPaint);
            }
        }

        void SkewDegrees(SKCanvas canvas, double xDegrees, double yDegrees)
        {
            canvas.Skew((float)Math.Tan(Math.PI * xDegrees / 180),
                        (float)Math.Tan(Math.PI * yDegrees / 180));
        }
    }
}

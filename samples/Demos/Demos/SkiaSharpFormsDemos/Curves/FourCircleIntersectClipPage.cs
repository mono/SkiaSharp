using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class FourCircleIntersectClipPage : ContentPage
    {
        public FourCircleIntersectClipPage()
        {
            Title = "Four Circle Intersect Clip";

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

            float size = Math.Min(info.Width, info.Height);
            float radius = 0.4f * size;
            float offset = size / 2 - radius;

            // Translate to center
            canvas.Translate(info.Width / 2, info.Height / 2);

            using (SKPath path = new SKPath())
            {
                path.AddCircle(-offset, -offset, radius);
                canvas.ClipPath(path, SKClipOperation.Intersect);

                path.Reset();
                path.AddCircle(-offset, offset, radius);
                canvas.ClipPath(path, SKClipOperation.Intersect);

                path.Reset();
                path.AddCircle(offset, -offset, radius);
                canvas.ClipPath(path, SKClipOperation.Intersect);

                path.Reset();
                path.AddCircle(offset, offset, radius);
                canvas.ClipPath(path, SKClipOperation.Intersect);

                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = SKColors.Blue;
                    canvas.DrawPaint(paint);
                }
            }
        }
    }
}

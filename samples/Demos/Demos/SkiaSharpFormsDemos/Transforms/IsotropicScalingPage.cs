using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class IsotropicScalingPage : ContentPage
    {
        public IsotropicScalingPage()
        {
            Title = "Isotropic Scaling";

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

            SKPath path = HendecagramArrayPage.HendecagramPath;
            SKRect pathBounds = path.Bounds;

            using (SKPaint fillPaint = new SKPaint())
            {
                fillPaint.Style = SKPaintStyle.Fill;

                float scale = Math.Min(info.Width / pathBounds.Width,
                                       info.Height / pathBounds.Height);

                for (int i = 0; i <= 10; i++)
                {
                    fillPaint.Color = new SKColor((byte)(255 * (10 - i) / 10),
                                                  0,
                                                  (byte)(255 * i / 10));
                    canvas.Save();
                    canvas.Translate(info.Width / 2, info.Height / 2);
                    canvas.Scale(scale);
                    canvas.Translate(-pathBounds.MidX, -pathBounds.MidY);
                    canvas.DrawPath(path, fillPaint);
                    canvas.Restore();

                    scale *= 0.9f;
                }
            }
        }
    }
}

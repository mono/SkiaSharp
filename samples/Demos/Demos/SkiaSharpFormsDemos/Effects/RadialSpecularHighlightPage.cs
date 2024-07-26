using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class RadialSpecularHighlightPage : ContentPage
    {
        public RadialSpecularHighlightPage()
        {
            Title = "Radial Specular Highlight";

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

            float radius = 0.4f * Math.Min(info.Width, info.Height);
            SKPoint center = new SKPoint(info.Rect.MidX, info.Rect.MidY);
            SKPoint offCenter = center - new SKPoint(radius / 2, radius / 2);

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateRadialGradient(
                                    offCenter,
                                    radius / 2,
                                    new SKColor[] { SKColors.White, SKColors.Red },
                                    null,
                                    SKShaderTileMode.Clamp);

                canvas.DrawCircle(center, radius, paint);
            }
        }
    }
}

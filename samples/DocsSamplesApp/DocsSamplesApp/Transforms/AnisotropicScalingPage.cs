using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Transforms
{
    public class AnisotropicScalingPage : ContentPage
    {
        public AnisotropicScalingPage()
        {
            Title = "Anisotropic Scaling";

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

            SKPath path = HendecagramArrayPage.HendecagramPath;
            SKRect pathBounds = path.Bounds;

            using (SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Pink
            })
            using (SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 3,
                StrokeJoin = SKStrokeJoin.Round
            })
            {
                canvas.Scale(info.Width / pathBounds.Width, 
                             info.Height / pathBounds.Height);
                canvas.Translate(-pathBounds.Left, -pathBounds.Top);

                canvas.DrawPath(path, fillPaint);
                canvas.DrawPath(path, strokePaint);
            }
        }
    }
}

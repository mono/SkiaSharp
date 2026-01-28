
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public class SimpleCirclePage : ContentPage
    {
        public SimpleCirclePage()
        {
            Title = "Simple Circle";

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

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Microsoft.Maui.Graphics.Colors.Red.ToSKColor(),
                StrokeWidth = 25
            };
            canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);

            paint.Style = SKPaintStyle.Fill;
            paint.Color = SKColors.Blue;
            canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
        }
    }
}

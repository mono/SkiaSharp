
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public partial class EllipseFillPage : ContentPage
    {
        public EllipseFillPage()
        {
            InitializeComponent();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float strokeWidth = 50;
            float xRadius = (info.Width - strokeWidth) / 2;
            float yRadius = (info.Height - strokeWidth) / 2;

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = strokeWidth
            };
            canvas.DrawOval(info.Width / 2, info.Height / 2, xRadius, yRadius, paint);
        }
    }
}

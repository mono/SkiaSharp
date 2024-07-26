using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;


namespace SkiaSharpFormsDemos.Basics
{
    public partial class TapToggleFillPage : ContentPage
    {
        bool showFill = true;

        public TapToggleFillPage()
        {
            InitializeComponent();
        }

        void OnCanvasViewTapped(object sender, EventArgs args)
        {
            showFill ^= true;
            (sender as SKCanvasView).InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Red.ToSKColor(),
                StrokeWidth = 50
            };
            canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);

            if (showFill)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = SKColors.Blue;
                canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
            }
        }
    }
}

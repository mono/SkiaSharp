using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class SkewExperimentPage : ContentPage
    {
        public SkewExperimentPage()
        {
            InitializeComponent();
        }

        void sliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint textPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue,
                TextSize = 200
            })
            {
                string text = "SKEW";
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);

                canvas.Skew((float)xSkewSlider.Value, (float)ySkewSlider.Value);
                canvas.DrawText(text, 0, -textBounds.Top, textPaint);
            }
        }
    }
}

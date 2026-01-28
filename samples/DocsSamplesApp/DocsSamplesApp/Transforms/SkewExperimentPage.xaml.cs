using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Transforms
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

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint textPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue
            })
            using (SKFont font = new SKFont { Size = 200 })
            {
                string text = "SKEW";
                font.MeasureText(text, out SKRect textBounds);

                canvas.Skew((float)xSkewSlider.Value, (float)ySkewSlider.Value);
                canvas.DrawText(text, 0, -textBounds.Top, SKTextAlign.Left, font, textPaint);
            }
        }
    }
}

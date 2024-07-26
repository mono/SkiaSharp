using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class SkewAngleExperimentPage : ContentPage
    {
        public SkewAngleExperimentPage()
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
                float xCenter = info.Width / 2;
                float yCenter = info.Height / 2;

                string text = "SKEW";
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float xText = xCenter - textBounds.MidX;
                float yText = yCenter - textBounds.MidY;

                canvas.Translate(xCenter, yCenter);
                SkewDegrees(canvas, xSkewSlider.Value, ySkewSlider.Value);
                canvas.Translate(-xCenter, -yCenter);
                canvas.DrawText(text, xText, yText, textPaint);
            }
        }

        void SkewDegrees(SKCanvas canvas, double xDegrees, double yDegrees)
        {
            canvas.Skew((float)Math.Tan(Math.PI * xDegrees / 180),
                        (float)Math.Tan(Math.PI * yDegrees / 180));
        }
    }
}

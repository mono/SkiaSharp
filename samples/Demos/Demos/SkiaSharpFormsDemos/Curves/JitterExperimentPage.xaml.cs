using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class JitterExperimentPage : ContentPage
    {
        public JitterExperimentPage()
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

            float segLength = (float)segLengthSlider.Value;
            float deviation = (float)deviationSlider.Value;

            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke; 
                paint.StrokeWidth = 5;
                paint.Color = SKColors.Blue;

                using (SKPathEffect pathEffect = SKPathEffect.CreateDiscrete(segLength, deviation))
                {
                    paint.PathEffect = pathEffect;

                    SKRect rect = new SKRect(100, 100, info.Width - 100, info.Height - 100);
                    canvas.DrawRect(rect, paint);
                }
            }
        }
    }
}

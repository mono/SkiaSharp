using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class PerlinNoisePage : ContentPage
    {
        public PerlinNoisePage()
        {
            InitializeComponent();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnStepperValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Get values from sliders and stepper
            float baseFreqX = (float)Math.Pow(10, baseFrequencyXSlider.Value - 4);
            baseFrequencyXText.Text = String.Format("Base Frequency X = {0:F4}", baseFreqX);

            float baseFreqY = (float)Math.Pow(10, baseFrequencyYSlider.Value - 4);
            baseFrequencyYText.Text = String.Format("Base Frequency Y = {0:F4}", baseFreqY);

            int numOctaves = (int)octavesStepper.Value;

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = 
                    SKShader.CreatePerlinNoiseFractalNoise(baseFreqX,
                                                           baseFreqY,
                                                           numOctaves,
                                                           0);

                SKRect rect = new SKRect(0, 0, info.Width, info.Height / 2);
                canvas.DrawRect(rect, paint);

                paint.Shader = 
                    SKShader.CreatePerlinNoiseTurbulence(baseFreqX,
                                                         baseFreqY,
                                                         numOctaves,
                                                         0);

                rect = new SKRect(0, info.Height / 2, info.Width, info.Height);
                canvas.DrawRect(rect, paint);
            }
        }
    }
}

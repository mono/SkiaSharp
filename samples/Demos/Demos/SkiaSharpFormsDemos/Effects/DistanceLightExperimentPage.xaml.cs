using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class DistantLightExperimentPage : ContentPage
    {
        const string TEXT = "Lighting";

        public DistantLightExperimentPage()
        {
            InitializeComponent();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float z = (float)zSlider.Value;
            float surfaceScale = (float)surfaceScaleSlider.Value;
            float lightConstant = (float)lightConstantSlider.Value;

            using (SKPaint paint = new SKPaint())
            {
                paint.IsAntialias = true;

                // Size text to 90% of canvas width
                paint.TextSize = 100;
                float textWidth = paint.MeasureText(TEXT);
                paint.TextSize *= 0.9f * info.Width / textWidth;

                // Find coordinates to center text
                SKRect textBounds = new SKRect();
                paint.MeasureText(TEXT, ref textBounds);

                float xText = info.Rect.MidX - textBounds.MidX;
                float yText = info.Rect.MidY - textBounds.MidY;

                // Create distant light image filter
                paint.ImageFilter = SKImageFilter.CreateDistantLitDiffuse(
                                        new SKPoint3(2, 3, z),
                                        SKColors.White,
                                        surfaceScale,
                                        lightConstant);

                canvas.DrawText(TEXT, xText, yText, paint);
            }
        }
    }
}

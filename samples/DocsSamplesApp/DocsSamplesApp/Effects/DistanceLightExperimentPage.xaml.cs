using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Effects
{
    public partial class DistantLightExperimentPage : ContentPage
    {
        const string TEXT = "Lighting";

        public DistantLightExperimentPage()
        {
            InitializeComponent();
        }

        void OnSliderValueChanged(object? sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float z = (float)zSlider.Value;
            float surfaceScale = (float)surfaceScaleSlider.Value;
            float lightConstant = (float)lightConstantSlider.Value;

            using (SKPaint paint = new SKPaint())
            using (SKFont font = new SKFont())
            {
                paint.IsAntialias = true;

                // Size text to 90% of canvas width
                font.Size = 100;
                float textWidth = font.MeasureText(TEXT);
                font.Size *= 0.9f * info.Width / textWidth;

                // Find coordinates to center text
                SKRect textBounds = new SKRect();
                font.MeasureText(TEXT, out textBounds);

                float xText = info.Rect.MidX - textBounds.MidX;
                float yText = info.Rect.MidY - textBounds.MidY;

                // Create distant light image filter
                paint.ImageFilter = SKImageFilter.CreateDistantLitDiffuse(
                                        new SKPoint3(2, 3, z),
                                        SKColors.White,
                                        surfaceScale,
                                        lightConstant);

                canvas.DrawText(TEXT, xText, yText, SKTextAlign.Left, font, paint);
            }
        }
    }
}

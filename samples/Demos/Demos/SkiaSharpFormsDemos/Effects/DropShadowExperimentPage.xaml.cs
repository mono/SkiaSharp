using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public partial class DropShadowExperimentPage : ContentPage
	{
        const string TEXT = "Drop Shadow";

		public DropShadowExperimentPage ()
		{
			InitializeComponent ();
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

            // Get values from sliders
            float dx = (float)dxSlider.Value;
            float dy = (float)dySlider.Value;
            float sigmaX = (float)sigmaXSlider.Value;
            float sigmaY = (float)sigmaYSlider.Value;

            using (SKPaint paint = new SKPaint())
            {
                // Set SKPaint properties
                paint.TextSize = info.Width / 7;
                paint.Color = SKColors.Blue;
                paint.ImageFilter = SKImageFilter.CreateDropShadow(
                                        dx,
                                        dy,
                                        sigmaX,
                                        sigmaY,
                                        SKColors.Red,
                                        SKDropShadowImageFilterShadowMode.DrawShadowAndForeground); 

                SKRect textBounds = new SKRect();
                paint.MeasureText(TEXT, ref textBounds);

                // Center the text in the display rectangle
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                canvas.DrawText(TEXT, xText, yText, paint);
            }
        }
    }
}
using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public partial class ImageBlurExperimentPage : ContentPage
	{
        const string TEXT = "Blur My Text";

        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
                                typeof(MaskBlurExperimentPage),
                                "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg");

        public ImageBlurExperimentPage ()
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

            canvas.Clear(SKColors.Pink);

            // Get values from sliders
            float sigmaX = (float)sigmaXSlider.Value;
            float sigmaY = (float)sigmaYSlider.Value;

            using (SKPaint paint = new SKPaint())
            {
                // Set SKPaint properties
                paint.TextSize = (info.Width - 100) / (TEXT.Length / 2);
                paint.ImageFilter = SKImageFilter.CreateDilate((int)(sigmaX), (int)(sigmaY)); // .CreateBlur(sigmaX, sigmaY);

                // Get text bounds and calculate display rectangle
                SKRect textBounds = new SKRect();
                paint.MeasureText(TEXT, ref textBounds);
                SKRect textRect = new SKRect(0, 0, info.Width, textBounds.Height + 50);

                // Center the text in the display rectangle
                float xText = textRect.Width / 2 - textBounds.MidX;
                float yText = textRect.Height / 2 - textBounds.MidY;

                canvas.DrawText(TEXT, xText, yText, paint);

                // Calculate rectangle for bitmap
                SKRect bitmapRect = new SKRect(0, textRect.Bottom, info.Width, info.Height);
                bitmapRect.Inflate(-50, -50);

                canvas.DrawBitmap(bitmap, bitmapRect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}

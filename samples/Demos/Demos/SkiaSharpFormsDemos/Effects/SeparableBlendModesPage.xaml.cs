using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class SeparableBlendModesPage : ContentPage
    {
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
                            typeof(SeparableBlendModesPage),
                            "SkiaSharpFormsDemos.Media.Banana.jpg"); 

        public SeparableBlendModesPage()
        {
            InitializeComponent();
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (sender == graySlider)
            {
                redSlider.Value = greenSlider.Value = blueSlider.Value = graySlider.Value;
            }

            colorLabel.Text = String.Format("Color = {0:X2} {1:X2} {2:X2}",
                                            (byte)(255 * redSlider.Value),
                                            (byte)(255 * greenSlider.Value),
                                            (byte)(255 * blueSlider.Value));

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Draw bitmap in top half
            SKRect rect = new SKRect(0, 0, info.Width, info.Height / 2);
            canvas.DrawBitmap(bitmap, rect, BitmapStretch.Uniform);

            // Draw bitmap in bottom halr
            rect = new SKRect(0, info.Height / 2, info.Width, info.Height);
            canvas.DrawBitmap(bitmap, rect, BitmapStretch.Uniform);

            // Get values from XAML controls
            SKBlendMode blendMode =
                (SKBlendMode)(blendModePicker.SelectedIndex == -1 ?
                                            0 : blendModePicker.SelectedItem);

            SKColor color = new SKColor((byte)(255 * redSlider.Value),
                                        (byte)(255 * greenSlider.Value),
                                        (byte)(255 * blueSlider.Value));

            // Draw rectangle with blend mode in bottom half
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = color;
                paint.BlendMode = blendMode;
                canvas.DrawRect(rect, paint);
            }
        }
    }
}

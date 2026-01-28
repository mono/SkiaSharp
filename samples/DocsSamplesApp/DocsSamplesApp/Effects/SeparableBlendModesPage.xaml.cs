using System;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Effects
{
    public partial class SeparableBlendModesPage : ContentPage
    {
        SKBitmap? bitmap;

        public SeparableBlendModesPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnPickerSelectedIndexChanged(object? sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
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

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

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

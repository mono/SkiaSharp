using System;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Effects
{
    public partial class NonSeparableBlendModesPage : ContentPage
    {
        SKBitmap? bitmap;
        SKColor color;

        public NonSeparableBlendModesPage()
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
            // Calculate new color based on sliders
            color = SKColor.FromHsl((float)hueSlider.Value,
                                    (float)satSlider.Value,
                                    (float)lumSlider.Value);

            // Use labels to display HSL and RGB color values
            color.ToHsl(out float hue, out float sat, out float lum);

            hslLabel.Text = String.Format("HSL = {0:F0} {1:F0} {2:F0}",
                                          hue, sat, lum);

            rgbLabel.Text = String.Format("RGB = {0:X2} {1:X2} {2:X2}",
                                          color.Red, color.Green, color.Blue);

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

            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);

            // Get blend mode from Picker
            SKBlendMode blendMode =
                (SKBlendMode)(blendModePicker.SelectedIndex == -1 ?
                                            0 : blendModePicker.SelectedItem);

            using (SKPaint paint = new SKPaint())
            {
                paint.Color = color;
                paint.BlendMode = blendMode;
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}

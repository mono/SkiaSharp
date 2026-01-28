using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public partial class ColorAdjustmentPage : ContentPage
    {
        SKBitmap? srcBitmap;
        SKBitmap? dstBitmap;

        public ColorAdjustmentPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            srcBitmap = SKBitmap.Decode(stream);
            dstBitmap = new SKBitmap(srcBitmap.Width, srcBitmap.Height);
            OnSliderValueChanged(null, null);
            canvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (srcBitmap is null || dstBitmap is null)
                return;

            float hueAdjust = (float)hueSlider.Value;
            hueLabel.Text = $"Hue Adjustment: {hueAdjust:F0}";

            float saturationAdjust = (float)Math.Pow(2, saturationSlider.Value);
            saturationLabel.Text = $"Saturation Adjustment: {saturationAdjust:F2}";

            float luminosityAdjust = (float)Math.Pow(2, luminositySlider.Value);
            luminosityLabel.Text = $"Luminosity Adjustment: {luminosityAdjust:F2}";

            TransferPixels(hueAdjust, saturationAdjust, luminosityAdjust);
            canvasView.InvalidateSurface();
        }

        unsafe void TransferPixels(float hueAdjust, float saturationAdjust, float luminosityAdjust)
        {
            byte* srcPtr = (byte*)srcBitmap.GetPixels().ToPointer();
            byte* dstPtr = (byte*)dstBitmap.GetPixels().ToPointer();

            int width = srcBitmap.Width;       // same for both bitmaps
            int height = srcBitmap.Height;

            SKColorType typeOrg = srcBitmap.ColorType;
            SKColorType typeAdj = dstBitmap.ColorType;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    // Get color from original bitmap
                    byte byte1 = *srcPtr++;         // red or blue
                    byte byte2 = *srcPtr++;         // green
                    byte byte3 = *srcPtr++;         // blue or red
                    byte byte4 = *srcPtr++;         // alpha

                    SKColor color = new SKColor();

                    if (typeOrg == SKColorType.Rgba8888)
                    {
                        color = new SKColor(byte1, byte2, byte3, byte4);
                    }
                    else if (typeOrg == SKColorType.Bgra8888)
                    {
                        color = new SKColor(byte3, byte2, byte1, byte4);
                    }

                    // Get HSL components
                    color.ToHsl(out float hue, out float saturation, out float luminosity);

                    // Adjust HSL components based on adjustments
                    hue = (hue + hueAdjust) % 360;
                    saturation = Math.Max(0, Math.Min(100, saturationAdjust * saturation));
                    luminosity = Math.Max(0, Math.Min(100, luminosityAdjust * luminosity));

                    // Recreate color from HSL components
                    color = SKColor.FromHsl(hue, saturation, luminosity);

                    // Store the bytes in the adjusted bitmap
                    if (typeAdj == SKColorType.Rgba8888)
                    {
                        *dstPtr++ = color.Red;
                        *dstPtr++ = color.Green;
                        *dstPtr++ = color.Blue;
                        *dstPtr++ = color.Alpha;
                    }
                    else if (typeAdj == SKColorType.Bgra8888)
                    {
                        *dstPtr++ = color.Blue;
                        *dstPtr++ = color.Green;
                        *dstPtr++ = color.Red;
                        *dstPtr++ = color.Alpha;
                    }
                }
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (dstBitmap is null)
                return;

            canvas.DrawBitmap(dstBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}
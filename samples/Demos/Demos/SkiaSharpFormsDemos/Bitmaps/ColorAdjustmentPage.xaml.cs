using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class ColorAdjustmentPage : ContentPage
    {
        SKBitmap srcBitmap =
            BitmapExtensions.LoadBitmapResource(typeof(FillRectanglePage),
                                                "SkiaSharpFormsDemos.Media.Banana.jpg");
        SKBitmap dstBitmap;

        public ColorAdjustmentPage()
        {
            InitializeComponent();

            dstBitmap = new SKBitmap(srcBitmap.Width, srcBitmap.Height);
            OnSliderValueChanged(null, null);
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
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
            canvas.DrawBitmap(dstBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}
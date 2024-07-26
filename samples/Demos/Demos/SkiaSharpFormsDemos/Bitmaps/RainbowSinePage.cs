using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public class RainbowSinePage : ContentPage
    {
        SKBitmap bitmap;

        public RainbowSinePage()
        {
            Title = "Rainbow Sine";

            bitmap = new SKBitmap(360 * 3, 1024, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            unsafe
            {
                // Pointer to first pixel of bitmap
                uint* basePtr = (uint*)bitmap.GetPixels().ToPointer();

                // Loop through the rows
                for (int row = 0; row < bitmap.Height; row++)
                {
                    // Calculate the sine curve angle and the sine value
                    double angle = 2 * Math.PI * row / bitmap.Height;
                    double sine = Math.Sin(angle);

                    // Loop through the hues
                    for (int hue = 0; hue < 360; hue++)
                    {
                        // Calculate the column
                        int col = (int)(360 + 360 * sine + hue);

                        // Calculate the address
                        uint* ptr = basePtr + bitmap.Width * row + col;

                        // Store the color value
                        *ptr = (uint)SKColor.FromHsl(hue, 100, 50);
                    }
                }
            }

            // Create the SKCanvasView
            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(bitmap, info.Rect);
        }
    }
}

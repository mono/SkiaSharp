using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Basics
{
    public partial class BitmapDissolvePage : ContentPage
    {
        SKBitmap? bitmap1;
        SKBitmap? bitmap2;

        public BitmapDissolvePage()
        {
            InitializeComponent();

            // Load two bitmaps
            _ = LoadBitmap1Async();
            _ = LoadBitmap2Async();
        }

        async Task LoadBitmap1Async()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("SeatedMonkey.jpg");
            bitmap1 = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        async Task LoadBitmap2Async()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("FacePalm.jpg");
            bitmap2 = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
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

            if (bitmap1 is null || bitmap2 is null)
                return;

            // Find rectangle to fit bitmap
            float scale = Math.Min((float)info.Width / bitmap1.Width,
                                   (float)info.Height / bitmap1.Height);
            SKRect rect = SKRect.Create(scale * bitmap1.Width,
                                        scale * bitmap1.Height);
            float x = (info.Width - rect.Width) / 2;
            float y = (info.Height - rect.Height) / 2;
            rect.Offset(x, y);

            // Get progress value from Slider
            float progress = (float)progressSlider.Value;

            // Display two bitmaps with transparency
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = paint.Color.WithAlpha((byte)(0xFF * (1 - progress)));
                canvas.DrawBitmap(bitmap1, rect, paint);

                paint.Color = paint.Color.WithAlpha((byte)(0xFF * progress));
                canvas.DrawBitmap(bitmap2, rect, paint);
            }
        }
    }
}

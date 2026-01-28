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
    public partial class BitmapRotatorPage : ContentPage
    {
        SKBitmap? originalBitmap;
        SKBitmap? rotatedBitmap;

        public BitmapRotatorPage ()
        {
            InitializeComponent ();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            originalBitmap = SKBitmap.Decode(stream);
            rotatedBitmap = originalBitmap;
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (rotatedBitmap is null)
                return;

            canvas.DrawBitmap(rotatedBitmap, info.Rect, BitmapStretch.Uniform);
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (originalBitmap is null)
                return;

            double angle = args.NewValue;
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear(SKColors.LightPink);
                canvas.Translate(rotatedWidth / 2, rotatedHeight / 2);
                canvas.RotateDegrees((float)angle);
                canvas.Translate(-originalWidth / 2, -originalHeight / 2);
                canvas.DrawBitmap(originalBitmap, new SKPoint());
            }

            canvasView.InvalidateSurface();
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public class PixelDimensionsPage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public PixelDimensionsPage()
        {
            Title = "Pixel Dimensions";

            // Create the SKCanvasView and set the PaintSurface handler
            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Load the bitmap from a resource
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);
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

            float x = (info.Width - bitmap.Width) / 2;
            float y = (info.Height - bitmap.Height) / 2;

            canvas.DrawBitmap(bitmap, x, y);
        }
    }
}
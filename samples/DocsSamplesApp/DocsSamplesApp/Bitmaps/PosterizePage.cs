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
    public class PosterizePage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public PosterizePage()
        {
            Title = "Posterize";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);

            unsafe
            {
                uint* ptr = (uint*)bitmap.GetPixels().ToPointer();
                int pixelCount = bitmap.Width * bitmap.Height;

                for (int i = 0; i < pixelCount; i++)
                {
                    *ptr++ &= 0xE0E0E0FF; 
                }
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}
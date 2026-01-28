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
    public class PixelizedImagePage : ContentPage
    {
        SKBitmap? pixelizedBitmap;
        SKCanvasView canvasView;

        public PixelizedImagePage ()
        {
            Title = "Pixelize Image";

            // Create SKCanvasView to view result
            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MountainClimbers.jpg");
            SKBitmap originalBitmap = SKBitmap.Decode(stream);

            // Create tiny bitmap for pixelized face
            SKBitmap faceBitmap = new SKBitmap(9, 9);

            // Copy subset of original bitmap to that
            using (SKCanvas canvas = new SKCanvas(faceBitmap))
            {
                canvas.Clear();
                canvas.DrawBitmap(originalBitmap,
                                  new SKRect(112, 238, 184, 310),   // source
                                  new SKRect(0, 0, 9, 9));          // destination

            }

            // Create full-sized bitmap for copy
            pixelizedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

            using (SKCanvas canvas = new SKCanvas(pixelizedBitmap))
            {
                canvas.Clear();

                // Draw original in full size
                canvas.DrawBitmap(originalBitmap, new SKPoint());

                // Draw tiny bitmap to cover face
                canvas.DrawBitmap(faceBitmap, 
                                  new SKRect(112, 238, 184, 310));  // destination
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (pixelizedBitmap is null)
                return;

            canvas.DrawBitmap(pixelizedBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}
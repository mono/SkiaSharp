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
    public partial class BitmapFlipperPage : ContentPage
    {
        SKBitmap? bitmap;

        public BitmapFlipperPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("SeatedMonkey.jpg");
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

            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);
        }

        void OnFlipVerticalClicked(object? sender, EventArgs args)
        {
            if (bitmap is null)
                return;

            SKBitmap flippedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                canvas.Scale(-1, 1, bitmap.Width / 2, 0);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = flippedBitmap;
            canvasView.InvalidateSurface();
        }

        void OnFlipHorizontalClicked(object? sender, EventArgs args)
        {
            if (bitmap is null)
                return;

            SKBitmap flippedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                canvas.Scale(1, -1, 0, bitmap.Height / 2);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = flippedBitmap;
            canvasView.InvalidateSurface();
        }
    }
}
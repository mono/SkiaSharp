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
    public class PosterizeTablePage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        byte[] colorTable = new byte[256];

        public PosterizeTablePage()
        {
            Title = "Posterize Table";

            // Create color table
            for (int i = 0; i < 256; i++)
            {
                colorTable[i] = (byte)(0xC0 & i);
            }

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MonkeyFace.png");
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

            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter =
                    SKColorFilter.CreateTable(null, null, colorTable, colorTable);

                canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}
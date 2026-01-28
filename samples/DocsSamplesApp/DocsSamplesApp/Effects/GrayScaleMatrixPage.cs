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
	public class GrayScaleMatrixPage : ContentPage
	{
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public GrayScaleMatrixPage()
        {
            Title = "Gray-Scale Matrix";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

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

            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter =
                    SKColorFilter.CreateColorMatrix(new float[]
                    {
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0,     0,     0,     1, 0
                    });

                canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}
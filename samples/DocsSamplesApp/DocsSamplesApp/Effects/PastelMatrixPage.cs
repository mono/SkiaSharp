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
    public class PastelMatrixPage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public PastelMatrixPage()
        {
            Title = "Pastel Matrix";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MountainClimbers.jpg");
            bitmap = SKBitmap.Decode(stream);
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

            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter =
                    SKColorFilter.CreateColorMatrix(new float[]
                    {
                        0.75f, 0.25f, 0.25f, 0, 0,
                        0.25f, 0.75f, 0.25f, 0, 0,
                        0.25f, 0.25f, 0.75f, 0, 0,
                        0, 0, 0, 1, 0
                    });

                canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}
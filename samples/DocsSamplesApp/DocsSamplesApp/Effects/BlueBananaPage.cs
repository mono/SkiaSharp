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
    public class BlueBananaPage : ContentPage
    {
        SKBitmap? bitmap;
        SKBitmap? blueBananaBitmap;
        SKCanvasView canvasView;

        public BlueBananaPage()
        {
            Title = "Blue Banana";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
            _ = LoadMatteBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        async Task LoadMatteBitmapAsync()
        {
            // Load banana matte bitmap (black on transparent)
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("BananaMatte.png");
            SKBitmap matteBitmap = SKBitmap.Decode(stream);

            // Create a bitmap with a solid blue banana and transparent otherwise
            blueBananaBitmap = new SKBitmap(matteBitmap.Width, matteBitmap.Height);

            using (SKCanvas canvas = new SKCanvas(blueBananaBitmap))
            {
                canvas.Clear();
                canvas.DrawBitmap(matteBitmap, new SKPoint(0, 0));

                using (SKPaint paint = new SKPaint())
                {
                    paint.Color = SKColors.Blue;
                    paint.BlendMode = SKBlendMode.SrcIn;
                    canvas.DrawPaint(paint);
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

            if (bitmap is null || blueBananaBitmap is null)
                return;

            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);

            using (SKPaint paint = new SKPaint())
            {
                paint.BlendMode = SKBlendMode.Color;
                canvas.DrawBitmap(blueBananaBitmap,
                                  info.Rect,
                                  BitmapStretch.Uniform,
                                  paint: paint);
            }
        }
    }
}


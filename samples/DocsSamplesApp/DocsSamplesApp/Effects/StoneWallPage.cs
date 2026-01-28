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
    public class StoneWallPage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public StoneWallPage()
        {
            Title = "Stone Wall";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("StoneWallTile.jpg");
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
                // Create scale transform
                SKMatrix matrix = SKMatrix.CreateScale(0.5f, 0.5f);

                // Create bitmap tiling
                paint.Shader = SKShader.CreateBitmap(bitmap,
                                                     SKShaderTileMode.Mirror,
                                                     SKShaderTileMode.Mirror,
                                                     matrix);
                // Draw background
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}

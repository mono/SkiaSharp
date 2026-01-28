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
	public class CenteredTilesPage : ContentPage
	{
        SKBitmap? bitmap;
        SKCanvasView canvasView;

		public CenteredTilesPage ()
		{
            Title = "Centered Tiles";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
		}

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("monkey.png");
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

            // Find coordinates to center bitmap in canvas...
            float x = (info.Width - bitmap.Width) / 2f;
            float y = (info.Height - bitmap.Height) / 2f;

            using (SKPaint paint = new SKPaint())
            {
                // ... but use them to create a translate transform
                SKMatrix matrix = SKMatrix.CreateTranslation(x, y);
                paint.Shader = SKShader.CreateBitmap(bitmap, 
                                                     SKShaderTileMode.Repeat, 
                                                     SKShaderTileMode.Repeat, 
                                                     matrix);

                // Use that tiled bitmap pattern to fill a circle
                canvas.DrawCircle(info.Rect.MidX, info.Rect.MidY,
                                  Math.Min(info.Width, info.Height) / 2,
                                  paint);
            }
        }
    }
}
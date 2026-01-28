using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Curves
{
    public class ClippingTextPage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;

        public ClippingTextPage()
        {
            Title = "Clipping Text";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("PageOfCode.png");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Blue);

            using (SKPaint paint = new SKPaint())
            {
                paint.Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);
                paint.TextSize = 10;

                using (SKPath textPath = paint.GetTextPath("CODE", 0, 0))
                {
                    // Set transform to center and enlarge clip path to window height
                    SKRect bounds;
                    textPath.GetTightBounds(out bounds);

                    canvas.Translate(info.Width / 2, info.Height / 2);
                    canvas.Scale(info.Width / bounds.Width, info.Height / bounds.Height);
                    canvas.Translate(-bounds.MidX, -bounds.MidY);

                    // Set the clip path
                    canvas.ClipPath(textPath);
                }
            }

            // Reset transforms
            canvas.ResetMatrix();

            if (bitmap is null)
                return;

            // Display bitmap to fill window but maintain aspect ratio
            SKRect rect = new SKRect(0, 0, info.Width, info.Height);
            canvas.DrawBitmap(bitmap, 
                rect.AspectFill(new SKSize(bitmap.Width, bitmap.Height)));
        }
    }
}


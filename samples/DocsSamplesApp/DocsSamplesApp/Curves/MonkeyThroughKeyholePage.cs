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
    public class MonkeyThroughKeyholePage : ContentPage
    {
        SKBitmap? bitmap;
        SKCanvasView canvasView;
        SKPath keyholePath = SKPath.ParseSvgPathData(
            "M 300 130 L 250 350 L 450 350 L 400 130 A 70 70 0 1 0 300 130 Z");

        public MonkeyThroughKeyholePage()
        {
            Title = "Monkey through Keyhole";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

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

            // Set transform to center and enlarge clip path to window height
            SKRect bounds;
            keyholePath.GetTightBounds(out bounds);

            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(0.98f * info.Height / bounds.Height);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            // Set the clip path
            canvas.ClipPath(keyholePath);

            // Reset transforms
            canvas.ResetMatrix();

            // Display monkey to fill height of window but maintain aspect ratio
            canvas.DrawBitmap(bitmap, 
                new SKRect((info.Width - info.Height) / 2, 0,
                           (info.Width + info.Height) / 2, info.Height));
        }
    }
}

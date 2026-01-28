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
    public partial class MonkeyMoustachePage : ContentPage
    {
        SKBitmap? monkeyBitmap;
        SKCanvasView canvasView;

        public MonkeyMoustachePage()
        {
            Title = "Monkey Moustache";

            // Create SKCanvasView to view result
            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MonkeyFace.png");
            monkeyBitmap = SKBitmap.Decode(stream);

            // Create canvas based on bitmap
            using (SKCanvas canvas = new SKCanvas(monkeyBitmap))
            {
                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Black;
                    paint.StrokeWidth = 24;
                    paint.StrokeCap = SKStrokeCap.Round;

                    using (SKPath path = new SKPath())
                    {
                        path.MoveTo(380, 390);
                        path.CubicTo(560, 390, 560, 280, 500, 280);

                        path.MoveTo(320, 390);
                        path.CubicTo(140, 390, 140, 280, 200, 280);

                        canvas.DrawPath(path, paint);
                    }
                }
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (monkeyBitmap is null)
                return;

            canvas.DrawBitmap(monkeyBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}
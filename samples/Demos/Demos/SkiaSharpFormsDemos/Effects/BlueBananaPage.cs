using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class BlueBananaPage : ContentPage
    {
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
            typeof(BlueBananaPage),
            "SkiaSharpFormsDemos.Media.Banana.jpg");

        SKBitmap blueBananaBitmap;

        public BlueBananaPage()
        {
            Title = "Blue Banana";

            // Load banana matte bitmap (black on transparent)
            SKBitmap matteBitmap = BitmapExtensions.LoadBitmapResource(
                typeof(BlueBananaPage),
                "SkiaSharpFormsDemos.Media.BananaMatte.png");

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

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

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


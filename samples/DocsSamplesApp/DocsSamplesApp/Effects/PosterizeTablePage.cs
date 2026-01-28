using System;

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
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
                            typeof(PosterizeTablePage),
                            "DocsSamplesApp.Media.MonkeyFace.png");

        byte[] colorTable = new byte[256];

        public PosterizeTablePage()
        {
            Title = "Posterize Table";

            // Create color table
            for (int i = 0; i < 256; i++)
            {
                colorTable[i] = (byte)(0xC0 & i);
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

            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter =
                    SKColorFilter.CreateTable(null, null, colorTable, colorTable);

                canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
            }
        }
    }
}
using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Bitmaps
{
    public class PosterizePage : ContentPage
    {
        SKBitmap bitmap =
            BitmapExtensions.LoadBitmapResource(typeof(FillRectanglePage),
                                                "DocsSamplesApp.Media.Banana.jpg");
        public PosterizePage()
        {
            Title = "Posterize";

            unsafe
            {
                uint* ptr = (uint*)bitmap.GetPixels().ToPointer();
                int pixelCount = bitmap.Width * bitmap.Height;

                for (int i = 0; i < pixelCount; i++)
                {
                    *ptr++ &= 0xE0E0E0FF; 
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
        }
    }
}
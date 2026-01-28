using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Bitmaps
{
    public partial class PhotoPuzzlePage2 : ContentPage
    {
        SKBitmap bitmap;

        public PhotoPuzzlePage2 (SKBitmap bitmap)
        {
            this.bitmap = bitmap;

            InitializeComponent ();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);
        }

        void OnRotateRightButtonClicked(object? sender, EventArgs args)
        {
            SKBitmap rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);

            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear();
                canvas.Translate(bitmap.Height, 0);
                canvas.RotateDegrees(90);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = rotatedBitmap;
            canvasView.InvalidateSurface();
        }

        void OnRotateLeftButtonClicked(object? sender, EventArgs args)
        {
            SKBitmap rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);

            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear();
                canvas.Translate(0, bitmap.Width);
                canvas.RotateDegrees(-90);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = rotatedBitmap;
            canvasView.InvalidateSurface();
        }

        async void OnDoneButtonClicked(object? sender, EventArgs args)
        {
            await Navigation.PushAsync(new PhotoPuzzlePage3(bitmap));
        }
    }
}
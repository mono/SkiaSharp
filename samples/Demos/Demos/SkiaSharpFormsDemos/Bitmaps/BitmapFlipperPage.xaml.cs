using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class BitmapFlipperPage : ContentPage
    {
        SKBitmap bitmap =
            BitmapExtensions.LoadBitmapResource(typeof(BitmapRotatorPage),
                "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg");

        public BitmapFlipperPage()
        {
            InitializeComponent();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform);
        }

        void OnFlipVerticalClicked(object sender, EventArgs args)
        {
            SKBitmap flippedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                canvas.Scale(-1, 1, bitmap.Width / 2, 0);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = flippedBitmap;
            canvasView.InvalidateSurface();
        }

        void OnFlipHorizontalClicked(object sender, EventArgs args)
        {
            SKBitmap flippedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

            using (SKCanvas canvas = new SKCanvas(flippedBitmap))
            {
                canvas.Clear();
                canvas.Scale(1, -1, 0, bitmap.Height / 2);
                canvas.DrawBitmap(bitmap, new SKPoint());
            }

            bitmap = flippedBitmap;
            canvasView.InvalidateSurface();
        }
    }
}
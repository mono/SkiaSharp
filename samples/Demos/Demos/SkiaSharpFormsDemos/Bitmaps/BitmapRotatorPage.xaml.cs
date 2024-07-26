using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class BitmapRotatorPage : ContentPage
    {
        static readonly SKBitmap originalBitmap = 
            BitmapExtensions.LoadBitmapResource(typeof(BitmapRotatorPage),
                "SkiaSharpFormsDemos.Media.Banana.jpg");

        SKBitmap rotatedBitmap = originalBitmap;

        public BitmapRotatorPage ()
        {
            InitializeComponent ();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(rotatedBitmap, info.Rect, BitmapStretch.Uniform);
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            double angle = args.NewValue;
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear(SKColors.LightPink);
                canvas.Translate(rotatedWidth / 2, rotatedHeight / 2);
                canvas.RotateDegrees((float)angle);
                canvas.Translate(-originalWidth / 2, -originalHeight / 2);
                canvas.DrawBitmap(originalBitmap, new SKPoint());
            }

            canvasView.InvalidateSurface();
        }
    }
}
using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Basics
{
    public partial class BitmapDissolvePage : ContentPage
    {
        SKBitmap bitmap1;
        SKBitmap bitmap2;

        public BitmapDissolvePage()
        {
            InitializeComponent();

            // Load two bitmaps
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(
                                    "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg"))
            {
                bitmap1 = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream(
                                    "SkiaSharpFormsDemos.Media.FacePalm.jpg"))
            {
                bitmap2 = SKBitmap.Decode(stream);
            }
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Find rectangle to fit bitmap
            float scale = Math.Min((float)info.Width / bitmap1.Width,
                                   (float)info.Height / bitmap1.Height);
            SKRect rect = SKRect.Create(scale * bitmap1.Width,
                                        scale * bitmap1.Height);
            float x = (info.Width - rect.Width) / 2;
            float y = (info.Height - rect.Height) / 2;
            rect.Offset(x, y);

            // Get progress value from Slider
            float progress = (float)progressSlider.Value;

            // Display two bitmaps with transparency
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = paint.Color.WithAlpha((byte)(0xFF * (1 - progress)));
                canvas.DrawBitmap(bitmap1, rect, paint);

                paint.Color = paint.Color.WithAlpha((byte)(0xFF * progress));
                canvas.DrawBitmap(bitmap2, rect, paint);
            }
        }
    }
}

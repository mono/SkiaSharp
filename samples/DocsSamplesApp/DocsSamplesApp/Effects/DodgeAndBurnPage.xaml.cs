using System;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Effects
{
    public partial class DodgeAndBurnPage : ContentPage
    {
        SKBitmap? bitmap;

        public DodgeAndBurnPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);
            dodgeCanvasView.InvalidateSurface();
            burnCanvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object? sender, ValueChangedEventArgs args)
        {
            if (sender == dodgeSlider)
            {
                dodgeCanvasView.InvalidateSurface();
            }
            else
            {
                burnCanvasView.InvalidateSurface();
            }
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

            // Find largest size rectangle in canvas
            float scale = Math.Min((float)info.Width / bitmap.Width,
                                   (float)info.Height / bitmap.Height);
            SKRect rect = SKRect.Create(scale * bitmap.Width, scale * bitmap.Height);
            float x = (info.Width - rect.Width) / 2;
            float y = (info.Height - rect.Height) / 2;
            rect.Offset(x, y);

            // Display bitmap
            canvas.DrawBitmap(bitmap, rect);

            // Display gray rectangle with blend mode
            using (SKPaint paint = new SKPaint())
            {
                if (sender == dodgeCanvasView)
                {
                    byte value = (byte)(255 * dodgeSlider.Value);
                    paint.Color = new SKColor(value, value, value);
                    paint.BlendMode = SKBlendMode.ColorDodge;
                }
                else
                {
                    byte value = (byte)(255 * (1 - burnSlider.Value));
                    paint.Color = new SKColor(value, value, value);
                    paint.BlendMode = SKBlendMode.ColorBurn;
                }

                canvas.DrawRect(rect, paint);
            }
        }
    }
}
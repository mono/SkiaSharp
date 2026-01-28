using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Transforms
{
    public partial class TestPerspectivePage : ContentPage
    {
        SKBitmap? bitmap;

        public TestPerspectivePage()
        {
            InitializeComponent();

            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("SeatedMonkey.jpg");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnPersp0SliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            Slider slider = (Slider)sender;
            persp0Label.Text = String.Format("Persp0 = {0:F4}", slider.Value / 100);
            canvasView.InvalidateSurface();
        }

        void OnPersp1SliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            Slider slider = (Slider)sender;
            persp1Label.Text = String.Format("Persp1 = {0:F4}", slider.Value / 100);
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

            // Calculate perspective matrix
            SKMatrix perspectiveMatrix = SKMatrix.Identity;
            perspectiveMatrix.Persp0 = (float)persp0Slider.Value / 100;
            perspectiveMatrix.Persp1 = (float)persp1Slider.Value / 100;

            // Center of screen
            float xCenter = info.Width / 2;
            float yCenter = info.Height / 2;

            SKMatrix matrix = SKMatrix.CreateTranslation(-xCenter, -yCenter);
            matrix = matrix.PostConcat(perspectiveMatrix);
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(xCenter, yCenter));

            // Coordinates to center bitmap on canvas
            float x = xCenter - bitmap.Width / 2;
            float y = yCenter - bitmap.Height / 2;

            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, x, y);
        }
    }
}
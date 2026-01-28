using System.IO;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;

namespace DocsSamplesApp.Transforms
{
    public partial class Rotation3DPage : ContentPage
    {
        SKBitmap? bitmap;

        public Rotation3DPage()
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

        void OnSliderValueChanged(object? sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
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

            // Find center of canvas
            float xCenter = info.Width / 2;
            float yCenter = info.Height / 2;

            // Translate center to origin
            SKMatrix matrix = SKMatrix.CreateTranslation(-xCenter, -yCenter);

            // Build 3D rotation matrix using *= operator
            SKMatrix44 matrix44 = SKMatrix44.CreateIdentity();
            matrix44 *= SKMatrix44.CreateRotationDegrees(1, 0, 0, (float)xRotateSlider.Value);
            matrix44 *= SKMatrix44.CreateRotationDegrees(0, 1, 0, (float)yRotateSlider.Value);
            matrix44 *= SKMatrix44.CreateRotationDegrees(0, 0, 1, (float)zRotateSlider.Value);

            // Apply perspective: set element [2,3] which affects the w-divide
            // In SKMatrix44, perspective is achieved by modifying how z affects w
            float depth = (float)depthSlider.Value;
            SKMatrix44 perspectiveMatrix = SKMatrix44.CreateIdentity();
            perspectiveMatrix[2, 3] = -1 / depth;
            matrix44 *= perspectiveMatrix;

            // Concatenate with 2D matrix
            matrix = matrix.PostConcat(matrix44.Matrix);

            // Translate back to center
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(xCenter, yCenter));

            // Set the matrix and display the bitmap
            canvas.SetMatrix(matrix);
            float xBitmap = xCenter - bitmap.Width / 2;
            float yBitmap = yCenter - bitmap.Height / 2;
            canvas.DrawBitmap(bitmap, xBitmap, yBitmap);
        }
    }
}

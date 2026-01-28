using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Transforms
{
    public partial class SingleFingerCornerScalePage : ContentPage
    {
        SKBitmap? bitmap;
        SKMatrix currentMatrix = SKMatrix.Identity;

        // Information for translating and scaling
        long? touchId = null;
        SKPoint pressedLocation;
        SKMatrix pressedMatrix;

        // Information for scaling
        bool isScaling;
        SKPoint pivotPoint;

        public SingleFingerCornerScalePage()
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

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

            canvas.SetMatrix(currentMatrix);
            canvas.DrawBitmap(bitmap, 0, 0);
        }

        void OnTouch(object? sender, SKTouchEventArgs e)
        {
            if (bitmap is null)
                return;

            // Location is already in pixels with built-in touch
            SKPoint point = e.Location;

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Track only one finger
                    if (touchId.HasValue)
                        return;

                    // Check if the finger is within the boundaries of the bitmap
                    SKRect rect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
                    rect = currentMatrix.MapRect(rect);
                    if (!rect.Contains(point))
                        return;

                    // First assume there will be no scaling
                    isScaling = false;

                    // If touch is outside interior ellipse, make this a scaling operation
                    if (Math.Pow((point.X - rect.MidX) / (rect.Width / 2), 2) +
                        Math.Pow((point.Y - rect.MidY) / (rect.Height / 2), 2) > 1)
                    {
                        isScaling = true;
                        float xPivot = point.X < rect.MidX ? rect.Right : rect.Left;
                        float yPivot = point.Y < rect.MidY ? rect.Bottom : rect.Top;
                        pivotPoint = new SKPoint(xPivot, yPivot);
                    }

                    // Common for either pan or scale
                    touchId = e.Id;
                    pressedLocation = point;
                    pressedMatrix = currentMatrix;
                    break;

                case SKTouchAction.Moved:
                    if (!touchId.HasValue || e.Id != touchId.Value)
                        return;

                    SKMatrix matrix = SKMatrix.Identity;

                    // Translating
                    if (!isScaling)
                    {
                        SKPoint delta = point - pressedLocation;
                        matrix = SKMatrix.CreateTranslation(delta.X, delta.Y);
                    }
                    // Scaling
                    else
                    {
                        float scaleX = (point.X - pivotPoint.X) / (pressedLocation.X - pivotPoint.X);
                        float scaleY = (point.Y - pivotPoint.Y) / (pressedLocation.Y - pivotPoint.Y);
                        matrix = SKMatrix.CreateScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y);
                    }

                    // Concatenate the matrices
                    matrix = matrix.PreConcat(pressedMatrix);
                    currentMatrix = matrix;
                    canvasView.InvalidateSurface();
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    touchId = null;
                    break;
            }

            e.Handled = true;
        }
    }
}

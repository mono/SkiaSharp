using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using TouchTracking;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class SingleFingerCornerScalePage : ContentPage
    {
        SKBitmap bitmap;
        SKMatrix currentMatrix = SKMatrix.MakeIdentity();

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

            string resourceID = "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                bitmap = SKBitmap.Decode(stream);
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.SetMatrix(currentMatrix);
            canvas.DrawBitmap(bitmap, 0, 0);
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            // Convert Xamarin.Forms point to pixels
            Point pt = args.Location;
            SKPoint point =
                new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                            (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));

            switch (args.Type)
            {
                case TouchActionType.Pressed:
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
                    touchId = args.Id;
                    pressedLocation = point;
                    pressedMatrix = currentMatrix;
                    break;

                case TouchActionType.Moved:
                    if (!touchId.HasValue || args.Id != touchId.Value)
                        return;

                    SKMatrix matrix = SKMatrix.MakeIdentity();

                    // Translating
                    if (!isScaling)
                    {
                        SKPoint delta = point - pressedLocation;
                        matrix = SKMatrix.MakeTranslation(delta.X, delta.Y);
                    }
                    // Scaling
                    else
                    {
                        float scaleX = (point.X - pivotPoint.X) / (pressedLocation.X - pivotPoint.X);
                        float scaleY = (point.Y - pivotPoint.Y) / (pressedLocation.Y - pivotPoint.Y);
                        matrix = SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y);
                    }

                    // Concatenate the matrices
                    SKMatrix.PreConcat(ref matrix, pressedMatrix);
                    currentMatrix = matrix;
                    canvasView.InvalidateSurface();
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    touchId = null;
                    break;
            }
        }
    }
}

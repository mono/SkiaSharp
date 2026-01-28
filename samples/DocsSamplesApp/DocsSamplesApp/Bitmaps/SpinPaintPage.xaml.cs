using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace DocsSamplesApp.Bitmaps
{
    public partial class SpinPaintPage : ContentPage
    {
        // These should be contrasting colors
        static readonly SKColor backgroundColor = SKColors.Black;
        static readonly SKColor crossHairColor = SKColors.White;

        // Current bitmap being drawn upon by user
        SKBitmap? bitmap;
        SKCanvas? bitmapCanvas;
        int bitmapSize;                 // bitmaps used here are always square

        // SKPaint for user drawings on bitmap
        SKPaint fingerPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
        };

        // SKPaint for crosshairs on bitmap
        SKPaint thinLinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = crossHairColor
        };

        // SKPath for clipping drawings to circle
        SKPath clipPath = new SKPath();

        // Animation helpers
        Stopwatch stopwatch = new Stopwatch();
        float angle;

        // Item to store in touch-tracking dictionary
        class FingerInfo
        {
            public SKPoint ThisPosition;
            public SKPoint LastPosition;
        }

        // Touch-tracking dictionary for tracking multiple fingers
        Dictionary<long, FingerInfo> idDictionary = new Dictionary<long, FingerInfo>();

        public SpinPaintPage()
        {
            InitializeComponent();

            // Start animation
            stopwatch.Start();
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        // Handle touch events from SKCanvasView
        void OnTouch(object? sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (e.InContact)
                    {
                        idDictionary[e.Id] = new FingerInfo
                        {
                            ThisPosition = e.Location,
                            LastPosition = new SKPoint(float.PositiveInfinity, float.PositiveInfinity)
                        };
                    }
                    break;

                case SKTouchAction.Moved:
                    if (idDictionary.ContainsKey(e.Id))
                    {
                        idDictionary[e.Id].ThisPosition = e.Location;
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (idDictionary.ContainsKey(e.Id))
                    {
                        idDictionary.Remove(e.Id);
                    }
                    break;
            }

            e.Handled = true;
        }

        // Every 1/60th second, update bitmap with user drawings
        bool OnTimerTick()
        {
            if (bitmap == null || bitmapCanvas == null)
            {
                return true;
            }

            // Determine the current color.
            float tColor = stopwatch.ElapsedMilliseconds % 10000 / 10000f;
            fingerPaint.Color = SKColor.FromHsl(360 * tColor, 100, 50);
            titleLabel.TextColor = fingerPaint.Color.ToMauiColor();

            // Determine the rotation angle.
            float tAngle = stopwatch.ElapsedMilliseconds % 5000 / 5000f;
            angle = 360 * tAngle;
            SKMatrix matrix = SKMatrix.CreateRotationDegrees(-angle, bitmap.Width / 2, bitmap.Height / 2);

            // Loop through the fingers touching the screen.
            foreach (long id in idDictionary.Keys)
            {
                FingerInfo fingerInfo = idDictionary[id];

                // Touch coordinates are already in pixels from SKTouchEventArgs
                SKPoint pt0 = matrix.MapPoint(fingerInfo.ThisPosition);

                if (!float.IsPositiveInfinity(fingerInfo.LastPosition.X))
                {
                    // Draw four lines in four quadrants.
                    SKPoint pt1 = fingerInfo.LastPosition;
                    bitmapCanvas.DrawLine(pt0.X, pt0.Y, pt1.X, pt1.Y, fingerPaint);

                    float x0Flip = bitmap.Width - pt0.X;
                    float y0Flip = bitmap.Height - pt0.Y;
                    float x1Flip = bitmap.Width - pt1.X;
                    float y1Flip = bitmap.Height - pt1.Y;

                    bitmapCanvas.DrawLine(x0Flip, pt0.Y, x1Flip, pt1.Y, fingerPaint);
                    bitmapCanvas.DrawLine(pt0.X, y0Flip, pt1.X, y1Flip, fingerPaint);
                    bitmapCanvas.DrawLine(x0Flip, y0Flip, x1Flip, y1Flip, fingerPaint);
                }

                // Save the current point for next time through.
                fingerInfo.LastPosition = pt0;
            }

            // Redraw the canvas.
            canvasView.InvalidateSurface();
            return true;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            OnPaintSurface(args.Surface, args.Info.Width, args.Info.Height);
        }

        void OnPaintSurface(SKSurface surface, int width, int height)
        {
            // Get the canvas
            SKCanvas canvas = surface.Canvas;

            // These two dimensions should be the same.
            int canvasSize = Math.Min(width, height);

            // If bitmap does not exist, create it
            if (bitmap == null)
            {
                // Set three fields
                bitmapSize = canvasSize;
                bitmap = new SKBitmap(bitmapSize, bitmapSize);
                bitmapCanvas = new SKCanvas(bitmap);

                // Establishes circular clipping and colors background
                PrepBitmap(bitmapCanvas, bitmapSize);
            }

            // If the canvas has become larger, make a new bitmap of that size.
            else if (bitmapSize < canvasSize)
            {
                // New versions of the three fields
                int newBitmapSize = canvasSize;
                SKBitmap newBitmap = new SKBitmap(newBitmapSize, newBitmapSize);
                SKCanvas newBitmapCanvas = new SKCanvas(newBitmap);

                // New circular clipping and background
                PrepBitmap(newBitmapCanvas, newBitmapSize);

                // Copy old bitmap to new bitmap
                float diff = (newBitmapSize - bitmapSize) / 2f;
                newBitmapCanvas.DrawBitmap(bitmap, diff, diff);

                // Dispose old bitmap and its canvas
                bitmapCanvas?.Dispose();
                bitmap.Dispose();

                // Set fields to new values
                bitmap = newBitmap;
                bitmapCanvas = newBitmapCanvas;
                bitmapSize = newBitmapSize;
            }

            // Clear the canvas
            canvas.Clear(SKColors.White);

            // Set the rotate transform
            float radius = canvasSize / 2;
            canvas.RotateDegrees(angle, radius, radius);

            // Set a circular clipping area
            clipPath.Reset();
            clipPath.AddCircle(radius, radius, radius);
            canvas.ClipPath(clipPath);

            // Draw the bitmap
            float offset = (canvasSize - bitmapSize) / 2f;
            canvas.DrawBitmap(bitmap, offset, offset);

            // Draw the cross hairs
            canvas.DrawLine(radius, 0, radius, canvasSize, thinLinePaint);
            canvas.DrawLine(0, radius, canvasSize, radius, thinLinePaint);
        }

        static void PrepBitmap(SKCanvas bitmapCanvas, int bitmapSize)
        {
            // Set clipping path based on bitmap size
            using (SKPath bitmapClipPath = new SKPath())
            {
                bitmapClipPath.AddCircle(bitmapSize / 2, bitmapSize / 2, bitmapSize / 2);
                bitmapCanvas.ClipPath(bitmapClipPath);
            }

            // Color the bitmap background
            bitmapCanvas.Clear(backgroundColor);
        }

        // Clear the bitmap of all user drawings.
        void OnClearButtonClicked(object? sender, EventArgs args)
        {
            // Color the bitmap background, erasing all user drawing
            bitmapCanvas?.Clear(backgroundColor);
        }

        // Save the bitmap using Community Toolkit FileSaver
        async void OnSaveButtonClicked(object? sender, EventArgs args)
        {
            if (bitmap == null)
                return;

            // Get this SKImage data as PNG
            SKData data = SKImage.FromBitmap(bitmap).Encode();

            // Fabricate a filename based on date and time.
            DateTime dt = DateTime.Now;
            string filename = String.Format("SpinPaint-{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}.png",
                                            dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);

            try
            {
                using var stream = new MemoryStream(data.ToArray());
                var result = await FileSaver.Default.SaveAsync(filename, stream, CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    await DisplayAlertAsync("SpinPaint", "Artwork could not be saved. Sorry!", "OK");
                }
            }
            catch (Exception)
            {
                await DisplayAlertAsync("SpinPaint", "Artwork could not be saved. Sorry!", "OK");
            }
        }
    }
}
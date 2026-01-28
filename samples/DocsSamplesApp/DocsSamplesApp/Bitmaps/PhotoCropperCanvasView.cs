using System;
using System.Collections.Generic;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

namespace DocsSamplesApp.Bitmaps
{
    class PhotoCropperCanvasView : SKCanvasView
    {
        const int CORNER = 50;      // pixel length of cropper corner
        const int RADIUS = 100;     // pixel radius of touch hit-test

        SKBitmap bitmap;
        CroppingRectangle croppingRect;
        SKMatrix inverseBitmapMatrix;

        // Touch tracking 
        struct TouchPointInfo
        {
            public int CornerIndex { set; get; }
            public SKPoint Offset { set; get; }
        }

        Dictionary<long, TouchPointInfo> touchPoints = new Dictionary<long, TouchPointInfo>();

        // Drawing objects
        SKPaint cornerStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 10
        };

        SKPaint edgeStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 2
        };

        public PhotoCropperCanvasView(SKBitmap bitmap, float? aspectRatio = null)
        {
            this.bitmap = bitmap;

            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            croppingRect = new CroppingRectangle(bitmapRect, aspectRatio);

            // Enable built-in touch events
            EnableTouchEvents = true;
            Touch += OnTouch;
        }

        public SKBitmap CroppedBitmap
        {
            get
            {
                SKRect cropRect = croppingRect.Rect;
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width, 
                                                      (int)cropRect.Height);
                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKRect source = new SKRect(cropRect.Left, cropRect.Top, 
                                           cropRect.Right, cropRect.Bottom);

                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    canvas.DrawBitmap(bitmap, source, dest);
                }

                return croppedBitmap;
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Gray);

            // Calculate rectangle for displaying bitmap 
            float scale = Math.Min((float)info.Width / bitmap.Width, (float)info.Height / bitmap.Height);
            float x = (info.Width - scale * bitmap.Width) / 2;
            float y = (info.Height - scale * bitmap.Height) / 2;
            SKRect bitmapRect = new SKRect(x, y, x + scale * bitmap.Width, y + scale * bitmap.Height);
            canvas.DrawBitmap(bitmap, bitmapRect);

            // Calculate a matrix transform for displaying the cropping rectangle
            SKMatrix bitmapScaleMatrix = SKMatrix.CreateScaleTranslation(scale, scale, x, y);

            // Display rectangle
            SKRect scaledCropRect = bitmapScaleMatrix.MapRect(croppingRect.Rect);
            canvas.DrawRect(scaledCropRect, edgeStroke);

            // Display heavier corners
            using (SKPath path = new SKPath())
            {
                path.MoveTo(scaledCropRect.Left, scaledCropRect.Top + CORNER);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Left + CORNER, scaledCropRect.Top);

                path.MoveTo(scaledCropRect.Right - CORNER, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Top + CORNER);

                path.MoveTo(scaledCropRect.Right, scaledCropRect.Bottom - CORNER);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Right - CORNER, scaledCropRect.Bottom);

                path.MoveTo(scaledCropRect.Left + CORNER, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Bottom - CORNER);

                canvas.DrawPath(path, cornerStroke);
            }

            // Invert the transform for touch tracking
            bitmapScaleMatrix.TryInvert(out inverseBitmapMatrix);
        }

        void OnTouch(object sender, SKTouchEventArgs e)
        {
            // Location is already in pixels with built-in touch
            SKPoint pixelLocation = e.Location;
            SKPoint bitmapLocation = inverseBitmapMatrix.MapPoint(pixelLocation);

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Convert radius to bitmap/cropping scale
                    float radius = inverseBitmapMatrix.ScaleX * RADIUS;

                    // Find corner that the finger is touching
                    int cornerIndex = croppingRect.HitTest(bitmapLocation, radius);

                    if (cornerIndex != -1 && !touchPoints.ContainsKey(e.Id))
                    {
                        TouchPointInfo touchPoint = new TouchPointInfo
                        {
                            CornerIndex = cornerIndex,
                            Offset = bitmapLocation - croppingRect.Corners[cornerIndex]
                        };

                        touchPoints.Add(e.Id, touchPoint);
                    }
                    break;

                case SKTouchAction.Moved:
                    if (touchPoints.ContainsKey(e.Id))
                    {
                        TouchPointInfo touchPoint = touchPoints[e.Id];
                        croppingRect.MoveCorner(touchPoint.CornerIndex, 
                                                bitmapLocation - touchPoint.Offset);
                        InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (touchPoints.ContainsKey(e.Id))
                    {
                        touchPoints.Remove(e.Id);
                    }
                    break;
            }

            e.Handled = true;
        }
    }
}

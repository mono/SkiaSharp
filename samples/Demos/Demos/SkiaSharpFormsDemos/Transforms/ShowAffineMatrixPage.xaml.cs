using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using TouchTracking;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class ShowAffineMatrixPage : ContentPage
    {
        SKMatrix matrix;
        SKBitmap bitmap;
        SKSize bitmapSize;

        TouchPoint[] touchPoints = new TouchPoint[3];

        MatrixDisplay matrixDisplay = new MatrixDisplay();

        public ShowAffineMatrixPage()
        {
            InitializeComponent();

            string resourceID = "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                bitmap = SKBitmap.Decode(stream);
            }

            touchPoints[0] = new TouchPoint(100, 100);                  // upper-left corner
            touchPoints[1] = new TouchPoint(bitmap.Width + 100, 100);   // upper-right corner
            touchPoints[2] = new TouchPoint(100, bitmap.Height + 100);  // lower-left corner

            bitmapSize = new SKSize(bitmap.Width, bitmap.Height);
            matrix = ComputeMatrix(bitmapSize, touchPoints[0].Center, 
                                               touchPoints[1].Center, 
                                               touchPoints[2].Center);
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            bool touchPointMoved = false;

            foreach (TouchPoint touchPoint in touchPoints)
            {
                float scale = canvasView.CanvasSize.Width / (float)canvasView.Width;
                SKPoint point = new SKPoint(scale * (float)args.Location.X,
                                            scale * (float)args.Location.Y);
                touchPointMoved |= touchPoint.ProcessTouchEvent(args.Id, args.Type, point);
            }

            if (touchPointMoved)
            {
                matrix = ComputeMatrix(bitmapSize, touchPoints[0].Center, 
                                                   touchPoints[1].Center, 
                                                   touchPoints[2].Center);
                canvasView.InvalidateSurface();
            }
        }

        static SKMatrix ComputeMatrix(SKSize size, SKPoint ptUL, SKPoint ptUR, SKPoint ptLL)
        {
            // Scale transform
            SKMatrix S = SKMatrix.MakeScale(1 / size.Width, 1 / size.Height);

            // Affine transform
            SKMatrix A = new SKMatrix
            {
                ScaleX = ptUR.X - ptUL.X,
                SkewY = ptUR.Y - ptUL.Y,
                SkewX = ptLL.X - ptUL.X,
                ScaleY = ptLL.Y - ptUL.Y,
                TransX = ptUL.X,
                TransY = ptUL.Y,
                Persp2 = 1
            };

            SKMatrix result = SKMatrix.MakeIdentity();
            SKMatrix.Concat(ref result, A, S);
            return result;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Display the bitmap using the matrix
            canvas.Save();
            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, 0, 0);
            canvas.Restore();

            // Display the matrix in the lower-right corner
            SKSize matrixSize = matrixDisplay.Measure(matrix);

            matrixDisplay.Paint(canvas, matrix,
                new SKPoint(info.Width - matrixSize.Width,
                            info.Height - matrixSize.Height));

            // Display the touchpoints
            foreach (TouchPoint touchPoint in touchPoints)
            {
                touchPoint.Paint(canvas);
            }
        }
    }
}
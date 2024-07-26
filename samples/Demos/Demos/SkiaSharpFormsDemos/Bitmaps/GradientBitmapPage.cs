using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
	public class GradientBitmapPage : ContentPage
	{
        const int REPS = 100;

        Stopwatch stopwatch = new Stopwatch();

        string[] descriptions = new string[8];
        SKBitmap[] bitmaps = new SKBitmap[8];
        int[] elapsedTimes = new int[8];

        SKCanvasView canvasView;

        public GradientBitmapPage ()
        {
            Title = "Gradient Bitmap";

            bitmaps[0] = FillBitmapSetPixel(out descriptions[0], out elapsedTimes[0]);
            bitmaps[1] = FillBitmapPixelsProp(out descriptions[1], out elapsedTimes[1]);
            bitmaps[2] = FillBitmapBytePtr(out descriptions[2], out elapsedTimes[2]);
            bitmaps[4] = FillBitmapUintPtr(out descriptions[4], out elapsedTimes[4]);
            bitmaps[6] = FillBitmapUintPtrColor(out descriptions[6], out elapsedTimes[6]);
            bitmaps[3] = FillBitmapByteBuffer(out descriptions[3], out elapsedTimes[3]);
            bitmaps[5] = FillBitmapUintBuffer(out descriptions[5], out elapsedTimes[5]);
            bitmaps[7] = FillBitmapUintBufferColor(out descriptions[7], out elapsedTimes[7]);

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        SKBitmap FillBitmapSetPixel(out string description, out int milliseconds)
        {
            description = "SetPixel";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            for (int rep = 0; rep < REPS; rep++)
                for (int row = 0; row < 256; row++)
                    for (int col = 0; col < 256; col++)
                    {
                        bitmap.SetPixel(col, row, new SKColor((byte)col, 0, (byte)row));
                    }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapPixelsProp(out string description, out int milliseconds)
        {
            description = "Pixels property";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            SKColor[] pixels = new SKColor[256 * 256]; 

            for (int rep = 0; rep < REPS; rep++)
                for (int row = 0; row < 256; row++)
                    for (int col = 0; col < 256; col++)
                    {
                        pixels[256 * row + col] = new SKColor((byte)col, 0, (byte)row);
                    }

            bitmap.Pixels = pixels;

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapBytePtr(out string description, out int milliseconds)
        {
            description = "GetPixels byte ptr";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            IntPtr pixelsAddr = bitmap.GetPixels();

            unsafe
            {
                for (int rep = 0; rep < REPS; rep++)
                {
                    byte* ptr = (byte*)pixelsAddr.ToPointer();

                    for (int row = 0; row < 256; row++)
                        for (int col = 0; col < 256; col++)
                        {
                            *ptr++ = (byte)(col);   // red
                            *ptr++ = 0;             // green
                            *ptr++ = (byte)(row);   // blue
                            *ptr++ = 0xFF;          // alpha
                        }
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapUintPtr(out string description, out int milliseconds)
        {
            description = "GetPixels uint ptr";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            IntPtr pixelsAddr = bitmap.GetPixels();

            unsafe
            {
                for (int rep = 0; rep < REPS; rep++)
                {
                    uint* ptr = (uint*)pixelsAddr.ToPointer();

                    for (int row = 0; row < 256; row++)
                        for (int col = 0; col < 256; col++)
                        {
                            *ptr++ = MakePixel((byte)col, 0, (byte)row, 0xFF);
                        }
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapUintPtrColor(out string description, out int milliseconds)
        {
            description = "GetPixels SKColor";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            IntPtr pixelsAddr = bitmap.GetPixels();

            unsafe
            {
                for (int rep = 0; rep < REPS; rep++)
                {
                    uint* ptr = (uint*)pixelsAddr.ToPointer();

                    for (int row = 0; row < 256; row++)
                        for (int col = 0; col < 256; col++)
                        {
                            *ptr++ = (uint)new SKColor((byte)col, 0, (byte)row);
                        }
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapByteBuffer(out string description, out int milliseconds)
        {
            description = "SetPixels byte buffer";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            byte[,,] buffer = new byte[256, 256, 4];

            for (int rep = 0; rep < REPS; rep++)
                for (int row = 0; row < 256; row++)
                    for (int col = 0; col < 256; col++)
                    {
                        buffer[row, col, 0] = (byte)col;   // red
                        buffer[row, col, 1] = 0;           // green
                        buffer[row, col, 2] = (byte)row;   // blue
                        buffer[row, col, 3] = 0xFF;        // alpha
                    }
    
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    bitmap.SetPixels((IntPtr)ptr);
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapUintBuffer(out string description, out int milliseconds)
        {
            description = "SetPixels uint buffer";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            uint[,] buffer = new uint[256, 256];

            for (int rep = 0; rep < REPS; rep++)
                for (int row = 0; row < 256; row++)
                    for (int col = 0; col < 256; col++)
                    {
                        buffer[row, col] = MakePixel((byte)col, 0, (byte)row, 0xFF);
                    }

            unsafe
            {
                fixed (uint* ptr = buffer)
                {
                    bitmap.SetPixels((IntPtr)ptr);
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        SKBitmap FillBitmapUintBufferColor(out string description, out int milliseconds)
        {
            description = "SetPixels SKColor";
            SKBitmap bitmap = new SKBitmap(256, 256);

            stopwatch.Restart();

            uint[,] buffer = new uint[256, 256];

            for (int rep = 0; rep < REPS; rep++)
                for (int row = 0; row < 256; row++)
                    for (int col = 0; col < 256; col++)
                    {
                        buffer[row, col] = (uint)new SKColor((byte)col, 0, (byte)row);
                    }

            unsafe
            {
                fixed (uint* ptr = buffer)
                {
                    bitmap.SetPixels((IntPtr)ptr);
                }
            }

            milliseconds = (int)stopwatch.ElapsedMilliseconds;
            return bitmap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint MakePixel(byte red, byte green, byte blue, byte alpha) =>
                (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            int width = info.Width;
            int height = info.Height;

            canvas.Clear();

            Display(canvas, 0, new SKRect(0, 0, width / 2, height / 4));
            Display(canvas, 1, new SKRect(width / 2, 0, width, height / 4));
            Display(canvas, 2, new SKRect(0, height / 4, width / 2, 2 * height / 4));
            Display(canvas, 3, new SKRect(width / 2, height / 4, width, 2 * height / 4));
            Display(canvas, 4, new SKRect(0, 2 * height / 4, width / 2, 3 * height / 4));
            Display(canvas, 5, new SKRect(width / 2, 2 * height / 4, width, 3 * height / 4));
            Display(canvas, 6, new SKRect(0, 3 * height / 4, width / 2, height));
            Display(canvas, 7, new SKRect(width / 2, 3 * height / 4, width, height));
        }

        void Display(SKCanvas canvas, int index, SKRect rect)
        {
            string text = String.Format("{0}: {1:F2} msec", descriptions[index], 
                                        (double)elapsedTimes[index] / REPS);

            SKRect bounds = new SKRect();

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.TextSize = (float)(12 * canvasView.CanvasSize.Width / canvasView.Width);
                textPaint.TextAlign = SKTextAlign.Center;
                textPaint.MeasureText("Tly", ref bounds);

                canvas.DrawText(text, new SKPoint(rect.MidX, rect.Bottom - bounds.Bottom), textPaint);
                rect.Bottom -= bounds.Height;
                canvas.DrawBitmap(bitmaps[index], rect, BitmapStretch.Uniform);
            }
        }
    }
}
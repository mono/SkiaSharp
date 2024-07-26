using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace MandelAnima
{
	public partial class MainPage : ContentPage
	{
        const int COUNT = 10;           // The number of bitmaps in the animation.
                                        // This can go up to 50!

        const int BITMAP_SIZE = 1000;   // Program uses square bitmaps exclusively

        // Uncomment just one of these, or define your own
        static readonly Complex center = new Complex(-1.17651152924355, 0.298520986549558);
        //   static readonly Complex center = new Complex(-0.774693089457127, 0.124226621261617);
        //   static readonly Complex center = new Complex(-0.556624880053304, 0.634696788141351);

        SKBitmap[] bitmaps = new SKBitmap[COUNT];   // array of bitmaps

        Stopwatch stopwatch = new Stopwatch();      // for the animation
        int bitmapIndex;
        double bitmapProgress = 0;

        public MainPage()
		{
			InitializeComponent();

            LoadAndStartAnimation();
		}

        // File path for storing each bitmap in local storage
        string FolderPath() => 
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string FilePath(int zoomLevel) => 
            Path.Combine(FolderPath(),
                         String.Format("R{0}I{1}Z{2:D2}.png", center.Real, center.Imaginary, zoomLevel));

        // Form bitmap pixel for Rgba8888 format
        uint MakePixel(byte alpha, byte red, byte green, byte blue) =>
            (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);

        async void LoadAndStartAnimation()
        {
            // Show total bitmap storage
            TallyBitmapSizes();

            // Create progressReporter for async operation
            Progress<double> progressReporter =
                new Progress<double>((double progress) => progressBar.Progress = progress);

            // Create (unused) CancellationTokenSource for async operation
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

            // Loop through all the zoom levels
            for (int zoomLevel = 0; zoomLevel < COUNT; zoomLevel++)
            {
                // If the file exists, load it
                if (File.Exists(FilePath(zoomLevel)))
                {
                    statusLabel.Text = $"Loading bitmap for zoom level {zoomLevel}";

                    using (Stream stream = File.OpenRead(FilePath(zoomLevel)))
                    {
                        bitmaps[zoomLevel] = SKBitmap.Decode(stream);
                    }
                }
                // Otherwise, create a new bitmap
                else
                {
                    statusLabel.Text = $"Creating bitmap for zoom level {zoomLevel}";

                    CancellationToken cancelToken = cancelTokenSource.Token;

                    // Do the (generally lengthy) Mandelbrot calculation 
                    BitmapInfo bitmapInfo =
                        await Mandelbrot.CalculateAsync(center,
                                                        4 / Math.Pow(2, zoomLevel),
                                                        4 / Math.Pow(2, zoomLevel),
                                                        BITMAP_SIZE, BITMAP_SIZE,
                                                        (int)Math.Pow(2, 10), progressReporter, cancelToken);

                    // Create bitmap & get pointer to the pixel bits
                    SKBitmap bitmap = new SKBitmap(BITMAP_SIZE, BITMAP_SIZE, SKColorType.Rgba8888, SKAlphaType.Opaque);
                    IntPtr basePtr = bitmap.GetPixels();

                    // Set pixel bits to color based on iteration count
                    for (int row = 0; row < bitmap.Width; row++)
                        for (int col = 0; col < bitmap.Height; col++)
                        {
                            int iterationCount = bitmapInfo.IterationCounts[row * bitmap.Width + col];
                            uint pixel = 0xFF000000;            // black

                            if (iterationCount != -1)
                            {
                                double proportion = (iterationCount / 32.0) % 1;
                                byte red = 0, green = 0, blue = 0;

                                if (proportion < 0.5)
                                {
                                    red = (byte)(255 * (1 - 2 * proportion));
                                    blue = (byte)(255 * 2 * proportion);
                                }
                                else
                                {
                                    proportion = 2 * (proportion - 0.5);
                                    green = (byte)(255 * proportion);
                                    blue = (byte)(255 * (1 - proportion));
                                }

                                pixel = MakePixel(0xFF, red, green, blue);
                            }

                            // Calculate pointer to pixel
                            IntPtr pixelPtr = basePtr + 4 * (row * bitmap.Width + col);

                            unsafe     // requires compiling with unsafe flag
                            {
                                *(uint*)pixelPtr.ToPointer() = pixel;
                            }
                        }

                    // Save as PNG file
                    SKData data = SKImage.FromBitmap(bitmap).Encode();

                    try
                    {
                        File.WriteAllBytes(FilePath(zoomLevel), data.ToArray());
                    }
                    catch
                    {
                        // Probably out of space, but just ignore
                    }

                    // Store in array
                    bitmaps[zoomLevel] = bitmap;

                    // Show new bitmap sizes
                    TallyBitmapSizes();
                }

                // Display the bitmap
                bitmapIndex = zoomLevel;
                canvasView.InvalidateSurface();
            }

            // Now start the animation
            stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        bool OnTimerTick()
        {
            int cycle = 6000 * COUNT;       // total cycle length in milliseconds

            // Time in milliseconds from 0 to cycle
            int time = (int)(stopwatch.ElapsedMilliseconds % cycle);

            // Make it sinusoidal, including bitmap index and gradation between bitmaps
            double progress = COUNT * 0.5 * (1 + Math.Sin(2 * Math.PI * time / cycle - Math.PI / 2));

            // These are the field values that the PaintSurface handler uses
            bitmapIndex = (int)progress;
            bitmapProgress = progress - bitmapIndex;

            // It doesn't often happen that we get up to COUNT, but an exception would be raised
            if (bitmapIndex < COUNT)
            {
                // Show progress in UI
                statusLabel.Text = $"Displaying bitmap for zoom level {bitmapIndex}";
                progressBar.Progress = bitmapProgress;

                // Update the canvas
                canvasView.InvalidateSurface();
            }

            return true;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmaps[bitmapIndex] != null)
            {
                // Determine destination rect as square in canvas
                int dimension = Math.Min(info.Width, info.Height);
                float x = (info.Width - dimension) / 2;
                float y = (info.Height - dimension) / 2;
                SKRect destRect = new SKRect(x, y, x + dimension, y + dimension);

                // Calculate source rectangle based on fraction:
                //  bitmapProgress == 0: full bitmap
                //  bitmapProgress == 1: half of length and width of bitmap
                float fraction = 0.5f * (1 - (float)Math.Pow(2, -bitmapProgress));
                SKBitmap bitmap = bitmaps[bitmapIndex];
                int width = bitmap.Width;
                int height = bitmap.Height;
                SKRect sourceRect = new SKRect(fraction * width, fraction * height, 
                                               (1 - fraction) * width, (1 - fraction) * height);

                // Display the bitmap
                canvas.DrawBitmap(bitmap, sourceRect, destRect);
            }
        }

        void TallyBitmapSizes()
        {
            long fileSize = 0;

            foreach (string filename in Directory.EnumerateFiles(FolderPath()))
            {
                fileSize += new FileInfo(filename).Length;
            }

            storageLabel.Text = $"Total storage: {fileSize:N0} bytes";
        }

        void OnDeleteButtonClicked(object sender, EventArgs args)
        {
            foreach (string filepath in Directory.EnumerateFiles(FolderPath()))
            {
                File.Delete(filepath);
            }

            TallyBitmapSizes();
        }
    }
}

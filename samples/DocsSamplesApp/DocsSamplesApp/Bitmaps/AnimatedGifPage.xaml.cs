using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public partial class AnimatedGifPage : ContentPage
    {
        SKBitmap[]? bitmaps;
        int[]? durations;
        int[]? accumulatedDurations;
        int totalDuration;

        Stopwatch stopwatch = new Stopwatch();
        bool isAnimating;

        int currentFrame;

        public AnimatedGifPage()
        {
            InitializeComponent();

            _ = LoadGifAsync();
        }

        async Task LoadGifAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Newtons_cradle_animation_book_2.gif");
            using SKManagedStream skStream = new SKManagedStream(stream);
            using SKCodec codec = SKCodec.Create(skStream);

            // Get frame count and allocate bitmaps
            int frameCount = codec.FrameCount;
            bitmaps = new SKBitmap[frameCount];
            durations = new int[frameCount];
            accumulatedDurations = new int[frameCount];
            totalDuration = 0;

            // Note: There's also a RepetitionCount property of SKCodec not used here

            // Loop through the frames
            for (int frame = 0; frame < frameCount; frame++)
            {
                // From the FrameInfo collection, get the duration of each frame
                durations[frame] = codec.FrameInfo[frame].Duration;

                // Create a full-color bitmap for each frame
                SKImageInfo imageInfo = new SKImageInfo(codec.Info.Width, codec.Info.Height);
                bitmaps[frame] = new SKBitmap(imageInfo);

                // Get the address of the pixels in that bitmap
                IntPtr pointer = bitmaps[frame].GetPixels();

                // Create an SKCodecOptions value to specify the frame
                SKCodecOptions codecOptions = new SKCodecOptions(frame);

                // Copy pixels from the frame into the bitmap
                codec.GetPixels(imageInfo, pointer, codecOptions);
            }

            // Sum up the total duration
            for (int frame = 0; frame < durations.Length; frame++)
            {
                totalDuration += durations[frame];
            }

            // Calculate the accumulated durations 
            for (int frame = 0; frame < durations.Length; frame++)
            {
                accumulatedDurations[frame] = durations[frame] +
                    (frame == 0 ? 0 : accumulatedDurations[frame - 1]);
            }

            canvasView.InvalidateSurface();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            isAnimating = true;
            stopwatch.Start();
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            stopwatch.Stop();
            isAnimating = false;
        }

        bool OnTimerTick()
        {
            if (bitmaps is null || accumulatedDurations is null || totalDuration == 0)
                return isAnimating;
            int msec = (int)(stopwatch.ElapsedMilliseconds % totalDuration);
            int frame = 0;

            // Find the frame based on the elapsed time
            for (frame = 0; frame < accumulatedDurations.Length; frame++)
            {
                if (msec < accumulatedDurations[frame])
                {
                    break;
                }
            }

            // Save in a field and invalidate the SKCanvasView.
            if (currentFrame != frame)
            {
                currentFrame = frame;
                canvasView.InvalidateSurface();
            }

            return isAnimating;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            if (bitmaps is null)
                return;

            // Get the bitmap and center it
            SKBitmap bitmap = bitmaps[currentFrame];
            canvas.DrawBitmap(bitmap,info.Rect, BitmapStretch.Uniform);
        }
    }
}
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Media;

namespace DocsSamplesApp.Basics
{
    public class BasicBitmapsPage : ContentPage
    {
        SKCanvasView canvasView;

        HttpClient httpClient = new HttpClient();

        SKBitmap? webBitmap;
        SKBitmap? resourceBitmap;
        SKBitmap? libraryBitmap;

        public BasicBitmapsPage()
        {
            Title = "Basic Bitmaps";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            // Load resource bitmap
            _ = LoadResourceBitmapAsync();

            // Add tap gesture recognizer
            TapGestureRecognizer tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.Tapped += async (sender, args) =>
            {
                // Load bitmap from photo library using MAUI MediaPicker
                var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                {
                    SelectionLimit = 1,
                    Title = "Select a photo"
                });
                
                var photo = results.FirstOrDefault();
                if (photo != null)
                {
                    using Stream stream = await photo.OpenReadAsync();
                    libraryBitmap = SKBitmap.Decode(stream);
                    canvasView.InvalidateSurface();
                }
            };
            canvasView.GestureRecognizers.Add(tapRecognizer);
        }

        async Task LoadResourceBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("monkey.png");
            resourceBitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load web bitmap.
            string url = "https://developer.xamarin.com/demo/IMG_3256.JPG?width=480";

            try
            {
                using (Stream stream = await httpClient.GetStreamAsync(url))
                using (MemoryStream memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);

                    webBitmap = SKBitmap.Decode(memStream);
                    canvasView.InvalidateSurface();
                }
            }
            catch
            {
            }
        }
        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (webBitmap != null)
            {
                float x = (info.Width - webBitmap.Width) / 2;
                float y = (info.Height / 3 - webBitmap.Height) / 2;
                canvas.DrawBitmap(webBitmap, x, y);
            }

            if (resourceBitmap is not null)
            {
                canvas.DrawBitmap(resourceBitmap, 
                    new SKRect(0, info.Height / 3, info.Width, 2 * info.Height / 3));
            }

            if (libraryBitmap != null)
            {
                float scale = Math.Min((float)info.Width / libraryBitmap.Width,
                                       info.Height / 3f / libraryBitmap.Height);

                float left = (info.Width - scale * libraryBitmap.Width) / 2;
                float top = (info.Height / 3 - scale * libraryBitmap.Height) / 2;
                float right = left + scale * libraryBitmap.Width;
                float bottom = top + scale * libraryBitmap.Height;
                SKRect rect = new SKRect(left, top, right, bottom);
                rect.Offset(0, 2 * info.Height / 3);

                canvas.DrawBitmap(libraryBitmap, rect);
            }
            else
            {
                using (SKPaint paint = new SKPaint())
                using (SKFont font = new SKFont())
                {
                    paint.Color = SKColors.Blue;
                    font.Size = 48;

                    canvas.DrawText("Tap to load bitmap", 
                        info.Width / 2, 5 * info.Height / 6, SKTextAlign.Center, font, paint);
                }
            }
        }
    }
}
using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class PhotoPuzzlePage3 : ContentPage
    {
        PhotoCropperCanvasView photoCropper;

        public PhotoPuzzlePage3(SKBitmap bitmap)
        {
            InitializeComponent ();

            photoCropper = new PhotoCropperCanvasView(bitmap, 1f);
            canvasViewHost.Children.Add(photoCropper);
        }

        async void OnDoneButtonClicked(object sender, EventArgs args)
        {
            SKBitmap croppedBitmap = photoCropper.CroppedBitmap;
            int width = croppedBitmap.Width / 4;
            int height = croppedBitmap.Height / 4;

            ImageSource[] imgSources = new ImageSource[15];

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    // Skip the last one!
                    if (row == 3 && col == 3)
                        break;

                    // Create a bitmap 1/4 the width and height of the original
                    SKBitmap bitmap = new SKBitmap(width, height);
                    SKRect dest = new SKRect(0, 0, width, height);
                    SKRect source = new SKRect(col * width, row * height, (col + 1) * width, (row + 1) * height);

                    // Copy 1/16 of the original into that bitmap
                    using (SKCanvas canvas = new SKCanvas(bitmap))
                    {
                        canvas.DrawBitmap(croppedBitmap, source, dest);
                    }

                    imgSources[4 * row + col] = (SKBitmapImageSource)bitmap;
                }
            }

            await Navigation.PushAsync(new PhotoPuzzlePage4(imgSources));
        }
    }
}
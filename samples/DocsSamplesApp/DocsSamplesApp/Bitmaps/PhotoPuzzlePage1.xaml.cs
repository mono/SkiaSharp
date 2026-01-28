using System;
using System.IO;

using SkiaSharp;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Bitmaps
{
    public partial class PhotoPuzzlePage1 : ContentPage
    {
        public PhotoPuzzlePage1 ()
        {
            InitializeComponent ();
        }

        async void OnPickButtonClicked(object? sender, EventArgs args)
        {
            IPhotoLibrary photoLibrary = DependencyService.Get<IPhotoLibrary>();
            using (Stream stream = await photoLibrary.PickPhotoAsync())
            {
                if (stream != null)
                {
                    SKBitmap bitmap = SKBitmap.Decode(stream);

                    await Navigation.PushAsync(new PhotoPuzzlePage2(bitmap));
                }
            }
        }
    }
}
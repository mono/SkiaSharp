using System;
using System.IO;

using SkiaSharp;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Media;

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
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                SelectionLimit = 1,
                Title = "Select a photo for the puzzle"
            });
            
            var photo = results.FirstOrDefault();
            if (photo != null)
            {
                using Stream stream = await photo.OpenReadAsync();
                SKBitmap bitmap = SKBitmap.Decode(stream);
                await Navigation.PushAsync(new PhotoPuzzlePage2(bitmap));
            }
        }
    }
}
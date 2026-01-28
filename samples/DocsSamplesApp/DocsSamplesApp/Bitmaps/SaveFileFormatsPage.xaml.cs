using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public partial class SaveFileFormatsPage : ContentPage
    {
        SKBitmap? bitmap;

	    public SaveFileFormatsPage ()
	    {
		    InitializeComponent ();
            _ = LoadBitmapAsync();
	    }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MonkeyFace.png");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            if (bitmap is null)
                return;

            args.Surface.Canvas.DrawBitmap(bitmap, args.Info.Rect, BitmapStretch.Uniform);
        }

        void OnFormatPickerChanged(object? sender, EventArgs args)
        {
            if (formatPicker.SelectedIndex != -1)
            {
                SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
                fileNameEntry.Text = Path.ChangeExtension(fileNameEntry.Text, imageFormat.ToString());
                statusLabel.Text = "OK";
            }
        }

        async void OnButtonClicked(object? sender, EventArgs args)
        {
            if (bitmap is null)
                return;
            SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
            int quality = (int)qualitySlider.Value;

            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                bitmap.Encode(wstream, imageFormat, quality);
                byte[] data = memStream.ToArray();

                if (data == null)
                {
                    statusLabel.Text = "Encode returned null";
                }
                else if (data.Length == 0)
                {
                    statusLabel.Text = "Encode returned empty array";
                }
                else
                {
                    using var stream = new MemoryStream(data);
                    var result = await FileSaver.Default.SaveAsync(fileNameEntry.Text, stream, CancellationToken.None);

                    if (!result.IsSuccessful)
                    {
                        statusLabel.Text = "Save failed: " + result.Exception?.Message;
                    }
                    else
                    {
                        statusLabel.Text = "Success! Saved to: " + result.FilePath;
                    }
                }
            }
        }
    }
}
using System;
using System.IO;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class SaveFileFormatsPage : ContentPage
    {
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(typeof(SaveFileFormatsPage),
            "SkiaSharpFormsDemos.Media.MonkeyFace.png");

	    public SaveFileFormatsPage ()
	    {
		    InitializeComponent ();
	    }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.DrawBitmap(bitmap, args.Info.Rect, BitmapStretch.Uniform);
        }

        void OnFormatPickerChanged(object sender, EventArgs args)
        {
            if (formatPicker.SelectedIndex != -1)
            {
                SKEncodedImageFormat imageFormat = (SKEncodedImageFormat)formatPicker.SelectedItem;
                fileNameEntry.Text = Path.ChangeExtension(fileNameEntry.Text, imageFormat.ToString());
                statusLabel.Text = "OK";
            }
        }

        async void OnButtonClicked(object sender, EventArgs args)
        {
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
                    bool success = await DependencyService.Get<IPhotoLibrary>().
                        SavePhotoAsync(data, folderNameEntry.Text, fileNameEntry.Text);

                    if (!success)
                    {
                        statusLabel.Text = "SavePhotoAsync return false";
                    }
                    else
                    {
                        statusLabel.Text = "Success!";
                    }
                }
            }
        }
    }
}
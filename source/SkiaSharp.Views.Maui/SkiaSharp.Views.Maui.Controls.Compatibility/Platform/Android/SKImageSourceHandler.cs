using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using SkiaSharp.Views.Android;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			Bitmap bitmap = null;

			var imageImageSource = imagesource as SKImageImageSource;
			if (imageImageSource != null)
			{
				bitmap = imageImageSource.Image?.ToBitmap();
			}

			var bitmapImageSource = imagesource as SKBitmapImageSource;
			if (bitmapImageSource != null)
			{
				bitmap = bitmapImageSource.Bitmap?.ToBitmap();
			}

			var pixmapImageSource = imagesource as SKPixmapImageSource;
			if (pixmapImageSource != null)
			{
				bitmap = pixmapImageSource.Pixmap?.ToBitmap();
			}

			var pictureImageSource = imagesource as SKPictureImageSource;
			if (pictureImageSource != null)
			{
				bitmap = pictureImageSource.Picture?.ToBitmap(pictureImageSource.Dimensions);
			}

			return Task.FromResult(bitmap);
		}
	}
}

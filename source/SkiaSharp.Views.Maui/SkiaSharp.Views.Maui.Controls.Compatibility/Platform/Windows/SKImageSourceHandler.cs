using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using SkiaSharp.Views.Windows;

using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<WImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken))
		{
			WImageSource image = null;

			var imageImageSource = imagesource as SKImageImageSource;
			if (imageImageSource != null)
			{
				image = imageImageSource.Image?.ToWriteableBitmap();
			}

			var bitmapImageSource = imagesource as SKBitmapImageSource;
			if (bitmapImageSource != null)
			{
				image = bitmapImageSource.Bitmap?.ToWriteableBitmap();
			}

			var pixmapImageSource = imagesource as SKPixmapImageSource;
			if (pixmapImageSource != null)
			{
				image = pixmapImageSource.Pixmap?.ToWriteableBitmap();
			}

			var pictureImageSource = imagesource as SKPictureImageSource;
			if (pictureImageSource != null)
			{
				image = pictureImageSource.Picture?.ToWriteableBitmap(pictureImageSource.Dimensions);
			}

			return Task.FromResult(image);
		}
	}
}

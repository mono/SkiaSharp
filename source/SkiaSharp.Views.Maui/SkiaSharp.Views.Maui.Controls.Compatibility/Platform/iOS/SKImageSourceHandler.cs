using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using SkiaSharp.Views.iOS;
using UIKit;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			UIImage image = null;

			var imageImageSource = imagesource as SKImageImageSource;
			if (imageImageSource != null)
			{
				image = imageImageSource.Image?.ToUIImage();
			}

			var bitmapImageSource = imagesource as SKBitmapImageSource;
			if (bitmapImageSource != null)
			{
				image = bitmapImageSource.Bitmap?.ToUIImage();
			}

			var pixmapImageSource = imagesource as SKPixmapImageSource;
			if (pixmapImageSource != null)
			{
				image = pixmapImageSource.Pixmap?.ToUIImage();
			}

			var pictureImageSource = imagesource as SKPictureImageSource;
			if (pictureImageSource != null)
			{
				image = pictureImageSource.Picture?.ToUIImage(pictureImageSource.Dimensions);
			}

			return Task.FromResult(image);
		}
	}
}

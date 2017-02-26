using System.Threading;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using SkiaSharp.Views.iOS;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]

namespace SkiaSharp.Views.Forms
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

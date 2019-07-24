using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

using SkiaSharp.Views.Mac;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]

namespace SkiaSharp.Views.Forms
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<NSImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			NSImage image = null;

			var imageImageSource = imagesource as SKImageImageSource;
			if (imageImageSource != null)
			{
                image = imageImageSource.Image?.ToNSImage();
			}

			var bitmapImageSource = imagesource as SKBitmapImageSource;
			if (bitmapImageSource != null)
			{
				image = bitmapImageSource.Bitmap?.ToNSImage();
			}

			var pixmapImageSource = imagesource as SKPixmapImageSource;
			if (pixmapImageSource != null)
			{
				image = pixmapImageSource.Pixmap?.ToNSImage();
			}

			var pictureImageSource = imagesource as SKPictureImageSource;
			if (pictureImageSource != null)
			{
				image = pictureImageSource.Picture?.ToNSImage(pictureImageSource.Dimensions);
			}

			return Task.FromResult(image);
		}
	}
}

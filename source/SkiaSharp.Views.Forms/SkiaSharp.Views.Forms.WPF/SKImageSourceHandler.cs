using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Views.WPF;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]

namespace SkiaSharp.Views.Forms
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<System.Windows.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken))
		{
			System.Windows.Media.ImageSource image = null;

			if (imagesource is SKImageImageSource imageImageSource)
				image = imageImageSource.Image?.ToWriteableBitmap();
			else if (imagesource is SKBitmapImageSource bitmapImageSource)
				image = bitmapImageSource.Bitmap?.ToWriteableBitmap();
			else if (imagesource is SKPixmapImageSource pixmapImageSource)
				image = pixmapImageSource.Pixmap?.ToWriteableBitmap();
			else if (imagesource is SKPictureImageSource pictureImageSource)
				image = pictureImageSource.Picture?.ToWriteableBitmap(pictureImageSource.Dimensions);

			return Task.FromResult(image);
		}
	}
}

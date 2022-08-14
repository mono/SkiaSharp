using System.Threading;
using System.Threading.Tasks;

#if __MAUI__
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;

using SkiaSharp.Views.Windows;

using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
#else
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using SkiaSharp.Views.UWP;

using WImageSource = Windows.UI.Xaml.Media.ImageSource;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
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

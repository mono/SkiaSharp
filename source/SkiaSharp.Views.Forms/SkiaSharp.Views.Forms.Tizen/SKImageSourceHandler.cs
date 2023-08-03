using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ElmImage = ElmSharp.Image;

#if __MAUI__
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#else
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

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
		private readonly StreamImageSourceHandler handler = new StreamImageSourceHandler();

		public Task<bool> LoadImageAsync(ElmImage image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			ImageSource newSource = null;

			switch (imageSource)
			{
				case SKImageImageSource imageImageSource:
					newSource = ImageSource.FromStream(() => ToStream(imageImageSource.Image));
					break;
				case SKBitmapImageSource bitmapImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromBitmap(bitmapImageSource.Bitmap)));
					break;
				case SKPixmapImageSource pixmapImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromPixels(pixmapImageSource.Pixmap)));
					break;
				case SKPictureImageSource pictureImageSource:
					newSource = ImageSource.FromStream(() => ToStream(SKImage.FromPicture(pictureImageSource.Picture, pictureImageSource.Dimensions)));
					break;
			}

			return handler.LoadImageAsync(image, newSource, cancelationToken);
		}

		private static Stream ToStream(SKImage skiaImage)
		{
			return skiaImage.Encode().AsStream();
		}
	}
}

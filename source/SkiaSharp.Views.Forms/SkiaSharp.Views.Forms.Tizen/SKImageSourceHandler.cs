using SkiaSharp.Views.Tizen;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKBitmapImageSourceeHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKPixmapImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKPictureImageSourceHandler))]

namespace SkiaSharp.Views.Forms
{
	public abstract class SKImageSourceHandler : IImageSourceHandler
	{
		private StreamImageSourceHandler handler = new StreamImageSourceHandler();

		public Task<bool> LoadImageAsync(Xamarin.Forms.Platform.Tizen.Native.Image image, ImageSource imageSource, CancellationToken cancelationToken = default(CancellationToken))
		{
			return handler.LoadImageAsync(image, ImageSource.FromStream(() => ToStream(imageSource)), cancelationToken);
		}

		protected abstract Stream ToStream(ImageSource imageSource);
	}

	public sealed class SKImageImageSourceHandler : SKImageSourceHandler
	{
		protected override Stream ToStream(ImageSource imageSource)
		{
			return (imageSource as SKImageImageSource)?.Image?.ToStream();
		}
	}

	public sealed class SKBitmapImageSourceeHandler : SKImageSourceHandler
	{
		protected override Stream ToStream(ImageSource imageSource)
		{
			return (imageSource as SKBitmapImageSource)?.Bitmap?.ToStream();
		}
	}

	public sealed class SKPixmapImageSourceHandler : SKImageSourceHandler
	{
		protected override Stream ToStream(ImageSource imageSource)
		{
			return (imageSource as SKPixmapImageSource)?.Pixmap?.ToStream();
		}
	}

	public sealed class SKPictureImageSourceHandler : SKImageSourceHandler
	{
		protected override Stream ToStream(ImageSource imageSource)
		{
			var pictureImageSource = imageSource as SKPictureImageSource;

			if (pictureImageSource != null)
			{
				return pictureImageSource.Picture?.ToStream(pictureImageSource.Dimensions);
			}
			else
			{
				return null;
			}
		}
	}
}

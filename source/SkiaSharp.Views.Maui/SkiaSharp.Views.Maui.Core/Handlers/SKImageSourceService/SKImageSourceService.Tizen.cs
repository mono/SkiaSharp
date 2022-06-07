using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Tizen.UIExtensions.ElmSharp;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		public override Task<IImageSourceServiceResult<Image>?> GetImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<Image>?> GetImageAsync(IStreamImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = imageSource switch
				{
					ISKImageImageSource img => ToStream(img.Image),
					ISKBitmapImageSource bmp => ToStream(SKImage.FromBitmap(bmp.Bitmap)),
					ISKPixmapImageSource pix => ToStream(SKImage.FromPixels(pix.Pixmap)),
					ISKPictureImageSource pic => ToStream(SKImage.FromPicture(pic.Picture, pic.Dimensions)),
					_ => null,
				};

				if (stream == null)
					throw new InvalidOperationException("Unable to load image stream.");

				var isLoadComplated = await image.LoadAsync(stream, cancellationToken);

				if (!isLoadComplated)
					throw new InvalidOperationException("Unable to decode image from stream.");

				return new ImageSourceServiceResult(image);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				throw;
			}
		}

		private static Stream ToStream(SKImage skiaImage)
		{
			return skiaImage.Encode().AsStream();
		}
	}
}

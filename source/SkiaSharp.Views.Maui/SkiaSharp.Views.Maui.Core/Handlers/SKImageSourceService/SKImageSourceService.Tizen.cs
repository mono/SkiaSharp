using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		public override Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(
			IImageSource imageSource,
			CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, cancellationToken);

		public async Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
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

				var image = new MauiImageSource();
				await image.LoadSource(stream);

				return new ImageSourceServiceResult(image, image.Dispose);

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

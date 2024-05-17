using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using SkiaSharp.Views.iOS;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			var image = imageSource switch
			{
				ISKImageImageSource img => img.Image?.ToUIImage(),
				ISKBitmapImageSource bmp => bmp.Bitmap?.ToUIImage(),
				ISKPixmapImageSource pix => pix.Pixmap?.ToUIImage(),
				ISKPictureImageSource pic => pic.Picture?.ToUIImage(pic.Dimensions),
				_ => null,
			};

			return image != null
				? FromResult(new ImageSourceServiceResult(image, () => image.Dispose()))
				: FromResult(null);
		}

		private static Task<IImageSourceServiceResult<UIImage>?> FromResult(ImageSourceServiceResult? result) =>
			Task.FromResult<IImageSourceServiceResult<UIImage>?>(result);
	}
}

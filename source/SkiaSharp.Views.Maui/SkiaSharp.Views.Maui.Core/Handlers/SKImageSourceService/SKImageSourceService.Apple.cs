using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using SkiaSharp.Views.iOS;
using UIKit;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		/// <summary>Asynchronously obtains a platform image for the specified image source.</summary>
		/// <param name="imageSource">The image source to convert.</param>
		/// <param name="scale">The display scale to use.</param>
		/// <param name="cancellationToken">The token used to cancel the operation.</param>
		/// <returns>A result containing the platform image, or <see langword="null" /> if no image is available.</returns>
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

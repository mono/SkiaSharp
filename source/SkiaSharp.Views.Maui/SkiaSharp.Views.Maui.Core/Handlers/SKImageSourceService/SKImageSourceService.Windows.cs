using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using SkiaSharp.Views.Windows;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			var bitmap = imageSource switch
			{
				ISKImageImageSource img => img.Image?.ToWriteableBitmap(),
				ISKBitmapImageSource bmp => bmp.Bitmap?.ToWriteableBitmap(),
				ISKPixmapImageSource pix => pix.Pixmap?.ToWriteableBitmap(),
				ISKPictureImageSource pic => pic.Picture?.ToWriteableBitmap(pic.Dimensions),
				_ => null,
			};

			return bitmap != null
				? FromResult(new ImageSourceServiceResult(bitmap))
				: FromResult(null);
		}

		private static Task<IImageSourceServiceResult<WImageSource>?> FromResult(ImageSourceServiceResult? result) =>
			Task.FromResult<IImageSourceServiceResult<WImageSource>?>(result);
	}
}

using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Microsoft.Maui;
using SkiaSharp.Views.Android;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKImageSourceService
	{
		public override async Task<IImageSourceServiceResult?> LoadDrawableAsync(
			IImageSource imageSource,
			global::Android.Widget.ImageView imageView,
			CancellationToken cancellationToken = default)
		{
			var realResult = await GetDrawableAsync(imageView.Context!, imageSource, cancellationToken);

			if (realResult is null)
			{
				imageView.SetImageDrawable(null);
				return null;
			}

			imageView.SetImageDrawable(realResult.Value);

			var result = new ImageSourceServiceLoadResult(
				realResult.IsResolutionDependent,
				() => realResult.Dispose());

			return result;
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(Context context, IImageSource imageSource, CancellationToken cancellationToken = default)
		{
			var bitmap = imageSource switch
			{
				ISKImageImageSource img => img.Image?.ToBitmap(),
				ISKBitmapImageSource bmp => bmp.Bitmap?.ToBitmap(),
				ISKPixmapImageSource pix => pix.Pixmap?.ToBitmap(),
				ISKPictureImageSource pic => pic.Picture?.ToBitmap(pic.Dimensions),
				_ => null,
			};

			return bitmap != null
				? FromResult(new ImageSourceServiceResult(new BitmapDrawable(context.Resources, bitmap), () => bitmap.Dispose()))
				: FromResult(null);
		}

		private static Task<IImageSourceServiceResult<Drawable>?> FromResult(ImageSourceServiceResult? result) =>
			Task.FromResult<IImageSourceServiceResult<Drawable>?>(result);
	}
}

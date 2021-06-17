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
		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
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

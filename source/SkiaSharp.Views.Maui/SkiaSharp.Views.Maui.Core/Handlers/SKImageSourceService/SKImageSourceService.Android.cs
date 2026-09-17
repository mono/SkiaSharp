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
		/// <summary>Gets an Android drawable for the specified image source.</summary>
		/// <param name="imageSource">The image source to convert.</param>
		/// <param name="context">The Android context used to create the drawable.</param>
		/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
		/// <returns>A task whose result contains the drawable, or <see langword="null" /> when the image source has no drawable representation.</returns>
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

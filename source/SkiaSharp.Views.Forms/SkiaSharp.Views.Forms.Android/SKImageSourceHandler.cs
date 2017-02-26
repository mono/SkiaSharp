using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using SkiaSharp.Views.Android;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]

namespace SkiaSharp.Views.Forms
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			Bitmap bitmap = null;

			var imageImageSource = imagesource as SKImageImageSource;
			if (imageImageSource != null)
			{
				bitmap = imageImageSource.Image?.ToBitmap();
			}

			var bitmapImageSource = imagesource as SKBitmapImageSource;
			if (bitmapImageSource != null)
			{
				bitmap = bitmapImageSource.Bitmap?.ToBitmap();
			}

			var pixmapImageSource = imagesource as SKPixmapImageSource;
			if (pixmapImageSource != null)
			{
				bitmap = pixmapImageSource.Pixmap?.ToBitmap();
			}

			var pictureImageSource = imagesource as SKPictureImageSource;
			if (pictureImageSource != null)
			{
				bitmap = pictureImageSource.Picture?.ToBitmap(pictureImageSource.Dimensions);
			}

			return Task.FromResult(bitmap);
		}
	}
}

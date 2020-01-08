using System.Threading;
using System.Threading.Tasks;
using Gdk;
using SkiaSharp.Views.Gtk;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKImageImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKBitmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPixmapImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]
[assembly: ExportImageSourceHandler(typeof(SkiaSharp.Views.Forms.SKPictureImageSource), typeof(SkiaSharp.Views.Forms.SKImageSourceHandler))]

namespace SkiaSharp.Views.Forms
{
	public sealed class SKImageSourceHandler : IImageSourceHandler
	{
		public Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1f)
		{
			Pixbuf image = null;

			if (imagesource is SKImageImageSource imageImageSource)
				image = imageImageSource.Image?.ToPixbuf();
			else if (imagesource is SKBitmapImageSource bitmapImageSource)
				image = bitmapImageSource.Bitmap?.ToPixbuf();
			else if (imagesource is SKPixmapImageSource pixmapImageSource)
				image = pixmapImageSource.Pixmap?.ToPixbuf();
			else if (imagesource is SKPictureImageSource pictureImageSource)
				image = pictureImageSource.Picture?.ToPixbuf(pictureImageSource.Dimensions);

			return Task.FromResult(image);
		}
	}
}

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	public interface ISKImageImageSource : IImageSource
	{
		SKImage Image { get; }
	}

	public interface ISKBitmapImageSource : IImageSource
	{
		SKBitmap Bitmap { get; }
	}

	public interface ISKPixmapImageSource : IImageSource
	{
		SKPixmap Pixmap { get; }
	}

	public interface ISKPictureImageSource : IImageSource
	{
		SKPicture Picture { get; }

		SKSizeI Dimensions { get; }
	}
}

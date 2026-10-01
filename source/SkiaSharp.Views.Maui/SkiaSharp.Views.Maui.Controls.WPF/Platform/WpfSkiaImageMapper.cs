using System;
using System.Windows.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers.WPF;
using SkiaSharp.Views.WPF;
using WpfImage = System.Windows.Controls.Image;
using WpfStretch = System.Windows.Media.Stretch;

namespace SkiaSharp.Views.Maui.Controls.WPF.Platform;

internal static class WpfSkiaImageMapper
{
	private static readonly object sync = new();
	private static bool registered;

	public static void Register()
	{
		lock (sync)
		{
			if (registered)
				return;

			if (ImageHandler.Mapper is not PropertyMapper<IImage, ImageHandler> imageMapper)
				throw new InvalidOperationException("The WPF image handler's property mapper cannot be extended.");

			var mapImage = imageMapper[nameof(IImage.Source)];
			var mapButton = ImageButtonHandler.Mapper[nameof(ImageButton.Source)];

			imageMapper[nameof(IImage.Source)] = (handler, image) =>
			{
				if (TryCreateBitmap(image.Source, out var bitmap))
					handler.PlatformView.Source = bitmap;
				else
					mapImage(handler, image);
			};

			ImageButtonHandler.Mapper[nameof(ImageButton.Source)] = (handler, button) =>
			{
				if (TryCreateBitmap(button.Source, out var bitmap))
				{
					handler.PlatformView.Content = bitmap is null
						? null
						: new WpfImage
						{
							Source = bitmap,
							Stretch = button.Aspect switch
							{
								Aspect.Fill => WpfStretch.Fill,
								Aspect.AspectFill => WpfStretch.UniformToFill,
								_ => WpfStretch.Uniform,
							},
						};
				}
				else
				{
					mapButton(handler, button);
				}
			};

			registered = true;
		}
	}

	internal static bool TryCreateBitmap(IImageSource? source, out WriteableBitmap? bitmap)
	{
		bitmap = source switch
		{
			ISKImageImageSource image => image.Image?.ToWriteableBitmap(),
			ISKBitmapImageSource skBitmap => skBitmap.Bitmap?.ToWriteableBitmap(),
			ISKPixmapImageSource pixmap => pixmap.Pixmap?.ToWriteableBitmap(),
			ISKPictureImageSource picture when picture.Picture is null => null,
			ISKPictureImageSource picture when picture.Dimensions.Width > 0 && picture.Dimensions.Height > 0 =>
				picture.Picture.ToWriteableBitmap(picture.Dimensions),
			ISKPictureImageSource => throw new ArgumentOutOfRangeException(nameof(source), "Picture dimensions must be positive."),
			_ => null,
		};

		return source is ISKImageImageSource or ISKBitmapImageSource or ISKPixmapImageSource or ISKPictureImageSource;
	}
}

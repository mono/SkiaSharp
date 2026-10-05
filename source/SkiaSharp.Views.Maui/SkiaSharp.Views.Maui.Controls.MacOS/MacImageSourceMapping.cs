using System;
using System.Threading;
using AppKit;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Platforms.MacOS.Handlers;
using ImageHandler = Microsoft.Maui.Platforms.MacOS.Handlers.ImageHandler;
using ImageButtonHandler = Microsoft.Maui.Platforms.MacOS.Handlers.ImageButtonHandler;

namespace SkiaSharp.Views.Maui.Handlers
{
	internal static class MacImageSourceMapping
	{
		private static int registered;

		public static void Register()
		{
			if (Interlocked.Exchange(ref registered, 1) != 0)
				return;

			ImageHandler.Mapper.AppendToMapping(nameof(IImage.Source), (handler, view) =>
			{
				if (view.Source is not ISKImageImageSource and not ISKBitmapImageSource and
					not ISKPixmapImageSource and not ISKPictureImageSource)
					return;

				using var image = ToNSImage(view.Source);
				handler.PlatformView.Image = image;
				handler.PlatformView.InvalidateIntrinsicContentSize();
			});
			ImageButtonHandler.Mapper.AppendToMapping(nameof(IImage.Source), (handler, view) =>
			{
				if (view.Source is null)
				{
					handler.PlatformView.Image = null;
					handler.PlatformView.InvalidateIntrinsicContentSize();
					return;
				}
				if (view.Source is not ISKImageImageSource and not ISKBitmapImageSource and
					not ISKPixmapImageSource and not ISKPictureImageSource)
					return;

				using var image = ToNSImage(view.Source);
				handler.PlatformView.Image = image;
				handler.PlatformView.InvalidateIntrinsicContentSize();
			});
		}

		private static NSImage? ToNSImage(IImageSource source)
		{
			if (source is ISKImageImageSource { Image: { } original })
			{
				var raster = original.ToRasterImage();
				try
				{
					return raster is null ? null : Encode(raster);
				}
				finally
				{
					if (!ReferenceEquals(raster, original))
						raster?.Dispose();
				}
			}

			using var image = source switch
			{
				ISKBitmapImageSource { Bitmap: { } bitmap } => SKImage.FromBitmap(bitmap),
				ISKPixmapImageSource { Pixmap: { } pixmap } => SKImage.FromPixels(pixmap),
				ISKPictureImageSource { Picture: { } picture, Dimensions: var size }
					when size.Width > 0 && size.Height > 0 => SKImage.FromPicture(picture, size),
				ISKPictureImageSource { Picture: not null } =>
					throw new ArgumentOutOfRangeException(nameof(ISKPictureImageSource.Dimensions)),
				_ => null,
			};
			return image is null ? null : Encode(image);
		}

		private static NSImage Encode(SKImage image)
		{
			using var data = image.Encode(SKEncodedImageFormat.Png, 100)
				?? throw new InvalidOperationException("Unable to encode the SkiaSharp image for AppKit.");
			using var bytes = NSData.FromBytes(data.Data, (nuint)data.Size);
			return new NSImage(bytes);
		}
	}
}
